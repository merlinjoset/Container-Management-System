-- Roles
CREATE TABLE dbo.Roles
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
    Name NVARCHAR(256) NOT NULL
);
-- User list
IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id                   UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        UserName              NVARCHAR(256)     NULL,
        Email                 NVARCHAR(256)     NULL,
        EmailConfirmed        BIT              NOT NULL,
        PasswordHash          NVARCHAR(MAX)     NULL,
        PhoneNumber           NVARCHAR(MAX)     NULL,
        PhoneNumberConfirmed  BIT              NOT NULL,
        TwoFactorEnabled      BIT              NOT NULL,
        LockoutEnd            DATETIMEOFFSET   NULL,
        LockoutEnabled        BIT              NOT NULL,
        AccessFailedCount     INT              NOT NULL
    );

END
GO
-- User Roles
IF OBJECT_ID('dbo.UserRoles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserRoles
    (
        UserId UNIQUEIDENTIFIER NOT NULL,
        RoleId UNIQUEIDENTIFIER NOT NULL,

        CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),

        CONSTRAINT FK_Users_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );

END
GO
-- User Login History
IF OBJECT_ID('dbo.UserLogins', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserLogins
    (
        LoginProvider       NVARCHAR(128)    NOT NULL,
        ProviderKey         NVARCHAR(128)    NOT NULL,
        ProviderDisplayName NVARCHAR(MAX)    NULL,
        UserId              UNIQUEIDENTIFIER NOT NULL,

        CONSTRAINT PK_UserLogins PRIMARY KEY (LoginProvider, ProviderKey),

        CONSTRAINT FK_UserLogins_Users_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );

END
GO
-- User Token
IF OBJECT_ID('dbo.UserTokens', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserTokens
    (
        UserId        UNIQUEIDENTIFIER NOT NULL,
        LoginProvider NVARCHAR(128)    NOT NULL,
        Name          NVARCHAR(128)    NOT NULL,
        Value         NVARCHAR(MAX)    NULL,

        CONSTRAINT PK_UserTokens PRIMARY KEY (UserId, LoginProvider, Name),

        CONSTRAINT FK_UserTokens_Users_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );
END
GO

-- PaymentProviders
IF OBJECT_ID('dbo.PaymentProviders', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PaymentProviders
    (
        Id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PaymentProviders PRIMARY KEY,
        [Name]        NVARCHAR(128)     NOT NULL,
        IsActive    BIT              NOT NULL CONSTRAINT DF_PaymentProviders_IsActive DEFAULT (1),
        EndpointUrl NVARCHAR(512)     NOT NULL
    );

    CREATE INDEX IX_PaymentProviders_Name ON dbo.PaymentProviders(Name);
END
GO

-- Payments
IF OBJECT_ID('dbo.Payments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Payments
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Payments PRIMARY KEY,
        UserId          UNIQUEIDENTIFIER NOT NULL, -- Identity user id (logical)
        ProviderId      UNIQUEIDENTIFIER NOT NULL,
        Amount          DECIMAL(18,2)    NOT NULL,
        Currency        NVARCHAR(3)      NOT NULL,        
	    CardExpMonth [int] NOT NULL,
	    CardExpYear [int] NOT NULL,
	    CardLast4  NVARCHAR(64)      NOT NULL,  
        Reference       NVARCHAR(64)     NOT NULL,
        [Status]          INT              NOT NULL,
        AttemptCount    INT              NOT NULL CONSTRAINT DF_Payments_AttemptCount DEFAULT (0),
        CreationTimeUtc DATETIME2        NOT NULL CONSTRAINT DF_Payments_CreationTimeUtc DEFAULT (SYSUTCDATETIME()),
        LastTriedAtUtc  DATETIME2        NULL,
        LastError       NVARCHAR(MAX)    NULL,

        CONSTRAINT FK_Payments_PaymentProviders_ProviderId
            FOREIGN KEY (ProviderId) REFERENCES dbo.PaymentProviders(Id)
    );
END
GO