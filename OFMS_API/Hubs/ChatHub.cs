using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OFMS_API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        // Track connectionId → userId  (in-memory; resets on restart)
        private static readonly ConcurrentDictionary<string, int> _connections = new();
        // Track userId → set of connectionIds (user may have multiple tabs)
        private static readonly ConcurrentDictionary<int, ConcurrentBag<string>> _userConns = new();

        // ── Connection lifecycle ──────────────────────────────────────

        public override async Task OnConnectedAsync()
        {
            int userId = GetUserId();
            if (userId > 0)
            {
                _connections[Context.ConnectionId] = userId;
                _userConns.GetOrAdd(userId, _ => new ConcurrentBag<string>()).Add(Context.ConnectionId);

                // Notify others that this user is online
                await Clients.Others.SendAsync("UserOnline", userId);
                await Clients.Others.SendAsync("UserStatusChanged", userId, true);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            int userId = GetUserId();
            if (userId > 0)
            {
                _connections.TryRemove(Context.ConnectionId, out _);

                if (_userConns.TryGetValue(userId, out var bag))
                {
                    var remaining = new ConcurrentBag<string>(
                       Enumerable.Where(bag, id => id != Context.ConnectionId));
                    _userConns[userId] = remaining;

                    // Only notify offline if no other connections remain
                    if (remaining.IsEmpty)
                    {
                        _userConns.TryRemove(userId, out _);
                        await Clients.Others.SendAsync("UserOffline", userId);
                        await Clients.Others.SendAsync("UserStatusChanged", userId, false);
                    }
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        // ── Conversation group management ─────────────────────────────

        /// <summary>Client calls this after opening a conversation to join the SignalR group.</summary>
        public async Task JoinConversation(int conversationId)
        {
            string groupName = ConvGroup(conversationId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveConversation(int conversationId)
        {
            string groupName = ConvGroup(conversationId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        // ── Typing indicators ────────────────────────────────────────

        public async Task SendTyping(int conversationId)
        {
            int userId = GetUserId();
            await Clients.OthersInGroup(ConvGroup(conversationId))
                .SendAsync("UserTyping", conversationId, userId);
        }

        public async Task StopTyping(int conversationId)
        {
            int userId = GetUserId();
            await Clients.OthersInGroup(ConvGroup(conversationId))
                .SendAsync("UserStoppedTyping", conversationId, userId);
        }

        // ── Static helpers ────────────────────────────────────────────

        private static string ConvGroup(int conversationId) => $"chat_{conversationId}";

        private int GetUserId()
        {
            var user = Context.User;
            if (user == null) return 0;

            var claim = user.FindFirst("userId")
                     ?? user.FindFirst("UserId")
                     ?? user.FindFirst("id")
                     ?? user.FindFirst("user_id")
                     ?? user.FindFirst(ClaimTypes.NameIdentifier)
                     ?? user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                     ?? user.FindFirst("nameid")
                     ?? user.FindFirst("sub");

            return int.TryParse(claim?.Value, out int id) ? id : 0;
        }

        /// <summary>Check whether a given userId is currently online (has at least one connection).</summary>
        public static bool IsUserOnline(int userId)
            => _userConns.TryGetValue(userId, out var bag) && !bag.IsEmpty;

        /// <summary>Broadcast a new message to all members of a conversation group.</summary>
        public static string GetConvGroupName(int conversationId) => $"chat_{conversationId}";
    }
}
