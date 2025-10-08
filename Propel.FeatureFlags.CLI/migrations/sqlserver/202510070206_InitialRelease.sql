-- =============================================================================
-- Initial schema for Feature Flags system
-- Database: SQL Server
-- Version: 20250928033300
-- =============================================================================

-- Create the FeatureFlags table
CREATE TABLE [dbo].[FeatureFlags] (
    -- Flag uniqueness scope
    [Key] NVARCHAR(255) NOT NULL,
    [ApplicationName] NVARCHAR(255) NOT NULL DEFAULT 'global',
    [ApplicationVersion] NVARCHAR(100) NOT NULL DEFAULT '0.0.0.0',
    [Scope] INT NOT NULL DEFAULT 0,
    
    -- Descriptive fields
    [Name] NVARCHAR(500) NOT NULL,
    [Description] NVARCHAR(MAX) NOT NULL DEFAULT '',

    -- Evaluation modes
    [EvaluationModes] NVARCHAR(MAX) NOT NULL DEFAULT '[]'
        CONSTRAINT CK_FeatureFlags_EvaluationModes_JSON CHECK (ISJSON([EvaluationModes]) = 1),
    
    -- Scheduling
    [ScheduledEnableDate] DATETIMEOFFSET NULL,
    [ScheduledDisableDate] DATETIMEOFFSET NULL,
    
    -- Time Windows
    [WindowStartTime] TIME NULL,
    [WindowEndTime] TIME NULL,
    [TimeZone] NVARCHAR(100) NULL,
    [WindowDays] NVARCHAR(MAX) NOT NULL DEFAULT '[]'
        CONSTRAINT CK_FeatureFlags_WindowDays_JSON CHECK (ISJSON([WindowDays]) = 1),
    
    -- Targeting
    [TargetingRules] NVARCHAR(MAX) NOT NULL DEFAULT '[]'
        CONSTRAINT CK_FeatureFlags_TargetingRules_JSON CHECK (ISJSON([TargetingRules]) = 1),

    -- User-level controls
    [EnabledUsers] NVARCHAR(MAX) NOT NULL DEFAULT '[]'
        CONSTRAINT CK_FeatureFlags_EnabledUsers_JSON CHECK (ISJSON([EnabledUsers]) = 1),
    [DisabledUsers] NVARCHAR(MAX) NOT NULL DEFAULT '[]'
        CONSTRAINT CK_FeatureFlags_DisabledUsers_JSON CHECK (ISJSON([DisabledUsers]) = 1),
    [UserPercentageEnabled] INT NOT NULL DEFAULT 100 
        CONSTRAINT CK_FeatureFlags_UserPercentageEnabled CHECK ([UserPercentageEnabled] >= 0 AND [UserPercentageEnabled] <= 100),

    -- Tenant-level controls
    [EnabledTenants] NVARCHAR(MAX) NOT NULL DEFAULT '[]'
        CONSTRAINT CK_FeatureFlags_EnabledTenants_JSON CHECK (ISJSON([EnabledTenants]) = 1),
    [DisabledTenants] NVARCHAR(MAX) NOT NULL DEFAULT '[]'
        CONSTRAINT CK_FeatureFlags_DisabledTenants_JSON CHECK (ISJSON([DisabledTenants]) = 1),
    [TenantPercentageEnabled] INT NOT NULL DEFAULT 100 
        CONSTRAINT CK_FeatureFlags_TenantPercentageEnabled CHECK ([TenantPercentageEnabled] >= 0 AND [TenantPercentageEnabled] <= 100),
    
    -- Variations
    [Variations] NVARCHAR(MAX) NOT NULL DEFAULT '{}'
        CONSTRAINT CK_FeatureFlags_Variations_JSON CHECK (ISJSON([Variations]) = 1),
    [DefaultVariation] NVARCHAR(255) NOT NULL DEFAULT 'off',

    CONSTRAINT PK_FeatureFlags PRIMARY KEY ([Key], [ApplicationName], [ApplicationVersion])
);

PRINT 'Table [FeatureFlags] created successfully';
GO

-- Create the FeatureFlagsMetadata table
CREATE TABLE [dbo].[FeatureFlagsMetadata] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [FlagKey] NVARCHAR(255) NOT NULL,

    -- Flag uniqueness scope
    [ApplicationName] NVARCHAR(255) NOT NULL DEFAULT 'global',
    [ApplicationVersion] NVARCHAR(100) NOT NULL DEFAULT '0.0.0.0',

    -- Retention and expiration
    [IsPermanent] BIT NOT NULL DEFAULT 0,
    [ExpirationDate] DATETIMEOFFSET NOT NULL,

    -- Tags for categorization
    [Tags] NVARCHAR(MAX) NOT NULL DEFAULT '{}'
        CONSTRAINT CK_FeatureFlagsMetadata_Tags_JSON CHECK (ISJSON([Tags]) = 1)
);

PRINT 'Table [FeatureFlagsMetadata] created successfully';
GO

-- Create the FeatureFlagsAudit table
CREATE TABLE [dbo].[FeatureFlagsAudit] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [FlagKey] NVARCHAR(255) NOT NULL,

    -- Flag uniqueness scope
    [ApplicationName] NVARCHAR(255) NULL DEFAULT 'global',
    [ApplicationVersion] NVARCHAR(100) NOT NULL DEFAULT '0.0.0.0',

    -- Action details
    [Action] NVARCHAR(50) NOT NULL,
    [Actor] NVARCHAR(255) NOT NULL,
    [Timestamp] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
    [Notes] NVARCHAR(MAX) NULL
);

PRINT 'Table [FeatureFlagsAudit] created successfully';
GO

PRINT 'Initial schema migration completed successfully';

-- DOWN
-- Rollback script to drop all tables

DROP TABLE IF EXISTS [dbo].[FeatureFlagsAudit];
PRINT 'Table [FeatureFlagsAudit] dropped';
GO

DROP TABLE IF EXISTS [dbo].[FeatureFlagsMetadata];
PRINT 'Table [FeatureFlagsMetadata] dropped';
GO

DROP TABLE IF EXISTS [dbo].[FeatureFlags];
PRINT 'Table [FeatureFlags] dropped';
GO

PRINT 'Rollback completed successfully';
