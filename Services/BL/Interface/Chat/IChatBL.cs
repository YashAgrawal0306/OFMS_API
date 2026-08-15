using DTO.Models.Chat;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.BL.Interface.Chat
{
    public interface IChatBL
    {
        Task<List<ChatUserTO>> GetChatUsersAsync(int currentUserId, string? searchText = null);
        Task<List<ConversationListItemTO>> GetUserConversationsAsync(int userId);
        Task<ConversationListItemTO?> GetConversationByIdAsync(int conversationId, int userId);
        Task<int> GetOrCreatePersonalConversationAsync(int currentUserId, int targetUserId);
        Task<List<MessageListItemTO>> GetMessagesAsync(int conversationId, int callerUserId, int page, int size);
        Task<MessageListItemTO?> SendMessageAsync(int senderUserId, SendMessageRequest request);
        Task<bool> EditMessageAsync(long messageId, int userId, string newText);
        Task<bool> DeleteMessageAsync(long messageId, int userId);
        Task MarkConversationReadAsync(int conversationId, int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task<int> CreateGroupAsync(int creatorUserId, CreateGroupRequest request);
        Task<bool> UpdateGroupAsync(int conversationId, UpdateGroupRequest request, int requestingUserId);
        Task<GroupDetailsTO?> GetGroupDetailsAsync(int conversationId, int callerUserId);
        Task<bool> AddGroupMemberAsync(int conversationId, int userId, int requestingUserId);
        Task<bool> RemoveGroupMemberAsync(int conversationId, int userId, int requestingUserId);
        Task<bool> LeaveConversationAsync(int conversationId, int userId);
        Task<ConversationListItemTO?> GetOrderConversationAsync(int orderId, int userId);
        Task<int> CreateOrderConversationAsync(int orderId, int creatorUserId);
        Task<(int archivedCount, int deletedCount)> ArchiveAndCleanupChatMessagesAsync(int retentionDays = 5);
    }
}
