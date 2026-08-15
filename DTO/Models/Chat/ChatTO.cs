using System;
using System.Collections.Generic;

namespace DTO.Models.Chat
{
    // ── Conversation ──────────────────────────────────────────────

    public class ChatConversationTO
    {
        public int      IdConversation          { get; set; }
        public int      ConversationType        { get; set; } // 1=Personal, 2=Group
        public string?  ConversationName        { get; set; }
        public string?  ConversationDescription { get; set; }
        public string?  GroupImage              { get; set; }
        public int?     IdOrderMaster           { get; set; }
        public int      CreatedBy               { get; set; }
        public DateTime CreatedOn               { get; set; }
        public bool     IsActive                { get; set; }
    }

    /// <summary>Summary item shown in the conversation list sidebar.</summary>
    public class ConversationListItemTO
    {
        public int      IdConversation     { get; set; }
        public int      ConversationType   { get; set; }
        public string?  Name               { get; set; } // other user name or group name
        public string?  Description        { get; set; }
        public string?  AvatarUrl          { get; set; }
        public string?  LastMessage        { get; set; }
        public DateTime? LastMessageTime   { get; set; }
        public int      UnreadCount        { get; set; }
        public bool     IsOnline           { get; set; }
        public DateTime? LastSeen          { get; set; }
        public int?     OtherUserId        { get; set; }
        public int?     IdOrderMaster      { get; set; }
        public string?  OrderNo            { get; set; }
        public int      MemberCount        { get; set; }
    }

    // ── Member ────────────────────────────────────────────────────

    public class ChatMemberTO
    {
        public int      IdConversationMember { get; set; }
        public int      IdConversation       { get; set; }
        public int      UserId               { get; set; }
        public string?  UserName             { get; set; }
        public string?  ProfileImage         { get; set; }
        public string?  RoleName             { get; set; }
        public int      MemberRole           { get; set; } // 1=Member,2=Admin
        public DateTime JoinedOn             { get; set; }
        public bool     IsActive             { get; set; }
        public bool     IsOnline             { get; set; }
        public DateTime? LastSeen            { get; set; }
    }

    // ── Message ────────────────────────────────────────────────────

    public class MessageListItemTO
    {
        public long     IdMessage         { get; set; }
        public int      IdConversation    { get; set; }
        public int      SenderUserId      { get; set; }
        public string?  SenderName        { get; set; }
        public string?  SenderAvatar      { get; set; }
        public string?  MessageText       { get; set; }
        public int      MessageType       { get; set; }
        public long?    ReplyToMessageId  { get; set; }
        public string?  ReplyToText       { get; set; }  // preview of replied message
        public string?  ReplyToSender     { get; set; }
        public string?  AttachmentPath    { get; set; }
        public string?  AttachmentName    { get; set; }
        public DateTime SentOn            { get; set; }
        public bool     IsEdited          { get; set; }
        public bool     IsDeleted         { get; set; }
        public bool     IsRead            { get; set; }  // Read by at least one recipient
        public bool     IsOwnMessage      { get; set; }  // populated in BL with caller's userId
    }

    // ── User (available for chat) ─────────────────────────────────

    public class ChatUserTO
    {
        public int      UserId       { get; set; }
        public string?  UserName     { get; set; }
        public string?  UserEmail    { get; set; }
        public string?  RoleName     { get; set; }
        public int      RoleId       { get; set; }
        public string?  ProfileImage { get; set; }
        public bool     IsOnline     { get; set; }
        public DateTime? LastSeen    { get; set; }
    }

    // ── Group details ─────────────────────────────────────────────

    public class GroupDetailsTO
    {
        public int                  IdConversation     { get; set; }
        public string?              GroupName          { get; set; }
        public string?              GroupDescription   { get; set; }
        public string?              GroupImage         { get; set; }
        public DateTime             CreatedOn          { get; set; }
        public string?              CreatedByName      { get; set; }
        public List<ChatMemberTO>?  Members            { get; set; }
        public int                  CurrentUserRole    { get; set; } // 1=Member,2=Admin
    }

    // ── Request DTOs ────────────────────────────────────────────────

    public class CreatePersonalChatRequest
    {
        public int UserId { get; set; }
    }

    public class CreateGroupRequest
    {
        public string  GroupName        { get; set; } = "";
        public string? GroupDescription{ get; set; }
        public List<int> MemberIds     { get; set; } = new();
    }

    public class UpdateGroupRequest
    {
        public string  GroupName        { get; set; } = "";
        public string? GroupDescription{ get; set; }
    }

    public class SendMessageRequest
    {
        public int    ConversationId      { get; set; }
        public string? MessageText        { get; set; }
        public int    MessageType         { get; set; } = 1;
        public long?  ReplyToMessageId    { get; set; }
        public string? AttachmentPath     { get; set; }
        public string? AttachmentName     { get; set; }
    }

    public class EditMessageRequest
    {
        public string MessageText { get; set; } = "";
    }

    public class AddMemberRequest
    {
        public int UserId { get; set; }
    }

    public class UnreadCountTO
    {
        public int TotalUnread { get; set; }
    }
}
