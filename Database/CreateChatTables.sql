-- ============================================================
-- OFMS Quick Chat / Communication Module
-- Database: Chat Tables
-- Run this script against the OFMS database once.
-- ============================================================

-- ── Table 1: tblChatConversation ─────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tblChatConversation')
BEGIN
    CREATE TABLE tblChatConversation
    (
        IdConversation          INT IDENTITY(1,1) PRIMARY KEY,
        ConversationType        INT NOT NULL,           -- 1=Personal, 2=Group
        ConversationName        NVARCHAR(200) NULL,
        ConversationDescription NVARCHAR(500) NULL,
        GroupImage              NVARCHAR(500) NULL,
        IdOrderMaster           INT NULL,               -- optional order link
        CreatedBy               INT NOT NULL,
        CreatedOn               DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedBy               INT NULL,
        UpdatedOn               DATETIME NULL,
        IsActive                BIT NOT NULL DEFAULT 1,
        IsDeleted               BIT NOT NULL DEFAULT 0
    );

    PRINT 'Created tblChatConversation';
END
ELSE
    PRINT 'tblChatConversation already exists — skipped';

-- ── Table 2: tblChatConversationMember ───────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tblChatConversationMember')
BEGIN
    CREATE TABLE tblChatConversationMember
    (
        IdConversationMember INT IDENTITY(1,1) PRIMARY KEY,
        IdConversation       INT NOT NULL,
        UserId               INT NOT NULL,
        MemberRole           INT NOT NULL DEFAULT 1,    -- 1=Member, 2=Admin
        JoinedOn             DATETIME NOT NULL DEFAULT GETDATE(),
        LeftOn               DATETIME NULL,
        IsActive             BIT NOT NULL DEFAULT 1,
        IsMuted              BIT NOT NULL DEFAULT 0,
        LastReadMessageId    BIGINT NULL
    );

    PRINT 'Created tblChatConversationMember';
END
ELSE
    PRINT 'tblChatConversationMember already exists — skipped';

-- ── Table 3: tblChatMessage ───────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tblChatMessage')
BEGIN
    CREATE TABLE tblChatMessage
    (
        IdMessage         BIGINT IDENTITY(1,1) PRIMARY KEY,
        IdConversation    INT NOT NULL,
        SenderUserId      INT NOT NULL,
        MessageText       NVARCHAR(MAX) NULL,
        MessageType       INT NOT NULL DEFAULT 1,       -- 1=Text,2=Image,3=File,4=System,5=OrderRef
        ReplyToMessageId  BIGINT NULL,
        AttachmentPath    NVARCHAR(1000) NULL,
        AttachmentName    NVARCHAR(255) NULL,
        SentOn            DATETIME NOT NULL DEFAULT GETDATE(),
        EditedOn          DATETIME NULL,
        IsEdited          BIT NOT NULL DEFAULT 0,
        IsDeleted         BIT NOT NULL DEFAULT 0,
        DeletedOn         DATETIME NULL,
        DeletedBy         INT NULL
    );

    PRINT 'Created tblChatMessage';
END
ELSE
    PRINT 'tblChatMessage already exists — skipped';

-- ── Table 4: tblChatMessageRead ───────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tblChatMessageRead')
BEGIN
    CREATE TABLE tblChatMessageRead
    (
        IdMessageRead BIGINT IDENTITY(1,1) PRIMARY KEY,
        IdMessage     BIGINT NOT NULL,
        UserId        INT NOT NULL,
        ReadOn        DATETIME NOT NULL DEFAULT GETDATE()
    );

    PRINT 'Created tblChatMessageRead';
END
ELSE
    PRINT 'tblChatMessageRead already exists — skipped';

-- ── Indexes ────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChatConversationMember_UserId')
    CREATE INDEX IX_ChatConversationMember_UserId ON tblChatConversationMember(UserId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChatConversationMember_Conversation')
    CREATE INDEX IX_ChatConversationMember_Conversation ON tblChatConversationMember(IdConversation);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChatMessage_Conversation')
    CREATE INDEX IX_ChatMessage_Conversation ON tblChatMessage(IdConversation);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChatMessage_Sender')
    CREATE INDEX IX_ChatMessage_Sender ON tblChatMessage(SenderUserId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChatMessage_SentOn')
    CREATE INDEX IX_ChatMessage_SentOn ON tblChatMessage(SentOn);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChatConversation_Order')
    CREATE INDEX IX_ChatConversation_Order ON tblChatConversation(IdOrderMaster);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChatMessageRead_Message')
    CREATE INDEX IX_ChatMessageRead_Message ON tblChatMessageRead(IdMessage);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChatMessageRead_User')
    CREATE INDEX IX_ChatMessageRead_User ON tblChatMessageRead(UserId);

PRINT 'Chat indexes created / verified.';
PRINT 'OFMS Quick Chat tables setup complete.';
