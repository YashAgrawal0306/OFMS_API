using DTO.Models.Chat;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.DAL.Interface.Chat
{
    public interface IChatDAL
    {
        // ── Users ──────────────────────────────────────────────────
        Task<List<ChatUserTO>> GetChatUsersAsync(int currentUserId, string? searchText = null);

        // ── Conversations ──────────────────────────────────────────
        Task<List<ConversationListItemTO>> GetUserConversationsAsync(int userId);
        Task<ConversationListItemTO?> GetConversationByIdAsync(int conversationId, int userId);
        Task<bool> IsUserMemberAsync(int conversationId, int userId);

        // ── Personal Chat ──────────────────────────────────────────
        Task<int> GetOrCreatePersonalConversationAsync(int userId1, int userId2);

        // ── Messages ───────────────────────────────────────────────
        Task<List<MessageListItemTO>> GetMessagesAsync(int conversationId, int callerUserId, int pageNumber, int pageSize);
        Task<MessageListItemTO?> SendMessageAsync(int senderUserId, SendMessageRequest request);
        Task<bool> EditMessageAsync(long messageId, int userId, string newText);
        Task<bool> DeleteMessageAsync(long messageId, int userId);
        Task MarkConversationReadAsync(int conversationId, int userId);
        Task<int> GetUnreadCountAsync(int userId);

        // ── Groups ─────────────────────────────────────────────────
        Task<int> CreateGroupAsync(int creatorUserId, CreateGroupRequest request);
        Task<bool> UpdateGroupAsync(int conversationId, UpdateGroupRequest request, int requestingUserId);
        Task<GroupDetailsTO?> GetGroupDetailsAsync(int conversationId, int callerUserId);
        Task<bool> AddGroupMemberAsync(int conversationId, int userId, int requestingUserId);
        Task<bool> RemoveGroupMemberAsync(int conversationId, int userId, int requestingUserId);
        Task<bool> LeaveConversationAsync(int conversationId, int userId);

        // ── Order Chat ─────────────────────────────────────────────
        Task<ConversationListItemTO?> GetOrderConversationAsync(int orderId, int userId);
        Task<int> CreateOrderConversationAsync(int orderId, int creatorUserId);

        // ── Cleanup & Automation ──────────────────────────────────
        Task<(int archivedCount, int deletedCount)> ArchiveAndCleanupChatMessagesAsync(int retentionDays = 5);
    }
}
