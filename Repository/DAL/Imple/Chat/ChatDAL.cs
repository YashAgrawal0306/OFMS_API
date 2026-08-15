using Dapper;
using DTO.Models.Chat;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Repository.DAL.Interface.Chat;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.DAL.Imple.Chat
{
    public class ChatDAL : IChatDAL
    {
        private readonly string _conn;
        public ChatDAL(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection") ?? "";
        }

        // ─────────────────────────────────────────────────────────────
        // Role-based communication matrix (who can chat with whom)
        // Admin(1)=all | Manager(2)=Cook(3),Delivery(4),Customer(6)
        // Cook(3)=Manager(2) | Delivery(4)=Manager(2),Customer(6)
        // Customer(6)=Manager(2),Delivery(4)
        // ─────────────────────────────────────────────────────────────
        private static readonly Dictionary<int, int[]> _allowedRoles = new()
        {
            { 1, new[] { 1, 2, 3, 4, 5, 6 } }, // Admin talks to everyone
            { 2, new[] { 1, 2, 3, 4, 6 } },     // Manager
            { 3, new[] { 1, 2 } },               // Cook → Admin, Manager
            { 4, new[] { 1, 2, 6 } },            // Delivery → Admin, Manager, Customer
            { 5, new[] { 1, 2 } },               // Other Staff → Admin, Manager
            { 6, new[] { 1, 2, 4 } },            // Customer → Admin, Manager, Delivery
        };

        // ── USERS ─────────────────────────────────────────────────────

        public async Task<List<ChatUserTO>> GetChatUsersAsync(int currentUserId, string? searchText = null)
        {
            using var conn = new SqlConnection(_conn);

            // Get current user's role
            var myRole = await conn.ExecuteScalarAsync<int?>(
                "SELECT TOP 1 RoleId FROM tblUserRoleMapping WHERE UserId = @uid AND IsActive = 1",
                new { uid = currentUserId }) ?? 1;

            int[] allowed = _allowedRoles.ContainsKey(myRole) ? _allowedRoles[myRole] : new[] { 1 };

            string sql = @"
                SELECT u.UserId, u.UserName, u.UserEmail, r.RoleName, r.RoleId,
                       u.Profile_Image AS ProfileImage
                FROM tblUser u
                INNER JOIN tblUserRoleMapping m ON m.UserId = u.UserId AND m.IsActive = 1
                INNER JOIN tblRoles r            ON r.RoleId = m.RoleId
                WHERE u.IsActive = 1
                  AND u.UserId != @currentUserId
                  AND m.RoleId IN @allowedRoles
                  AND (@search IS NULL OR u.UserName LIKE @search OR u.UserEmail LIKE @search)
                ORDER BY u.UserName";

            var list = (await conn.QueryAsync<ChatUserTO>(sql, new
            {
                currentUserId,
                allowedRoles = allowed,
                search = string.IsNullOrEmpty(searchText) ? null : $"%{searchText}%"
            })).ToList();

            return list;
        }

        // ── CONVERSATIONS ─────────────────────────────────────────────

        public async Task<List<ConversationListItemTO>> GetUserConversationsAsync(int userId)
        {
            using var conn = new SqlConnection(_conn);

            string sql = @"
                SELECT * FROM (
                    SELECT
                        c.IdConversation,
                        c.ConversationType,
                        c.ConversationName AS Name,
                        c.ConversationDescription AS Description,
                        c.IdOrderMaster,
                        -- last message text
                        (SELECT TOP 1 CASE WHEN m.IsDeleted=1 THEN '🚫 Message deleted'
                                           ELSE m.MessageText END
                         FROM tblChatMessage m
                         WHERE m.IdConversation = c.IdConversation
                         ORDER BY m.SentOn DESC) AS LastMessage,
                        -- last message time
                        (SELECT TOP 1 m.SentOn
                         FROM tblChatMessage m
                         WHERE m.IdConversation = c.IdConversation
                         ORDER BY m.SentOn DESC) AS LastMessageTime,
                        -- unread count
                        (SELECT COUNT(*)
                         FROM tblChatMessage msg
                         WHERE msg.IdConversation = c.IdConversation
                           AND msg.IsDeleted = 0
                           AND msg.SenderUserId != @userId
                           AND msg.IdMessage > ISNULL(mem.LastReadMessageId, 0)) AS UnreadCount,
                        -- member count (for groups)
                        (SELECT COUNT(*) FROM tblChatConversationMember x
                         WHERE x.IdConversation = c.IdConversation AND x.IsActive = 1) AS MemberCount
                    FROM tblChatConversation c
                    INNER JOIN tblChatConversationMember mem
                        ON mem.IdConversation = c.IdConversation AND mem.UserId = @userId AND mem.IsActive = 1
                    WHERE c.IsDeleted = 0
                ) AS convs
                ORDER BY ISNULL(convs.LastMessageTime, '1900-01-01') DESC, convs.IdConversation DESC";

            var convs = (await conn.QueryAsync<ConversationListItemTO>(sql, new { userId })).ToList();

            // For personal chats, replace ConversationName with the OTHER user's name
            foreach (var c in convs.Where(x => x.ConversationType == 1))
            {
                var other = await conn.QueryFirstOrDefaultAsync<ChatUserTO>(@"
                    SELECT u.UserId, u.UserName, u.Profile_Image AS ProfileImage, r.RoleName
                    FROM tblChatConversationMember m
                    INNER JOIN tblUser u ON u.UserId = m.UserId
                    LEFT JOIN tblUserRoleMapping rm ON rm.UserId = u.UserId AND rm.IsActive = 1
                    LEFT JOIN tblRoles r ON r.RoleId = rm.RoleId
                    WHERE m.IdConversation = @cid AND m.UserId != @uid AND m.IsActive = 1",
                    new { cid = c.IdConversation, uid = userId });

                if (other != null)
                {
                    c.Name = other.UserName;
                    c.AvatarUrl = other.ProfileImage;
                    c.OtherUserId = other.UserId;
                }
            }

            return convs;
        }

        public async Task<ConversationListItemTO?> GetConversationByIdAsync(int conversationId, int userId)
        {
            var list = await GetUserConversationsAsync(userId);
            return list.FirstOrDefault(c => c.IdConversation == conversationId);
        }

        public async Task<bool> IsUserMemberAsync(int conversationId, int userId)
        {
            using var conn = new SqlConnection(_conn);
            int cnt = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM tblChatConversationMember WHERE IdConversation=@c AND UserId=@u AND IsActive=1",
                new { c = conversationId, u = userId });
            return cnt > 0;
        }

        // ── PERSONAL CHAT ─────────────────────────────────────────────

        public async Task<int> GetOrCreatePersonalConversationAsync(int userId1, int userId2)
        {
            using var conn = new SqlConnection(_conn);
            await conn.OpenAsync();

            // Check for existing personal conversation between these two users
            int existing = await conn.ExecuteScalarAsync<int>(@"
                SELECT TOP 1 c.IdConversation
                FROM tblChatConversation c
                WHERE c.ConversationType = 1 AND c.IsDeleted = 0
                  AND EXISTS (SELECT 1 FROM tblChatConversationMember m1
                              WHERE m1.IdConversation=c.IdConversation AND m1.UserId=@u1 AND m1.IsActive=1)
                  AND EXISTS (SELECT 1 FROM tblChatConversationMember m2
                              WHERE m2.IdConversation=c.IdConversation AND m2.UserId=@u2 AND m2.IsActive=1)",
                new { u1 = userId1, u2 = userId2 });

            if (existing > 0) return existing;

            // Create new personal conversation
            using var tran = conn.BeginTransaction();
            try
            {
                int convId = await conn.ExecuteScalarAsync<int>(@"
                    INSERT INTO tblChatConversation (ConversationType,CreatedBy,CreatedOn,IsActive,IsDeleted)
                    VALUES (1,@cb,GETDATE(),1,0);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    new { cb = userId1 }, tran);

                await conn.ExecuteAsync(@"
                    INSERT INTO tblChatConversationMember (IdConversation,UserId,MemberRole,JoinedOn,IsActive)
                    VALUES (@c,@u,1,GETDATE(),1)",
                    new { c = convId, u = userId1 }, tran);

                await conn.ExecuteAsync(@"
                    INSERT INTO tblChatConversationMember (IdConversation,UserId,MemberRole,JoinedOn,IsActive)
                    VALUES (@c,@u,1,GETDATE(),1)",
                    new { c = convId, u = userId2 }, tran);

                tran.Commit();
                return convId;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        // ── MESSAGES ─────────────────────────────────────────────────

        public async Task<List<MessageListItemTO>> GetMessagesAsync(
            int conversationId, int callerUserId, int pageNumber, int pageSize)
        {
            using var conn = new SqlConnection(_conn);

            string sql = @"
                SELECT
                    m.IdMessage, m.IdConversation, m.SenderUserId,
                    u.UserName AS SenderName,
                    u.Profile_Image AS SenderAvatar,
                    CASE WHEN m.IsDeleted=1 THEN NULL ELSE m.MessageText END AS MessageText,
                    m.MessageType, m.ReplyToMessageId,
                    (SELECT TOP 1 CASE WHEN rp.IsDeleted=1 THEN '🚫 Deleted' ELSE rp.MessageText END
                     FROM tblChatMessage rp WHERE rp.IdMessage=m.ReplyToMessageId) AS ReplyToText,
                    (SELECT TOP 1 ru.UserName FROM tblChatMessage rp
                     INNER JOIN tblUser ru ON ru.UserId=rp.SenderUserId
                     WHERE rp.IdMessage=m.ReplyToMessageId) AS ReplyToSender,
                    m.AttachmentPath, m.AttachmentName,
                    m.SentOn, m.IsEdited, m.IsDeleted,
                    (CASE WHEN EXISTS (
                        SELECT 1 FROM tblChatConversationMember mem
                        WHERE mem.IdConversation = m.IdConversation
                          AND mem.UserId != m.SenderUserId
                          AND mem.IsActive = 1
                          AND mem.LastReadMessageId >= m.IdMessage
                    ) THEN 1 ELSE 0 END) AS IsRead
                FROM tblChatMessage m
                INNER JOIN tblUser u ON u.UserId = m.SenderUserId
                WHERE m.IdConversation = @cid
                ORDER BY m.SentOn DESC
                OFFSET @offset ROWS FETCH NEXT @size ROWS ONLY";

            int offset = (pageNumber - 1) * pageSize;
            var msgs = (await conn.QueryAsync<MessageListItemTO>(sql,
                new { cid = conversationId, offset, size = pageSize }))
                .Reverse()
                .ToList();

            foreach (var msg in msgs)
                msg.IsOwnMessage = msg.SenderUserId == callerUserId;

            return msgs;
        }

        public async Task<MessageListItemTO?> SendMessageAsync(int senderUserId, SendMessageRequest req)
        {
            using var conn = new SqlConnection(_conn);

            long msgId = await conn.ExecuteScalarAsync<long>(@"
                INSERT INTO tblChatMessage
                    (IdConversation, SenderUserId, MessageText, MessageType, ReplyToMessageId,
                     AttachmentPath, AttachmentName, SentOn, IsEdited, IsDeleted)
                VALUES
                    (@cid, @sender, @text, @mtype, @replyTo,
                     @attachPath, @attachName, GETDATE(), 0, 0);
                SELECT CAST(SCOPE_IDENTITY() AS BIGINT);",
                new
                {
                    cid        = req.ConversationId,
                    sender     = senderUserId,
                    text       = req.MessageText,
                    mtype      = req.MessageType,
                    replyTo    = req.ReplyToMessageId,
                    attachPath = req.AttachmentPath,
                    attachName = req.AttachmentName
                });

            if (msgId <= 0) return null;

            return await conn.QueryFirstOrDefaultAsync<MessageListItemTO>(@"
                SELECT m.IdMessage, m.IdConversation, m.SenderUserId,
                       u.UserName AS SenderName, u.Profile_Image AS SenderAvatar,
                       m.MessageText, m.MessageType, m.ReplyToMessageId,
                       NULL AS ReplyToText, NULL AS ReplyToSender,
                       m.AttachmentPath, m.AttachmentName,
                       m.SentOn, m.IsEdited, m.IsDeleted,
                       CAST(0 AS BIT) AS IsRead
                FROM tblChatMessage m
                INNER JOIN tblUser u ON u.UserId = m.SenderUserId
                WHERE m.IdMessage = @id",
                new { id = msgId });
        }

        public async Task<bool> EditMessageAsync(long messageId, int userId, string newText)
        {
            using var conn = new SqlConnection(_conn);

            // Validate ownership and edit window (15 min)
            int allowed = await conn.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM tblChatMessage
                WHERE IdMessage=@id AND SenderUserId=@uid AND IsDeleted=0
                  AND DATEDIFF(MINUTE, SentOn, GETDATE()) <= 15",
                new { id = messageId, uid = userId });

            if (allowed == 0) return false;

            int rows = await conn.ExecuteAsync(@"
                UPDATE tblChatMessage
                SET MessageText=@text, IsEdited=1, EditedOn=GETDATE()
                WHERE IdMessage=@id AND SenderUserId=@uid",
                new { text = newText, id = messageId, uid = userId });

            return rows > 0;
        }

        public async Task<bool> DeleteMessageAsync(long messageId, int userId)
        {
            using var conn = new SqlConnection(_conn);

            int allowed = await conn.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM tblChatMessage m
                LEFT JOIN tblChatConversationMember mem
                    ON mem.IdConversation = m.IdConversation AND mem.UserId = @uid AND mem.IsActive = 1
                WHERE m.IdMessage = @id AND m.IsDeleted = 0
                  AND (m.SenderUserId = @uid OR mem.MemberRole = 2)",
                new { id = messageId, uid = userId });

            if (allowed == 0) return false;

            int rows = await conn.ExecuteAsync(@"
                UPDATE tblChatMessage
                SET IsDeleted=1, DeletedOn=GETDATE(), DeletedBy=@uid
                WHERE IdMessage=@id AND IsDeleted=0",
                new { id = messageId, uid = userId });

            return rows > 0;
        }

        public async Task MarkConversationReadAsync(int conversationId, int userId)
        {
            using var conn = new SqlConnection(_conn);

            long? lastId = await conn.ExecuteScalarAsync<long?>(@"
                SELECT TOP 1 IdMessage FROM tblChatMessage
                WHERE IdConversation=@cid AND IsDeleted=0
                ORDER BY SentOn DESC",
                new { cid = conversationId });

            if (lastId.HasValue)
            {
                await conn.ExecuteAsync(@"
                    UPDATE tblChatConversationMember
                    SET LastReadMessageId = @lastId
                    WHERE IdConversation=@cid AND UserId=@uid",
                    new { lastId, cid = conversationId, uid = userId });
            }
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            using var conn = new SqlConnection(_conn);
            string sql = @"
                SELECT CAST(ISNULL(SUM(cnt), 0) AS INT)
                FROM (
                    SELECT (
                        SELECT COUNT(*)
                        FROM tblChatMessage m
                        WHERE m.IdConversation = mem.IdConversation
                          AND m.IsDeleted = 0
                          AND m.SenderUserId != @uid
                          AND m.IdMessage > ISNULL(mem.LastReadMessageId, 0)
                    ) AS cnt
                    FROM tblChatConversationMember mem
                    INNER JOIN tblChatConversation c ON c.IdConversation = mem.IdConversation
                    WHERE mem.UserId = @uid AND mem.IsActive = 1 AND c.IsDeleted = 0
                ) AS unread";

            return await conn.ExecuteScalarAsync<int>(sql, new { uid = userId });
        }

        // ── GROUPS ───────────────────────────────────────────────────

        public async Task<int> CreateGroupAsync(int creatorUserId, CreateGroupRequest req)
        {
            using var conn = new SqlConnection(_conn);
            await conn.OpenAsync();
            using var tran = conn.BeginTransaction();
            try
            {
                int convId = await conn.ExecuteScalarAsync<int>(@"
                    INSERT INTO tblChatConversation
                        (ConversationType, ConversationName, ConversationDescription,
                         CreatedBy, CreatedOn, IsActive, IsDeleted)
                    VALUES (2, @name, @desc, @cb, GETDATE(), 1, 0);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    new { name = req.GroupName, desc = req.GroupDescription, cb = creatorUserId },
                    tran);

                // Creator as Admin (role 2)
                await conn.ExecuteAsync(@"
                    INSERT INTO tblChatConversationMember
                        (IdConversation,UserId,MemberRole,JoinedOn,IsActive)
                    VALUES (@c,@u,2,GETDATE(),1)",
                    new { c = convId, u = creatorUserId }, tran);

                // Add other members
                foreach (int memberId in req.MemberIds.Where(m => m != creatorUserId))
                {
                    await conn.ExecuteAsync(@"
                        INSERT INTO tblChatConversationMember
                            (IdConversation,UserId,MemberRole,JoinedOn,IsActive)
                        VALUES (@c,@u,1,GETDATE(),1)",
                        new { c = convId, u = memberId }, tran);
                }

                tran.Commit();
                return convId;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public async Task<bool> UpdateGroupAsync(int conversationId, UpdateGroupRequest req, int requestingUserId)
        {
            using var conn = new SqlConnection(_conn);

            int isMember = await conn.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM tblChatConversationMember
                WHERE IdConversation=@cid AND UserId=@uid AND IsActive=1",
                new { cid = conversationId, uid = requestingUserId });

            if (isMember == 0) return false;

            int rows = await conn.ExecuteAsync(@"
                UPDATE tblChatConversation
                SET ConversationName = @name,
                    ConversationDescription = @desc,
                    UpdatedBy = @uid,
                    UpdatedOn = GETDATE()
                WHERE IdConversation = @cid AND ConversationType = 2 AND IsDeleted = 0",
                new
                {
                    name = req.GroupName,
                    desc = req.GroupDescription,
                    uid  = requestingUserId,
                    cid  = conversationId
                });

            return rows > 0;
        }

        public async Task<GroupDetailsTO?> GetGroupDetailsAsync(int conversationId, int callerUserId)
        {
            using var conn = new SqlConnection(_conn);

            var conv = await conn.QueryFirstOrDefaultAsync<ChatConversationTO>(@"
                SELECT * FROM tblChatConversation
                WHERE IdConversation=@id AND IsDeleted=0",
                new { id = conversationId });

            if (conv == null) return null;

            var members = (await conn.QueryAsync<ChatMemberTO>(@"
                SELECT m.IdConversationMember, m.IdConversation, m.UserId,
                       u.UserName, u.Profile_Image AS ProfileImage, r.RoleName,
                       m.MemberRole, m.JoinedOn, m.IsActive
                FROM tblChatConversationMember m
                INNER JOIN tblUser u ON u.UserId = m.UserId
                LEFT JOIN tblUserRoleMapping rm ON rm.UserId = u.UserId AND rm.IsActive = 1
                LEFT JOIN tblRoles r ON r.RoleId = rm.RoleId
                WHERE m.IdConversation = @cid AND m.IsActive = 1",
                new { cid = conversationId })).ToList();

            string? createdByName = await conn.ExecuteScalarAsync<string?>(
                "SELECT UserName FROM tblUser WHERE UserId=@id",
                new { id = conv.CreatedBy });

            int myRole = members.FirstOrDefault(m => m.UserId == callerUserId)?.MemberRole ?? 1;

            return new GroupDetailsTO
            {
                IdConversation  = conversationId,
                GroupName       = conv.ConversationName,
                GroupDescription= conv.ConversationDescription,
                GroupImage      = conv.GroupImage,
                CreatedOn       = conv.CreatedOn,
                CreatedByName   = createdByName,
                Members         = members,
                CurrentUserRole = myRole
            };
        }

        public async Task<bool> AddGroupMemberAsync(int conversationId, int userId, int requestingUserId)
        {
            using var conn = new SqlConnection(_conn);

            // Validate requester is member of group
            int isMember = await conn.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM tblChatConversationMember
                WHERE IdConversation=@cid AND UserId=@rid AND IsActive=1",
                new { cid = conversationId, rid = requestingUserId });

            if (isMember == 0) return false;

            // Check already member
            int exists = await conn.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM tblChatConversationMember
                WHERE IdConversation=@cid AND UserId=@uid",
                new { cid = conversationId, uid = userId });

            if (exists > 0)
            {
                // Reactivate if left
                await conn.ExecuteAsync(@"
                    UPDATE tblChatConversationMember
                    SET IsActive=1, LeftOn=NULL, JoinedOn=GETDATE()
                    WHERE IdConversation=@cid AND UserId=@uid",
                    new { cid = conversationId, uid = userId });
            }
            else
            {
                await conn.ExecuteAsync(@"
                    INSERT INTO tblChatConversationMember (IdConversation,UserId,MemberRole,JoinedOn,IsActive)
                    VALUES (@cid,@uid,1,GETDATE(),1)",
                    new { cid = conversationId, uid = userId });
            }

            return true;
        }

        public async Task<bool> RemoveGroupMemberAsync(int conversationId, int userId, int requestingUserId)
        {
            using var conn = new SqlConnection(_conn);

            // Validate requester is admin and not removing themselves
            int isAdmin = await conn.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM tblChatConversationMember
                WHERE IdConversation=@cid AND UserId=@rid AND MemberRole=2 AND IsActive=1",
                new { cid = conversationId, rid = requestingUserId });

            if (isAdmin == 0 || requestingUserId == userId) return false;

            int rows = await conn.ExecuteAsync(@"
                UPDATE tblChatConversationMember
                SET IsActive=0, LeftOn=GETDATE()
                WHERE IdConversation=@cid AND UserId=@uid",
                new { cid = conversationId, uid = userId });

            return rows > 0;
        }

        public async Task<bool> LeaveConversationAsync(int conversationId, int userId)
        {
            using var conn = new SqlConnection(_conn);
            int rows = await conn.ExecuteAsync(@"
                UPDATE tblChatConversationMember
                SET IsActive=0, LeftOn=GETDATE()
                WHERE IdConversation=@cid AND UserId=@uid AND IsActive=1",
                new { cid = conversationId, uid = userId });
            return rows > 0;
        }

        // ── ORDER CHAT ───────────────────────────────────────────────

        public async Task<ConversationListItemTO?> GetOrderConversationAsync(int orderId, int userId)
        {
            using var conn = new SqlConnection(_conn);

            int? convId = await conn.ExecuteScalarAsync<int?>(@"
                SELECT TOP 1 c.IdConversation
                FROM tblChatConversation c
                INNER JOIN tblChatConversationMember m ON m.IdConversation=c.IdConversation
                WHERE c.IdOrderMaster=@oid AND m.UserId=@uid AND m.IsActive=1 AND c.IsDeleted=0",
                new { oid = orderId, uid = userId });

            if (!convId.HasValue) return null;
            return await GetConversationByIdAsync(convId.Value, userId);
        }

        public async Task<int> CreateOrderConversationAsync(int orderId, int creatorUserId)
        {
            using var conn = new SqlConnection(_conn);
            await conn.OpenAsync();
            using var tran = conn.BeginTransaction();
            try
            {
                // Validate order exists
                int orderExists = await conn.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM tblOrderMaster WHERE IdOrderMaster=@oid",
                    new { oid = orderId }, tran);

                if (orderExists == 0) throw new Exception($"Order {orderId} not found.");

                // Get order participants: customer + assigned cook + delivery boy + managers
                string orderNo = await conn.ExecuteScalarAsync<string?>(@"
                    SELECT ISNULL(CAST(IdOrderMaster AS NVARCHAR), '') FROM tblOrderMaster WHERE IdOrderMaster=@oid",
                    new { oid = orderId }, tran) ?? orderId.ToString();

                int convId = await conn.ExecuteScalarAsync<int>(@"
                    INSERT INTO tblChatConversation
                        (ConversationType, ConversationName, IdOrderMaster, CreatedBy, CreatedOn, IsActive, IsDeleted)
                    VALUES (2, @name, @oid, @cb, GETDATE(), 1, 0);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    new { name = $"Order #{orderNo} Chat", oid = orderId, cb = creatorUserId },
                    tran);

                // Add creator
                await conn.ExecuteAsync(@"
                    INSERT INTO tblChatConversationMember (IdConversation,UserId,MemberRole,JoinedOn,IsActive)
                    VALUES (@c,@u,2,GETDATE(),1)",
                    new { c = convId, u = creatorUserId }, tran);

                // Add order customer
                var customerUserId = await conn.ExecuteScalarAsync<int?>(@"
                    SELECT IdCustomer FROM tblOrderMaster WHERE IdOrderMaster=@oid", new { oid = orderId }, tran);
                if (customerUserId.HasValue && customerUserId.Value != creatorUserId)
                {
                    await conn.ExecuteAsync(@"
                        INSERT INTO tblChatConversationMember (IdConversation,UserId,MemberRole,JoinedOn,IsActive)
                        VALUES (@c,@u,1,GETDATE(),1)",
                        new { c = convId, u = customerUserId.Value }, tran);
                }

                tran.Commit();
                return convId;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        // ── AUTOMATED CLEANUP & ARCHIVE ──────────────────────────────

        public async Task<(int archivedCount, int deletedCount)> ArchiveAndCleanupChatMessagesAsync(int retentionDays = 5)
        {
            using var conn = new SqlConnection(_conn);
            await conn.OpenAsync();
            using var tran = conn.BeginTransaction();
            try
            {
                // 1. Ensure backup table exists
                string createBackupTableSql = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tblChatMessage_Backup')
                    BEGIN
                        CREATE TABLE tblChatMessage_Backup (
                            IdMessage BIGINT NOT NULL,
                            IdConversation INT NOT NULL,
                            SenderUserId INT NOT NULL,
                            MessageText NVARCHAR(MAX) NULL,
                            MessageType INT NOT NULL,
                            ReplyToMessageId BIGINT NULL,
                            AttachmentPath NVARCHAR(500) NULL,
                            AttachmentName NVARCHAR(255) NULL,
                            SentOn DATETIME NOT NULL,
                            IsEdited BIT NOT NULL,
                            IsDeleted BIT NOT NULL,
                            ArchivedOn DATETIME NOT NULL DEFAULT GETDATE(),
                            PRIMARY KEY (IdMessage)
                        );
                    END";
                await conn.ExecuteAsync(createBackupTableSql, transaction: tran);

                // 2. Insert messages older than retentionDays into backup table
                string archiveSql = @"
                    INSERT INTO tblChatMessage_Backup
                        (IdMessage, IdConversation, SenderUserId, MessageText, MessageType,
                         ReplyToMessageId, AttachmentPath, AttachmentName, SentOn, IsEdited, IsDeleted, ArchivedOn)
                    SELECT
                        m.IdMessage, m.IdConversation, m.SenderUserId, m.MessageText, m.MessageType,
                        m.ReplyToMessageId, m.AttachmentPath, m.AttachmentName, m.SentOn, m.IsEdited, m.IsDeleted, GETDATE()
                    FROM tblChatMessage m
                    LEFT JOIN tblChatMessage_Backup b ON b.IdMessage = m.IdMessage
                    WHERE m.SentOn < DATEADD(DAY, -@days, GETDATE())
                      AND b.IdMessage IS NULL";
                int archivedCount = await conn.ExecuteAsync(archiveSql, new { days = retentionDays }, transaction: tran);

                // 3. Delete messages older than retentionDays from primary tblChatMessage
                string deleteSql = @"
                    DELETE FROM tblChatMessage
                    WHERE SentOn < DATEADD(DAY, -@days, GETDATE())";
                int deletedCount = await conn.ExecuteAsync(deleteSql, new { days = retentionDays }, transaction: tran);

                tran.Commit();
                return (archivedCount, deletedCount);
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }
    }
}
