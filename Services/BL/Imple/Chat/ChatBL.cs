using DTO.Models.Chat;
using Repository.DAL.Interface.Chat;
using Services.BL.Interface.Chat;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.BL.Imple.Chat
{
    public class ChatBL : IChatBL
    {
        private readonly IChatDAL _dal;

        public ChatBL(IChatDAL dal)
        {
            _dal = dal;
        }

        public async Task<List<ChatUserTO>> GetChatUsersAsync(int currentUserId, string? searchText = null)
            => await _dal.GetChatUsersAsync(currentUserId, searchText);

        public async Task<List<ConversationListItemTO>> GetUserConversationsAsync(int userId)
            => await _dal.GetUserConversationsAsync(userId);

        public async Task<ConversationListItemTO?> GetConversationByIdAsync(int conversationId, int userId)
            => await _dal.GetConversationByIdAsync(conversationId, userId);

        public async Task<int> GetOrCreatePersonalConversationAsync(int currentUserId, int targetUserId)
        {
            if (currentUserId == targetUserId)
                throw new InvalidOperationException("Cannot start a conversation with yourself.");

            return await _dal.GetOrCreatePersonalConversationAsync(currentUserId, targetUserId);
        }

        public async Task<List<MessageListItemTO>> GetMessagesAsync(
            int conversationId, int callerUserId, int page, int size)
        {
            // Security: caller must be a member
            bool isMember = await _dal.IsUserMemberAsync(conversationId, callerUserId);
            if (!isMember) return new List<MessageListItemTO>();

            page = Math.Max(1, page);
            size = Math.Clamp(size, 1, 100);
            return await _dal.GetMessagesAsync(conversationId, callerUserId, page, size);
        }

        public async Task<MessageListItemTO?> SendMessageAsync(int senderUserId, SendMessageRequest request)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(request.MessageText) && request.AttachmentPath == null)
                throw new InvalidOperationException("Message cannot be empty.");

            if (request.MessageText?.Length > 2000)
                throw new InvalidOperationException("Message exceeds maximum length of 2000 characters.");

            // Security: sender must be a member
            bool isMember = await _dal.IsUserMemberAsync(request.ConversationId, senderUserId);
            if (!isMember) throw new UnauthorizedAccessException("Not a member of this conversation.");

            return await _dal.SendMessageAsync(senderUserId, request);
        }

        public async Task<bool> EditMessageAsync(long messageId, int userId, string newText)
        {
            if (string.IsNullOrWhiteSpace(newText))
                throw new InvalidOperationException("Edited message cannot be empty.");

            if (newText.Length > 2000)
                throw new InvalidOperationException("Message exceeds maximum length of 2000 characters.");

            return await _dal.EditMessageAsync(messageId, userId, newText);
        }

        public async Task<bool> DeleteMessageAsync(long messageId, int userId)
            => await _dal.DeleteMessageAsync(messageId, userId);

        public async Task MarkConversationReadAsync(int conversationId, int userId)
            => await _dal.MarkConversationReadAsync(conversationId, userId);

        public async Task<int> GetUnreadCountAsync(int userId)
            => await _dal.GetUnreadCountAsync(userId);

        public async Task<int> CreateGroupAsync(int creatorUserId, CreateGroupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.GroupName))
                throw new InvalidOperationException("Group name is required.");

            if (request.GroupName.Length > 200)
                throw new InvalidOperationException("Group name is too long.");

            return await _dal.CreateGroupAsync(creatorUserId, request);
        }

        public async Task<bool> UpdateGroupAsync(int conversationId, UpdateGroupRequest request, int requestingUserId)
        {
            if (string.IsNullOrWhiteSpace(request.GroupName))
                throw new InvalidOperationException("Group name is required.");

            return await _dal.UpdateGroupAsync(conversationId, request, requestingUserId);
        }

        public async Task<GroupDetailsTO?> GetGroupDetailsAsync(int conversationId, int callerUserId)
        {
            bool isMember = await _dal.IsUserMemberAsync(conversationId, callerUserId);
            if (!isMember) return null;
            return await _dal.GetGroupDetailsAsync(conversationId, callerUserId);
        }

        public async Task<bool> AddGroupMemberAsync(int conversationId, int userId, int requestingUserId)
            => await _dal.AddGroupMemberAsync(conversationId, userId, requestingUserId);

        public async Task<bool> RemoveGroupMemberAsync(int conversationId, int userId, int requestingUserId)
            => await _dal.RemoveGroupMemberAsync(conversationId, userId, requestingUserId);

        public async Task<bool> LeaveConversationAsync(int conversationId, int userId)
            => await _dal.LeaveConversationAsync(conversationId, userId);

        public async Task<ConversationListItemTO?> GetOrderConversationAsync(int orderId, int userId)
            => await _dal.GetOrderConversationAsync(orderId, userId);

        public async Task<int> CreateOrderConversationAsync(int orderId, int creatorUserId)
            => await _dal.CreateOrderConversationAsync(orderId, creatorUserId);

        public async Task<(int archivedCount, int deletedCount)> ArchiveAndCleanupChatMessagesAsync(int retentionDays = 5)
            => await _dal.ArchiveAndCleanupChatMessagesAsync(retentionDays);
    }
}
