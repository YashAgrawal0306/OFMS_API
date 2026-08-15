using DTO.Models.Chat;
using DTO.Models.CommonModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OFMS_API.Hubs;
using Services.BL.Interface.Chat;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OFMS_API.Controllers.Chat
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatBL _bl;
        private readonly IHubContext<ChatHub> _chatHub;

        public ChatController(IChatBL bl, IHubContext<ChatHub> chatHub)
        {
            _bl = bl;
            _chatHub = chatHub;
        }

        // ── GET /api/Chat/users ────────────────────────────────────────
        [HttpGet("users")]
        public async Task<IActionResult> GetChatUsers([FromQuery] string? q = null)
        {
            var response = new GlobalResponseModel<object>();
            try
            {
                int userId = GetUserIdFromToken();
                var users = await _bl.GetChatUsersAsync(userId, q);
                foreach (var u in users)
                {
                    u.IsOnline = ChatHub.IsUserOnline(u.UserId);
                }
                response.data       = users;
                response.message    = "Users retrieved successfully";
                response.statusCode = StatusCodes.Status200OK;
                response.status     = "Success";
                return Ok(response);
            }
            catch (Exception ex)
            {
                return ServerError(response, ex);
            }
        }

        // ── GET /api/Chat/conversations ────────────────────────────────
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var response = new GlobalResponseModel<object>();
            try
            {
                int userId = GetUserIdFromToken();
                var convs = await _bl.GetUserConversationsAsync(userId);
                foreach (var c in convs)
                {
                    if (c.ConversationType == 1 && c.OtherUserId.HasValue)
                    {
                        c.IsOnline = ChatHub.IsUserOnline(c.OtherUserId.Value);
                    }
                }
                response.data       = convs;
                response.message    = "Conversations retrieved";
                response.statusCode = StatusCodes.Status200OK;
                response.status     = "Success";
                return Ok(response);
            }
            catch (Exception ex) { return ServerError(response, ex); }
        }

        // ── POST /api/Chat/personal ────────────────────────────────────
        [HttpPost("personal")]
        public async Task<IActionResult> GetOrCreatePersonal([FromBody] CreatePersonalChatRequest req)
        {
                var response = new GlobalResponseModel<object>();
            try
            {
                int userId = GetUserIdFromToken();
                int convId = await _bl.GetOrCreatePersonalConversationAsync(userId, req.UserId);
                var conv   = await _bl.GetConversationByIdAsync(convId, userId);
                response.data       = new { conversationId = convId, conversation = conv };
                response.message    = "Personal conversation ready";
                response.statusCode = StatusCodes.Status200OK;
                response.status     = "Success";
                return Ok(response);
            }
            catch (InvalidOperationException ioe)
            {
                response.message    = ioe.Message;
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status     = "Error";
                return BadRequest(response);
            }
            catch (Exception ex) { return ServerError(response, ex); }
        }

        // ── GET /api/Chat/conversations/{id}/messages ─────────────────
        [HttpGet("conversations/{id}/messages")]
        public async Task<IActionResult> GetMessages(int id, [FromQuery] int page = 1, [FromQuery] int size = 30)
        {
            var response = new GlobalResponseModel<object>();
            try
            {
                int userId = GetUserIdFromToken();
                var messages = await _bl.GetMessagesAsync(id, userId, page, size);

                // Mark as read after fetching
                await _bl.MarkConversationReadAsync(id, userId);

                response.data       = messages;
                response.message    = "Messages retrieved";
                response.statusCode = StatusCodes.Status200OK;
                response.status     = "Success";
                return Ok(response);
            }
            catch (Exception ex) { return ServerError(response, ex); }
        }

        // ── POST /api/Chat/messages ────────────────────────────────────
        [HttpPost("messages")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest req)
        {
            var response = new GlobalResponseModel<object>();
            try
            {
                int userId = GetUserIdFromToken();
                var msg    = await _bl.SendMessageAsync(userId, req);

                if (msg == null)
                {
                    response.message    = "Failed to send message";
                    response.statusCode = StatusCodes.Status400BadRequest;
                    response.status     = "Error";
                    return BadRequest(response);
                }

                // Broadcast via SignalR
                await _chatHub.Clients
                    .Group(ChatHub.GetConvGroupName(req.ConversationId))
                    .SendAsync("ReceiveMessage", msg);

                await _chatHub.Clients.All
                    .SendAsync("UnreadCountUpdated");

                response.data       = msg;
                response.message    = "Message sent";
                response.statusCode = StatusCodes.Status200OK;
                response.status     = "Success";
                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ioe)
            {
                response.message    = ioe.Message;
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status     = "Error";
                return BadRequest(response);
            }
            catch (Exception ex) { return ServerError(response, ex); }
        }

        // ── PUT /api/Chat/messages/{id} ────────────────────────────────
        [HttpPut("messages/{id}")]
        public async Task<IActionResult> EditMessage(long id, [FromBody] EditMessageRequest req)
        {
            var response = new GlobalResponseModel<object>();
            try
            {
                int userId  = GetUserIdFromToken();
                bool success = await _bl.EditMessageAsync(id, userId, req.MessageText);

                if (success)
                {
                    // Broadcast edit event — we don't know conversationId here without a DB lookup;
                    // the controller sends a generic event; the client updates locally.
                    await _chatHub.Clients.All.SendAsync("MessageEdited", id, req.MessageText);
                }

                response.data       = success;
                response.message    = success ? "Message edited" : "Edit not allowed (time window expired or not owner)";
                response.statusCode = success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest;
                response.status     = success ? "Success" : "Error";
                return success ? Ok(response) : BadRequest(response);
            }
            catch (Exception ex) { return ServerError(response, ex); }
        }

        // ── DELETE /api/Chat/messages/{id} ────────────────────────────
        [HttpDelete("messages/{id}")]
        public async Task<IActionResult> DeleteMessage(long id)
        {
            var response = new GlobalResponseModel<object>();
            try
            {
                int userId   = GetUserIdFromToken();
                bool success = await _bl.DeleteMessageAsync(id, userId);

                if (success)
                    await _chatHub.Clients.All.SendAsync("MessageDeleted", id);

                response.data       = success;
                response.message    = success ? "Message deleted" : "Delete not allowed";
                response.statusCode = success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest;
                response.status     = success ? "Success" : "Error";
                return success ? Ok(response) : BadRequest(response);
            }
            catch (Exception ex) { return ServerError(response, ex); }
        }

        // ── POST /api/Chat/conversations/{id}/read ─────────────────────
        [HttpPost("conversations/{id}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var response = new GlobalResponseModel<object>();
            try
            {
                int userId = GetUserIdFromToken();
                await _bl.MarkConversationReadAsync(id, userId);
                await _chatHub.Clients.All.SendAsync("MessageRead", id, userId);
                await _chatHub.Clients.All.SendAsync("UnreadCountUpdated");
                response.data       = true;
                response.message    = "Marked as read";
                response.statusCode = StatusCodes.Status200OK;
                response.status     = "Success";
                return Ok(response);
            }
            catch (Exception ex) { return ServerError(response, ex); }
        }

        // ── GET /api/Chat/unread-count ─────────────────────────────────
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var response = new GlobalResponseModel<object>();
            try
            {
                int userId = GetUserIdFromToken();
                if (userId <= 0)
                {
                    response.data       = new { totalUnread = 0 };
                    response.message    = "No active user session";
                    response.statusCode = StatusCodes.Status200OK;
                    response.status     = "Success";
                    return Ok(response);
                }

                int count = 0;
                try
                {
                    count = await _bl.GetUnreadCountAsync(userId);
                }
                catch
                {
                    count = 0;
                }

                response.data       = new { totalUnread = count };
                response.message    = "Unread count retrieved";
                response.statusCode = StatusCodes.Status200OK;
                response.status     = "Success";
                return Ok(response);
            }
            catch (Exception ex) { return ServerError(response, ex); }
        }

        // ── POST /api/Chat/groups ──────────────────────────────────────
        [HttpPost("groups")]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest req)
        {
            var response = new GlobalResponseModel<object>();
            try
            {
                int userId = GetUserIdFromToken();
                int convId = await _bl.CreateGroupAsync(userId, req);
                response.data       = new { conversationId = convId };
                response.message    = "Group created successfully";
                response.statusCode = StatusCodes.Status200OK;
                response.status     = "Success";
                return Ok(response);
            }
            catch (InvalidOperationException ioe)
            {
                response.message    = ioe.Message;
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status     = "Error";
                return BadRequest(response);
            }
            catch (Exception ex) { return ServerError(response, ex); }
        }

        // ── GET /api/Chat/groups/{id} ──────────────────────────────────
        [HttpGet("groups/{id}")]
        public async Task<IActionResult> GetGroupDetails(int id)
        {
            var response = new GlobalResponseModel<object>();
            try
            {
                int userId = GetUserIdFromToken();
                var details = await _bl.GetGroupDetailsAsync(id, userId);
                if (details == null)
                {
                    response.message    = "Group not found or access denied";
                    response.statusCode = StatusCodes.Status404NotFound;
                    response.status     = "Error";
                    return NotFound(response);
                }
                response.data       = details;
                response.message    = "Group details retrieved";
                response.statusCode = StatusCodes.Status200OK;
                response.status     = "Success";
                return Ok(response);
            }
            catch (Exception ex) { return ServerError(response, ex); }
        }

        // ── PUT /api/Chat/groups/{id} ──────────────────────────────────
        [HttpPut("groups/{id}")]
        public async Task<IActionResult> UpdateGroup(int id, [FromBody] UpdateGroupRequest req)
        {
            var response = new GlobalResponseModel<object>();
            try
            {
                int userId   = GetUserIdFromToken();
                bool success = await _bl.UpdateGroupAsync(id, req, userId);

                if (success)
                    await _chatHub.Clients.All.SendAsync("GroupUpdated", id, req.GroupName);

                response.data       = success;
                response.message    = success ? "Group updated" : "Not authorized or group not found";
                response.statusCode = success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest;
                response.status     = success ? "Success" : "Error";
                return success ? Ok(response) : BadRequest(response);
            }
            catch (Exception ex) { return ServerError(response, ex); }
        }

        // ── POST /api/Chat/groups/{id}/members ────────────────────────
        [HttpPost("groups/{id}/members")]
        public async Task<IActionResult> AddMember(int id, [FromBody] AddMemberRequest req)
        {
            var response = new GlobalResponseModel<object>();
            try
            {
                int userId   = GetUserIdFromToken();
                bool success = await _bl.AddGroupMemberAsync(id, req.UserId, userId);
                response.data       = success;
                response.message    = success ? "Member added" : "Not authorized or already a member";
                response.statusCode = success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest;
                response.status     = success ? "Success" : "Error";
                return success ? Ok(response) : BadRequest(response);
            }
            catch (Exception ex) { return ServerError(response, ex); }
        }

        // ── DELETE /api/Chat/groups/{id}/members/{userId} ─────────────
        [HttpDelete("groups/{id}/members/{memberId}")]
        public async Task<IActionResult> RemoveMember(int id, int memberId)
        {
            var response = new GlobalResponseModel<object>();
            try
            {
                int userId   = GetUserIdFromToken();
                bool success = await _bl.RemoveGroupMemberAsync(id, memberId, userId);
                response.data       = success;
                response.message    = success ? "Member removed" : "Not authorized";
                response.statusCode = success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest;
                response.status     = success ? "Success" : "Error";
                return success ? Ok(response) : BadRequest(response);
            }
            catch (Exception ex) { return ServerError(response, ex); }
        }

        // ── POST /api/Chat/groups/{id}/leave ──────────────────────────
        [HttpPost("groups/{id}/leave")]
        public async Task<IActionResult> LeaveGroup(int id)
        {
            var response = new GlobalResponseModel<object>();
            try
            {
                int userId   = GetUserIdFromToken();
                bool success = await _bl.LeaveConversationAsync(id, userId);
                response.data       = success;
                response.message    = success ? "Left group" : "Failed to leave";
                response.statusCode = success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest;
                response.status     = success ? "Success" : "Error";
                return success ? Ok(response) : BadRequest(response);
            }
            catch (Exception ex) { return ServerError(response, ex); }
        }

        // ── GET /api/Chat/order/{orderId} ─────────────────────────────
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetOrderConversation(int orderId)
        {
            var response = new GlobalResponseModel<object>();
            try
            {
                int userId = GetUserIdFromToken();
                var conv   = await _bl.GetOrderConversationAsync(orderId, userId);
                response.data       = conv;
                response.message    = conv != null ? "Order conversation found" : "No conversation yet";
                response.statusCode = StatusCodes.Status200OK;
                response.status     = "Success";
                return Ok(response);
            }
            catch (Exception ex) { return ServerError(response, ex); }
        }

        // ── POST /api/Chat/order/{orderId} ────────────────────────────
        [HttpPost("order/{orderId}")]
        public async Task<IActionResult> CreateOrderConversation(int orderId)
        {
            var response = new GlobalResponseModel<object>();
            try
            {
                int userId = GetUserIdFromToken();
                int convId = await _bl.CreateOrderConversationAsync(orderId, userId);
                var conv   = await _bl.GetConversationByIdAsync(convId, userId);
                response.data       = new { conversationId = convId, conversation = conv };
                response.message    = "Order conversation created";
                response.statusCode = StatusCodes.Status200OK;
                response.status     = "Success";
                return Ok(response);
            }
            catch (Exception ex) { return ServerError(response, ex); }
        }

        // ── POST /api/Chat/cleanup ────────────────────────────────────
        [HttpPost("cleanup")]
        public async Task<IActionResult> CleanupOldMessages([FromQuery] int retentionDays = 5)
        {
            var response = new GlobalResponseModel<object>();
            try
            {
                var (archived, deleted) = await _bl.ArchiveAndCleanupChatMessagesAsync(retentionDays);
                response.data       = new { archivedMessages = archived, deletedMessages = deleted, retentionDays };
                response.message    = $"Successfully archived {archived} and cleaned up {deleted} chat messages older than {retentionDays} days.";
                response.statusCode = StatusCodes.Status200OK;
                response.status     = "Success";
                return Ok(response);
            }
            catch (Exception ex) { return ServerError(response, ex); }
        }

        // ── Helpers ────────────────────────────────────────────────────

        private int GetUserIdFromToken()
        {
            var claim = User.FindFirst("userId")
                     ?? User.FindFirst("UserId")
                     ?? User.FindFirst("id")
                     ?? User.FindFirst("user_id")
                     ?? User.FindFirst(ClaimTypes.NameIdentifier)
                     ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                     ?? User.FindFirst("nameid")
                     ?? User.FindFirst("sub");
            return int.TryParse(claim?.Value, out int id) ? id : 0;
        }

        private IActionResult ServerError(GlobalResponseModel<object> response, Exception ex)
        {
            response.exception  = ex;
            response.message    = ex.Message;
            response.statusCode = StatusCodes.Status500InternalServerError;
            response.status     = "Error";
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }
    }
}
