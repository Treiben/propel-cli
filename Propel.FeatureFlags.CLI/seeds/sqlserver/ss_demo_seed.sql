-- ====================================================================
-- COMPREHENSIVE FEATURE FLAG EVALUATION MODE EXAMPLES -- TEST SEEDS
-- ====================================================================
-- These examples demonstrate all valid evaluation_modes combinations
--	Off = 0
--	On = 1
--	Scheduled = 2
--	TimeWindow = 3
--	UserTargeted = 4
--	UserRolloutPercentage = 5
--	TenantRolloutPercentage = 6
--	TenantTargeted = 7
--	TargetingRules = 8

-- Targeting rules use operators:
--	Equals = 0
--	NotEquals = 1
--	Contains = 2
--	NotContains = 3
--	In = 4
--	NotIn = 5
--	GreaterThan = 6
--	LessThan = 7

-- Flags used in demo api that will be included in the initial database setup
-- 1. recommendation-algorithm (variations, user targeting)
-- 2. featured-products-launch (user targeting (10 allowed users by user ids)
-- 3. enhanced-catalog-ui (variations, user percentage rollout)

DECLARE @GlobalScope INT = 0;
DECLARE @GlobalName NVARCHAR(255) = 'global';
DECLARE @GlobalVersion NVARCHAR(100) = '0.0.0.0';

DECLARE @AppScope INT = 2;
DECLARE @AppName NVARCHAR(255) = 'WebClientDemo';
DECLARE @AppVersion NVARCHAR(100) = '1.0.0.0';

------------------------------------------------------------------------
DECLARE @FlagKey NVARCHAR(255) = 'admin-panel-enabled';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'Admin Panel Access',
		Description = 'Controls access to administrative panel features including user management, system settings, and sensitive operations',
		EvaluationModes = '[8]',
		Scope = @AppScope,
		WindowStartTime = NULL,
		WindowEndTime = NULL,
		TimeZone = NULL,
		WindowDays = '[]',
		ScheduledEnableDate = NULL,
		ScheduledDisableDate = NULL,
		TenantPercentageEnabled = 100,
		EnabledTenants = '[]',
		DisabledTenants = '[]',
		UserPercentageEnabled = 100,
		EnabledUsers = '[]',
		DisabledUsers = '[]',
		TargetingRules = '[{"attribute":"role","operator":4,"values":["admin","super-admin"],"variation":"on"},{"attribute":"department","operator":4,"values":["engineering","operations"],"variation":"on"}]',
		Variations = '{}',
		DefaultVariation = ''
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes, DefaultVariation, TargetingRules)
	VALUES (@FlagKey, @AppName, @AppVersion, @AppScope, 
		'Admin Panel Access',
		'Controls access to administrative panel features including user management, system settings, and sensitive operations',
		'[8]', 'off',
		'[{"attribute":"role","operator":4,"values":["admin","super-admin"],"variation":"on"},{"attribute":"department","operator":4,"values":["engineering","operations"],"variation":"on"}]');

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @AppName, @AppVersion, '{"category":"security","impact":"high","team":"platform","environment":"all"}', DATEADD(YEAR, 1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @AppName, @AppVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'checkout-version';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'Checkout Processing Version',
		Description = 'Controls which checkout processing implementation is used for A/B testing. Supports v1 (legacy stable), v2 (enhanced with optimizations), and v3 (experimental cutting-edge algorithms). All variations achieve the same business outcome with different technical approaches.',
		EvaluationModes = '[5]',
		Scope = @AppScope,
		Variations = '{"v1":"v1","v2":"v2","v3":"v3"}',
		DefaultVariation = 'v1',
		UserPercentageEnabled = 33
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes, Variations, DefaultVariation, UserPercentageEnabled)
	VALUES (@FlagKey, @AppName, @AppVersion, @AppScope,
		'Checkout Processing Version',
		'Controls which checkout processing implementation is used for A/B testing. Supports v1 (legacy stable), v2 (enhanced with optimizations), and v3 (experimental cutting-edge algorithms). All variations achieve the same business outcome with different technical approaches.',
		'[5]', '{"v1":"v1","v2":"v2","v3":"v3"}', 'v1', 33);

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @AppName, @AppVersion, '{"category":"performance","type":"a-b-test","impact":"medium","team":"checkout","variations":"v1,v2,v3"}', DATEADD(YEAR, 1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @AppName, @AppVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'new-payment-processor';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'New Payment Processor',
		Description = 'Controls whether to use the enhanced payment processing implementation with improved performance and features, or fall back to the legacy processor. Enables gradual rollout with automatic fallback for resilience and risk mitigation during payment processing.',
		EvaluationModes = '[0]',
		Scope = @AppScope
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes)
	VALUES (@FlagKey, @AppName, @AppVersion, @AppScope,
		'New Payment Processor',
		'Controls whether to use the enhanced payment processing implementation with improved performance and features, or fall back to the legacy processor. Enables gradual rollout with automatic fallback for resilience and risk mitigation during payment processing.',
		'[0]');

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @AppName, @AppVersion, '{"category":"payment","type":"implementation-toggle","impact":"high","team":"payments","rollback":"automatic","critical":"true"}', DATEADD(YEAR, 1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @AppName, @AppVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'new-product-api';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'New Product API',
		Description = 'Controls whether to use the new enhanced product API implementation with improved performance and additional product data, or fall back to the legacy API. Enables safe rollout of API improvements without affecting existing functionality.',
		EvaluationModes = '[0]',
		Scope = @AppScope
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes)
	VALUES (@FlagKey, @AppName, @AppVersion, @AppScope,
		'New Product API',
		'Controls whether to use the new enhanced product API implementation with improved performance and additional product data, or fall back to the legacy API. Enables safe rollout of API improvements without affecting existing functionality.',
		'[0]');

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @AppName, @AppVersion, '{"category":"api","type":"implementation-toggle","impact":"medium","team":"product","rollback":"instant"}', DATEADD(YEAR, 1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @AppName, @AppVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'featured-products-launch';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'Featured Products Launch',
		Description = 'Controls the scheduled launch of enhanced featured products display with new promotions and special pricing. Designed for coordinated marketing campaigns and product launches that require precise timing across all platform touchpoints.',
		EvaluationModes = '[2]',
		Scope = @AppScope,
		ScheduledEnableDate = DATEADD(HOUR, 1, GETUTCDATE()),
		ScheduledDisableDate = DATEADD(DAY, 30, GETUTCDATE())
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes, ScheduledEnableDate, ScheduledDisableDate)
	VALUES (@FlagKey, @AppName, @AppVersion, @AppScope,
		'Featured Products Launch',
		'Controls the scheduled launch of enhanced featured products display with new promotions and special pricing. Designed for coordinated marketing campaigns and product launches that require precise timing across all platform touchpoints.',
		'[2]', DATEADD(HOUR, 1, GETUTCDATE()), DATEADD(DAY, 30, GETUTCDATE()));

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @AppName, @AppVersion, '{"category":"marketing","type":"scheduled-launch","impact":"high","team":"product-marketing","coordination":"required"}', DATEADD(YEAR, 1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @AppName, @AppVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'enhanced-catalog-ui';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'Enhanced Catalog UI',
		Description = 'Controls whether to display the enhanced catalog interface with advanced features like detailed analytics, live chat, and smart recommendations. Typically enabled during business hours when customer support is available to assist users with the more complex interface features.',
		EvaluationModes = '[3,5]',
		Scope = @AppScope,
		Variations = '{"enhanced":"enhanced-catalog","legacy":"old-catalog"}',
		DefaultVariation = 'legacy',
		UserPercentageEnabled = 50,
		WindowStartTime = '09:00:00',
		WindowEndTime = '18:00:00',
		TimeZone = 'America/Chicago',
		WindowDays = '[1,2,3,4,5]'
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes, Variations, DefaultVariation, UserPercentageEnabled, WindowStartTime, WindowEndTime, TimeZone, WindowDays)
	VALUES (@FlagKey, @AppName, @AppVersion, @AppScope,
		'Enhanced Catalog UI',
		'Controls whether to display the enhanced catalog interface with advanced features like detailed analytics, live chat, and smart recommendations. Typically enabled during business hours when customer support is available to assist users with the more complex interface features.',
		'[3,5]', '{"enhanced":"enhanced-catalog","legacy":"old-catalog"}', 'legacy', 50, '09:00:00', '18:00:00', 'America/Chicago', '[1,2,3,4,5]');

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @AppName, @AppVersion, '{"category":"ui","type":"time-window","impact":"medium","team":"frontend","support-dependent":"true"}', DATEADD(YEAR, 1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @AppName, @AppVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'recommendation-algorithm';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'Recommendation Algorithm',
		Description = 'Controls which recommendation algorithm implementation is used for generating user recommendations. Supports variations including machine-learning, content-based, and collaborative-filtering algorithms. Enables A/B testing of different technical approaches while maintaining consistent business functionality.',
		EvaluationModes = '[4,8]',
		Scope = @AppScope,
		Variations = '{"collaborative-filtering":"collaborative-filtering","content-based":"content-based","machine-learning":"machine-learning"}',
		DefaultVariation = 'collaborative-filtering',
		TargetingRules = '[{"attribute":"userType","operator":4,"values":["premium","enterprise"],"variation":"machine-learning"},{"attribute":"country","operator":4,"values":["US","CA","UK"],"variation":"content-based"}]',
		EnabledUsers = '["user123","alice.johnson","premium-user-456","ml-tester-789","data-scientist-001"]',
		DisabledUsers = '["blocked-user-999","test-account-disabled","spam-user-123","violator-456"]'
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes, Variations, DefaultVariation, TargetingRules, EnabledUsers, DisabledUsers)
	VALUES (@FlagKey, @AppName, @AppVersion, @AppScope,
		'Recommendation Algorithm',
		'Controls which recommendation algorithm implementation is used for generating user recommendations. Supports variations including machine-learning, content-based, and collaborative-filtering algorithms. Enables A/B testing of different technical approaches while maintaining consistent business functionality.',
		'[4,8]', '{"collaborative-filtering":"collaborative-filtering","content-based":"content-based","machine-learning":"machine-learning"}', 'collaborative-filtering',
		'[{"attribute":"userType","operator":4,"values":["premium","enterprise"],"variation":"machine-learning"},{"attribute":"country","operator":4,"values":["US","CA","UK"],"variation":"content-based"}]',
		'["user123","alice.johnson","premium-user-456","ml-tester-789","data-scientist-001"]',
		'["blocked-user-999","test-account-disabled","spam-user-123","violator-456"]');

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @AppName, @AppVersion, '{"category":"algorithm","type":"variation-test","impact":"medium","team":"recommendations","variations":"ml,content-based,collaborative-filtering","default":"collaborative-filtering"}', DATEADD(YEAR, 1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @AppName, @AppVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'flash-sale-window';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'Flash Sale Time Window',
		Description = 'Shows flash sale products only during business hours (9 AM - 6 PM EST, weekdays)',
		EvaluationModes = '[3]',
		Scope = @AppScope,
		WindowStartTime = '09:00:00',
		WindowEndTime = '18:00:00',
		TimeZone = 'America/New_York',
		WindowDays = '[1,2,3,4,5]'
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes, WindowStartTime, WindowEndTime, TimeZone, WindowDays)
	VALUES (@FlagKey, @AppName, @AppVersion, @AppScope,
		'Flash Sale Time Window',
		'Shows flash sale products only during business hours (9 AM - 6 PM EST, weekdays)',
		'[3]', '09:00:00', '18:00:00', 'America/New_York', '[1,2,3,4,5]');

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @AppName, @AppVersion, '{"service":"products","type":"time-window","component":"flash-sale","promotion":"business-hours","status":"time-window"}', DATEADD(YEAR, 1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @AppName, @AppVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'tenant-percentage-rollout';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'New Dashboard Tenant Rollout',
		Description = 'Progressive rollout of new dashboard to 60% of tenants for gradual deployment',
		EvaluationModes = '[6]',
		Scope = @AppScope,
		TenantPercentageEnabled = 60
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes, TenantPercentageEnabled)
	VALUES (@FlagKey, @AppName, @AppVersion, @AppScope,
		'New Dashboard Tenant Rollout',
		'Progressive rollout of new dashboard to 60% of tenants for gradual deployment',
		'[6]', 60);

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @AppName, @AppVersion, '{"tenant":"rollout","type":"percentage","component":"dashboard","status":"tenant-percentage"}', DATEADD(YEAR, 1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @AppName, @AppVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'holiday-promotions';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'Holiday Promotions with Business Hours',
		Description = 'Holiday promotions active only during scheduled period and within business hours',
		EvaluationModes = '[2,3]',
		Scope = @AppScope,
		Variations = '{"holiday":"holiday-pricing","regular":"standard-pricing","off":"disabled"}',
		DefaultVariation = 'off',
		ScheduledEnableDate = DATEADD(DAY, 2, GETUTCDATE()),
		ScheduledDisableDate = DATEADD(DAY, 10, GETUTCDATE()),
		WindowStartTime = '08:00:00',
		WindowEndTime = '20:00:00',
		TimeZone = 'America/New_York',
		WindowDays = '[1,2,3,4,5,6]'
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes, Variations, DefaultVariation, ScheduledEnableDate, ScheduledDisableDate, WindowStartTime, WindowEndTime, TimeZone, WindowDays)
	VALUES (@FlagKey, @AppName, @AppVersion, @AppScope,
		'Holiday Promotions with Business Hours',
		'Holiday promotions active only during scheduled period and within business hours',
		'[2,3]', '{"holiday":"holiday-pricing","regular":"standard-pricing","off":"disabled"}', 'off',
		DATEADD(DAY, 2, GETUTCDATE()), DATEADD(DAY, 10, GETUTCDATE()), '08:00:00', '20:00:00', 'America/New_York', '[1,2,3,4,5,6]');

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @AppName, @AppVersion, '{"event":"holidays","type":"promotional","constraint":"business-hours","status":"scheduled-with-time-window"}', DATEADD(YEAR, 1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @AppName, @AppVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'beta-features-preview';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'Scheduled Beta Features Preview',
		Description = 'Beta features available to specific user groups during preview period',
		EvaluationModes = '[2,4,8]',
		Scope = @AppScope,
		Variations = '{"beta":"beta-features","standard":"regular-features"}',
		DefaultVariation = 'standard',
		ScheduledEnableDate = DATEADD(DAY, 3, GETUTCDATE()),
		ScheduledDisableDate = DATEADD(DAY, 21, GETUTCDATE()),
		TargetingRules = '[{"attribute":"betaTester","operator":0,"values":["true"],"variation":"beta"},{"attribute":"userLevel","operator":0,"values":["power-user","enterprise"],"variation":"beta"}]',
		EnabledUsers = '["beta-tester-001","power-user-alice","early-adopter-bob","qa-engineer-charlie","product-manager-diana"]',
		DisabledUsers = '["conservative-user-001","stability-focused-eve","production-only-frank","risk-averse-grace"]'
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes, Variations, DefaultVariation, ScheduledEnableDate, ScheduledDisableDate, TargetingRules, EnabledUsers, DisabledUsers)
	VALUES (@FlagKey, @AppName, @AppVersion, @AppScope,
		'Scheduled Beta Features Preview',
		'Beta features available to specific user groups during preview period',
		'[2,4,8]', '{"beta":"beta-features","standard":"regular-features"}', 'standard',
		DATEADD(DAY, 3, GETUTCDATE()), DATEADD(DAY, 21, GETUTCDATE()),
		'[{"attribute":"betaTester","operator":0,"values":["true"],"variation":"beta"},{"attribute":"userLevel","operator":0,"values":["power-user","enterprise"],"variation":"beta"}]',
		'["beta-tester-001","power-user-alice","early-adopter-bob","qa-engineer-charlie","product-manager-diana"]',
		'["conservative-user-001","stability-focused-eve","production-only-frank","risk-averse-grace"]');

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @AppName, @AppVersion, '{"type":"beta-preview","audience":"targeted","status":"scheduled-with-user-targeting"}', DATEADD(YEAR, 1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @AppName, @AppVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'premium-features-rollout';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'Scheduled Premium Features Rollout',
		Description = 'Gradual rollout of premium features starting at scheduled time',
		EvaluationModes = '[2,5]',
		Scope = @AppScope,
		Variations = '{"premium":"premium-tier","standard":"standard-tier"}',
		DefaultVariation = 'standard',
		ScheduledEnableDate = DATEADD(DAY, 1, GETUTCDATE()),
		ScheduledDisableDate = DATEADD(DAY, 60, GETUTCDATE()),
		UserPercentageEnabled = 40
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes, Variations, DefaultVariation, ScheduledEnableDate, ScheduledDisableDate, UserPercentageEnabled)
	VALUES (@FlagKey, @AppName, @AppVersion, @AppScope,
		'Scheduled Premium Features Rollout',
		'Gradual rollout of premium features starting at scheduled time',
		'[2,5]', '{"premium":"premium-tier","standard":"standard-tier"}', 'standard',
		DATEADD(DAY, 1, GETUTCDATE()), DATEADD(DAY, 60, GETUTCDATE()), 40);

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @AppName, @AppVersion, '{"service":"premium","type":"scheduled-rollout","status":"scheduled-with-user-percentage"}', DATEADD(YEAR, 1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @AppName, @AppVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'vip-event-access';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'VIP Event Access with Limited Hours',
		Description = 'VIP users get access to special events during scheduled period and specific hours',
		EvaluationModes = '[2,3,4,8]',
		Scope = @AppScope,
		Variations = '{"vip":"vip-access","standard":"regular-access"}',
		DefaultVariation = 'standard',
		ScheduledEnableDate = DATEADD(DAY, 5, GETUTCDATE()),
		ScheduledDisableDate = DATEADD(DAY, 12, GETUTCDATE()),
		WindowStartTime = '19:00:00',
		WindowEndTime = '23:00:00',
		TimeZone = 'America/New_York',
		WindowDays = '[6,7]',
		TargetingRules = '[{"attribute":"vipStatus","operator":0,"values":["gold","platinum","diamond"],"variation":"vip"},{"attribute":"eventInvited","operator":0,"values":["true"],"variation":"vip"}]',
		EnabledUsers = '["vip-member-001","gold-tier-alice","platinum-user-bob","diamond-member-charlie","event-organizer-diana","special-guest-eve"]',
		DisabledUsers = '["banned-user-001","policy-violator-frank","restricted-account-grace","underage-user-henry"]'
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes, Variations, DefaultVariation, ScheduledEnableDate, ScheduledDisableDate, WindowStartTime, WindowEndTime, TimeZone, WindowDays, TargetingRules, EnabledUsers, DisabledUsers)
	VALUES (@FlagKey, @AppName, @AppVersion, @AppScope,
		'VIP Event Access with Limited Hours',
		'VIP users get access to special events during scheduled period and specific hours',
		'[2,3,4,8]', '{"vip":"vip-access","standard":"regular-access"}', 'standard',
		DATEADD(DAY, 5, GETUTCDATE()), DATEADD(DAY, 12, GETUTCDATE()), '19:00:00', '23:00:00', 'America/New_York', '[6,7]',
		'[{"attribute":"vipStatus","operator":0,"values":["gold","platinum","diamond"],"variation":"vip"},{"attribute":"eventInvited","operator":0,"values":["true"],"variation":"vip"}]',
		'["vip-member-001","gold-tier-alice","platinum-user-bob","diamond-member-charlie","event-organizer-diana","special-guest-eve"]',
		'["banned-user-001","policy-violator-frank","restricted-account-grace","underage-user-henry"]');

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @AppName, @AppVersion, '{"event":"vip-exclusive","type":"access-control","constraint":"scheduled-hours","status":"scheduled-time-window-user-targeted"}', DATEADD(YEAR, 1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @AppName, @AppVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'ultimate-premium-experience';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'Ultimate Premium Experience - Complete Feature Showcase',
		Description = 'The most sophisticated feature flag combining scheduling, time windows, user targeting, and percentage rollout',
		EvaluationModes = '[2,3,4,5,8]',
		Scope = @AppScope,
		Variations = '{"ultimate":"premium-experience","enhanced":"improved-features","standard":"regular-features"}',
		DefaultVariation = 'standard',
		ScheduledEnableDate = DATEADD(DAY, 7, GETUTCDATE()),
		ScheduledDisableDate = DATEADD(DAY, 60, GETUTCDATE()),
		WindowStartTime = '10:00:00',
		WindowEndTime = '16:00:00',
		TimeZone = 'America/New_York',
		WindowDays = '[1,2,3,4,5]',
		UserPercentageEnabled = 20,
		TargetingRules = '[{"attribute":"subscriptionTier","operator":0,"values":["enterprise","platinum"],"variation":"ultimate"},{"attribute":"accountValue","operator":7,"values":["10000"],"variation":"ultimate"},{"attribute":"betaOptIn","operator":0,"values":["true"],"variation":"enhanced"}]',
		EnabledUsers = '["enterprise-admin-001","platinum-member-alice","high-value-bob","beta-champion-charlie","product-evangelist-diana"]',
		DisabledUsers = '["budget-user-001","basic-tier-eve","trial-expired-frank","inactive-account-grace"]'
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes, Variations, DefaultVariation, ScheduledEnableDate, ScheduledDisableDate, WindowStartTime, WindowEndTime, TimeZone, WindowDays, UserPercentageEnabled, TargetingRules, EnabledUsers, DisabledUsers)
	VALUES (@FlagKey, @AppName, @AppVersion, @AppScope,
		'Ultimate Premium Experience - Complete Feature Showcase',
		'The most sophisticated feature flag combining scheduling, time windows, user targeting, and percentage rollout',
		'[2,3,4,5,8]', '{"ultimate":"premium-experience","enhanced":"improved-features","standard":"regular-features"}', 'standard',
		DATEADD(DAY, 7, GETUTCDATE()), DATEADD(DAY, 60, GETUTCDATE()), '10:00:00', '16:00:00', 'America/New_York', '[1,2,3,4,5]', 20,
		'[{"attribute":"subscriptionTier","operator":0,"values":["enterprise","platinum"],"variation":"ultimate"},{"attribute":"accountValue","operator":7,"values":["10000"],"variation":"ultimate"},{"attribute":"betaOptIn","operator":0,"values":["true"],"variation":"enhanced"}]',
		'["enterprise-admin-001","platinum-member-alice","high-value-bob","beta-champion-charlie","product-evangelist-diana"]',
		'["budget-user-001","basic-tier-eve","trial-expired-frank","inactive-account-grace"]');

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @AppName, @AppVersion, '{"tier":"ultimate","complexity":"maximum","showcase":"complete","status":"all-modes-combined"}', DATEADD(YEAR, 1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @AppName, @AppVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'tenant-premium-features';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'Premium Features for Tenants',
		Description = 'Enable advanced features for premium and enterprise tenants only',
		EvaluationModes = '[1]',
		Scope = @AppScope,
		EnabledTenants = '["premium-corp","enterprise-solutions","vip-client-alpha","mega-corp-beta"]'
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes, EnabledTenants)
	VALUES (@FlagKey, @AppName, @AppVersion, @AppScope,
		'Premium Features for Tenants',
		'Enable advanced features for premium and enterprise tenants only',
		'[1]', '["premium-corp","enterprise-solutions","vip-client-alpha","mega-corp-beta"]');

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @AppName, @AppVersion, '{"tenant":"premium","type":"access-control","tier":"premium","status":"enabled-for-tenants"}', DATEADD(YEAR, 1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @AppName, @AppVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'tenant-beta-program';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'Multi-Tenant Beta Program',
		Description = 'Beta features with tenant percentage rollout plus explicit inclusions/exclusions',
		EvaluationModes = '[6]',
		Scope = @AppScope,
		EnabledTenants = '["beta-tester-1","beta-tester-2","early-adopter-corp","tech-forward-inc"]',
		DisabledTenants = '["conservative-corp","legacy-systems-ltd","security-first-org","compliance-strict-co"]',
		TenantPercentageEnabled = 40
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes, EnabledTenants, DisabledTenants, TenantPercentageEnabled)
	VALUES (@FlagKey, @AppName, @AppVersion, @AppScope,
		'Multi-Tenant Beta Program',
		'Beta features with tenant percentage rollout plus explicit inclusions/exclusions',
		'[6]', '["beta-tester-1","beta-tester-2","early-adopter-corp","tech-forward-inc"]',
		'["conservative-corp","legacy-systems-ltd","security-first-org","compliance-strict-co"]', 40);

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @AppName, @AppVersion, '{"tenant":"multi","type":"beta-program","phase":"phase1","status":"tenant-percentage-with-lists"}', DATEADD(YEAR, 1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @AppName, @AppVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'legacy-checkout-v1';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'Legacy Checkout System V1',
		Description = 'Old checkout system that was deprecated and expired yesterday',
		EvaluationModes = '[0]',
		Scope = @AppScope
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes)
	VALUES (@FlagKey, @AppName, @AppVersion, @AppScope,
		'Legacy Checkout System V1',
		'Old checkout system that was deprecated and expired yesterday',
		'[0]');

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @AppName, @AppVersion, '{"service":"checkout","type":"legacy","status":"expired","deprecated":"true"}', DATEADD(DAY, -1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @AppName, @AppVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'old-search-algorithm';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'Deprecated Search Algorithm',
		Description = 'Previous search implementation that expired yesterday - was at 15% rollout',
		EvaluationModes = '[5]',
		Scope = @AppScope,
		Variations = '{"enhanced":"enhanced-search","legacy":"old-search"}',
		DefaultVariation = 'legacy',
		UserPercentageEnabled = 15
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes, Variations, DefaultVariation, UserPercentageEnabled)
	VALUES (@FlagKey, @AppName, @AppVersion, @AppScope,
		'Deprecated Search Algorithm',
		'Previous search implementation that expired yesterday - was at 15% rollout',
		'[5]', '{"enhanced":"enhanced-search","legacy":"old-search"}', 'legacy', 15);

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @AppName, @AppVersion, '{"service":"search","type":"algorithm","status":"expired","rollout":"partial"}', DATEADD(DAY, -1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @AppName, @AppVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'experimental-analytics';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'Experimental Analytics Dashboard',
		Description = 'Experimental analytics feature that was being tested with select tenants - expired yesterday',
		EvaluationModes = '[7]',
		Scope = @AppScope,
		EnabledTenants = '["pilot-tenant-1","beta-analytics-corp","test-organization"]'
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes, EnabledTenants)
	VALUES (@FlagKey, @AppName, @AppVersion, @AppScope,
		'Experimental Analytics Dashboard',
		'Experimental analytics feature that was being tested with select tenants - expired yesterday',
		'[7]', '["pilot-tenant-1","beta-analytics-corp","test-organization"]');

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @AppName, @AppVersion, '{"service":"analytics","type":"experimental","status":"expired","phase":"pilot"}', DATEADD(DAY, -1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @AppName, @AppVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'mobile-app-redesign-pilot';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'Mobile App Redesign Pilot Program',
		Description = 'Mobile redesign that was targeted to specific user groups - expired yesterday',
		EvaluationModes = '[4,8]',
		Scope = @AppScope,
		Variations = '{"new":"redesigned-mobile","old":"classic-mobile"}',
		DefaultVariation = 'old',
		TargetingRules = '[{"attribute":"mobileVersion","operator":6,"values":["2.0.0"],"variation":"new"},{"attribute":"betaTester","operator":0,"values":["true"],"variation":"new"}]',
		EnabledUsers = '["mobile-tester-001","ui-designer-alice","beta-user-bob","app-developer-charlie"]',
		DisabledUsers = '["old-device-user-001","stability-user-diana","conservative-mobile-eve"]'
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes, Variations, DefaultVariation, TargetingRules, EnabledUsers, DisabledUsers)
	VALUES (@FlagKey, @AppName, @AppVersion, @AppScope,
		'Mobile App Redesign Pilot Program',
		'Mobile redesign that was targeted to specific user groups - expired yesterday',
		'[4,8]', '{"new":"redesigned-mobile","old":"classic-mobile"}', 'old',
		'[{"attribute":"mobileVersion","operator":6,"values":["2.0.0"],"variation":"new"},{"attribute":"betaTester","operator":0,"values":["true"],"variation":"new"}]',
		'["mobile-tester-001","ui-designer-alice","beta-user-bob","app-developer-charlie"]',
		'["old-device-user-001","stability-user-diana","conservative-mobile-eve"]');

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @AppName, @AppVersion, '{"service":"mobile","type":"redesign","status":"expired","platform":"ios-android"}', DATEADD(DAY, -1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @AppName, @AppVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'weekend-flash-sale-q3';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'Q3 Weekend Flash Sale Campaign',
		Description = 'Limited time weekend flash sale that ran during Q3 and expired yesterday',
		EvaluationModes = '[2,3]',
		Scope = @AppScope,
		Variations = '{"sale":"flash-sale-prices","regular":"standard-prices"}',
		DefaultVariation = 'regular',
		ScheduledEnableDate = DATEADD(DAY, -30, GETUTCDATE()),
		ScheduledDisableDate = DATEADD(DAY, -2, GETUTCDATE()),
		WindowStartTime = '00:00:00',
		WindowEndTime = '23:59:59',
		TimeZone = 'UTC',
		WindowDays = '[5,6]'
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes, Variations, DefaultVariation, ScheduledEnableDate, ScheduledDisableDate, WindowStartTime, WindowEndTime, TimeZone, WindowDays)
	VALUES (@FlagKey, @AppName, @AppVersion, @AppScope,
		'Q3 Weekend Flash Sale Campaign',
		'Limited time weekend flash sale that ran during Q3 and expired yesterday',
		'[2,3]', '{"sale":"flash-sale-prices","regular":"standard-prices"}', 'regular',
		DATEADD(DAY, -30, GETUTCDATE()), DATEADD(DAY, -2, GETUTCDATE()), '00:00:00', '23:59:59', 'UTC', '[5,6]');

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @AppName, @AppVersion, '{"campaign":"q3-flash-sale","type":"promotional","status":"expired","period":"weekend"}', DATEADD(DAY, -1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @AppName, @AppVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'enterprise-analytics-suite';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'Enterprise Analytics Suite',
		Description = 'Advanced analytics features targeted to specific enterprise tenants',
		EvaluationModes = '[7]',
		Scope = @AppScope,
		EnabledTenants = '["acme-corp","global-industries","tech-giants-inc","innovation-labs","enterprise-solutions-ltd"]',
		DisabledTenants = '["startup-company","small-business-co","trial-tenant","free-tier-org"]'
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes, EnabledTenants, DisabledTenants)
	VALUES (@FlagKey, @AppName, @AppVersion, @AppScope,
		'Enterprise Analytics Suite',
		'Advanced analytics features targeted to specific enterprise tenants',
		'[7]', '["acme-corp","global-industries","tech-giants-inc","innovation-labs","enterprise-solutions-ltd"]',
		'["startup-company","small-business-co","trial-tenant","free-tier-org"]');

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @AppName, @AppVersion, '{"service":"analytics","type":"enterprise","tier":"advanced","status":"tenant-targeted"}', DATEADD(YEAR, 1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @AppName, @AppVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'white-label-branding';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'White Label Branding Features',
		Description = 'Custom branding and white-label features for select partners and enterprise clients',
		EvaluationModes = '[7]',
		Scope = @AppScope,
		Variations = '{"custom":"white-label-enabled","standard":"default-branding"}',
		DefaultVariation = 'standard',
		EnabledTenants = '["partner-alpha","partner-beta","white-label-corp","custom-brand-inc","enterprise-partner-solutions","mega-client-xyz"]',
		DisabledTenants = '["competitor-company","unauthorized-reseller","blocked-partner"]'
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes, Variations, DefaultVariation, EnabledTenants, DisabledTenants)
	VALUES (@FlagKey, @AppName, @AppVersion, @AppScope,
		'White Label Branding Features',
		'Custom branding and white-label features for select partners and enterprise clients',
		'[7]', '{"custom":"white-label-enabled","standard":"default-branding"}', 'standard',
		'["partner-alpha","partner-beta","white-label-corp","custom-brand-inc","enterprise-partner-solutions","mega-client-xyz"]',
		'["competitor-company","unauthorized-reseller","blocked-partner"]');

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @AppName, @AppVersion, '{"service":"branding","type":"white-label","partner":"enterprise","status":"tenant-targeted"}', DATEADD(YEAR, 1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @AppName, @AppVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'compliance-reporting-module';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'Advanced Compliance Reporting',
		Description = 'SOX, GDPR, and HIPAA compliance reporting features for regulated industry tenants',
		EvaluationModes = '[7]',
		Scope = @AppScope,
		Variations = '{"compliance":"full-compliance-suite","basic":"standard-reporting"}',
		DefaultVariation = 'basic',
		EnabledTenants = '["healthcare-corp","financial-services-inc","pharma-company","bank-holdings","insurance-giant","government-agency"]',
		DisabledTenants = '["non-regulated-startup","consumer-app-company","entertainment-corp"]'
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes, Variations, DefaultVariation, EnabledTenants, DisabledTenants)
	VALUES (@FlagKey, @AppName, @AppVersion, @AppScope,
		'Advanced Compliance Reporting',
		'SOX, GDPR, and HIPAA compliance reporting features for regulated industry tenants',
		'[7]', '{"compliance":"full-compliance-suite","basic":"standard-reporting"}', 'basic',
		'["healthcare-corp","financial-services-inc","pharma-company","bank-holdings","insurance-giant","government-agency"]',
		'["non-regulated-startup","consumer-app-company","entertainment-corp"]');

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @AppName, @AppVersion, '{"service":"compliance","type":"reporting","regulation":"multi","status":"tenant-targeted"}', DATEADD(YEAR, 1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @AppName, @AppVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'advanced-search-engine';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'Advanced Search Engine Rollout',
		Description = 'Progressive rollout of enhanced search capabilities to 75% of tenants',
		EvaluationModes = '[6]',
		Scope = @AppScope,
		Variations = '{"enhanced":"advanced-search","legacy":"basic-search"}',
		DefaultVariation = 'legacy',
		TenantPercentageEnabled = 75
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes, Variations, DefaultVariation, TenantPercentageEnabled)
	VALUES (@FlagKey, @AppName, @AppVersion, @AppScope,
		'Advanced Search Engine Rollout',
		'Progressive rollout of enhanced search capabilities to 75% of tenants',
		'[6]', '{"enhanced":"advanced-search","legacy":"basic-search"}', 'legacy', 75);

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @AppName AS ApplicationName, @AppVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @AppName, @AppVersion, '{"service":"search","type":"engine-upgrade","performance":"enhanced","status":"tenant-percentage"}', DATEADD(YEAR, 1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @AppName, @AppVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
-- global flags

------------------------------------------------------------------------
SET @FlagKey = 'real-time-notifications';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @GlobalName AS ApplicationName, @GlobalVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'Real-time Notification System',
		Description = 'WebSocket-based real-time notifications rolled out to 45% of tenants',
		EvaluationModes = '[6]',
		Scope = @GlobalScope,
		Variations = '{"realtime":"websocket-notifications","polling":"traditional-polling"}',
		DefaultVariation = 'polling',
		TenantPercentageEnabled = 45
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes, Variations, DefaultVariation, TenantPercentageEnabled)
	VALUES (@FlagKey, @GlobalName, @GlobalVersion, @GlobalScope,
		'Real-time Notification System',
		'WebSocket-based real-time notifications rolled out to 45% of tenants',
		'[6]', '{"realtime":"websocket-notifications","polling":"traditional-polling"}', 'polling', 45);

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @GlobalName AS ApplicationName, @GlobalVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @GlobalName, @GlobalVersion, '{"service":"notifications","type":"realtime","transport":"websocket","status":"tenant-percentage"}', DATEADD(YEAR, 1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @GlobalName, @GlobalVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'multi-region-backup';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @GlobalName AS ApplicationName, @GlobalVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'Multi-Region Data Backup',
		Description = 'Automated multi-region backup system gradually rolled out to 30% of tenants',
		EvaluationModes = '[6]',
		Scope = @GlobalScope,
		Variations = '{"multi-region":"geo-distributed-backup","single-region":"local-backup-only"}',
		DefaultVariation = 'single-region',
		TenantPercentageEnabled = 30
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes, Variations, DefaultVariation, TenantPercentageEnabled)
	VALUES (@FlagKey, @GlobalName, @GlobalVersion, @GlobalScope,
		'Multi-Region Data Backup',
		'Automated multi-region backup system gradually rolled out to 30% of tenants',
		'[6]', '{"multi-region":"geo-distributed-backup","single-region":"local-backup-only"}', 'single-region', 30);

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @GlobalName AS ApplicationName, @GlobalVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @GlobalName, @GlobalVersion, '{"service":"backup","type":"multi-region","reliability":"high","status":"tenant-percentage"}', DATEADD(YEAR, 1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @GlobalName, @GlobalVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'performance-monitoring-v2';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @GlobalName AS ApplicationName, @GlobalVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'Enhanced Performance Monitoring',
		Description = 'Next-generation performance monitoring and APM features for 85% of tenants',
		EvaluationModes = '[6]',
		Scope = @GlobalScope,
		Variations = '{"v2":"enhanced-monitoring","v1":"basic-monitoring"}',
		DefaultVariation = 'v1',
		TenantPercentageEnabled = 85
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes, Variations, DefaultVariation, TenantPercentageEnabled)
	VALUES (@FlagKey, @GlobalName, @GlobalVersion, @GlobalScope,
		'Enhanced Performance Monitoring',
		'Next-generation performance monitoring and APM features for 85% of tenants',
		'[6]', '{"v2":"enhanced-monitoring","v1":"basic-monitoring"}', 'v1', 85);

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @GlobalName AS ApplicationName, @GlobalVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @GlobalName, @GlobalVersion, '{"service":"monitoring","type":"performance","version":"v2","status":"tenant-percentage"}', DATEADD(YEAR, 1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @GlobalName, @GlobalVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'priority-support-hours';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @GlobalName AS ApplicationName, @GlobalVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'Priority Support During Business Hours',
		Description = 'Enhanced support features for premium users during business hours',
		EvaluationModes = '[3,4]',
		Scope = @GlobalScope,
		Variations = '{"priority":"priority-support","standard":"regular-support"}',
		DefaultVariation = 'standard',
		WindowStartTime = '08:00:00',
		WindowEndTime = '18:00:00',
		TimeZone = 'America/New_York',
		WindowDays = '[1,2,3,4,5]',
		TargetingRules = '[{"attribute":"subscriptionTier","operator":0,"values":["premium","enterprise"],"variation":"priority"},{"attribute":"supportLevel","operator":0,"values":["gold","platinum"],"variation":"priority"}]',
		EnabledUsers = '["premium-customer-001","enterprise-admin-alice","gold-support-bob","platinum-user-charlie","vip-customer-diana"]',
		DisabledUsers = '["basic-user-001","free-tier-eve","trial-account-frank","suspended-user-grace"]'
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes, Variations, DefaultVariation, WindowStartTime, WindowEndTime, TimeZone, WindowDays, TargetingRules, EnabledUsers, DisabledUsers)
	VALUES (@FlagKey, @GlobalName, @GlobalVersion, @GlobalScope,
		'Priority Support During Business Hours',
		'Enhanced support features for premium users during business hours',
		'[3,4]', '{"priority":"priority-support","standard":"regular-support"}', 'standard',
		'08:00:00', '18:00:00', 'America/New_York', '[1,2,3,4,5]',
		'[{"attribute":"subscriptionTier","operator":0,"values":["premium","enterprise"],"variation":"priority"},{"attribute":"supportLevel","operator":0,"values":["gold","platinum"],"variation":"priority"}]',
		'["premium-customer-001","enterprise-admin-alice","gold-support-bob","platinum-user-charlie","vip-customer-diana"]',
		'["basic-user-001","free-tier-eve","trial-account-frank","suspended-user-grace"]');

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @GlobalName AS ApplicationName, @GlobalVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @GlobalName, @GlobalVersion, '{"service":"support","type":"priority","constraint":"business-hours","status":"time-window-with-user-targeting"}', DATEADD(YEAR, 1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @GlobalName, @GlobalVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'peak-hours-optimization';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @GlobalName AS ApplicationName, @GlobalVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'Peak Hours Performance Optimization',
		Description = 'Enable performance optimizations for subset of users during peak hours',
		EvaluationModes = '[3,5]',
		Scope = @GlobalScope,
		Variations = '{"optimized":"performance-mode","standard":"normal-mode"}',
		DefaultVariation = 'standard',
		WindowStartTime = '10:00:00',
		WindowEndTime = '14:00:00',
		TimeZone = 'America/New_York',
		WindowDays = '[1,2,3,4,5]',
		UserPercentageEnabled = 60
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes, Variations, DefaultVariation, WindowStartTime, WindowEndTime, TimeZone, WindowDays, UserPercentageEnabled)
	VALUES (@FlagKey, @GlobalName, @GlobalVersion, @GlobalScope,
		'Peak Hours Performance Optimization',
		'Enable performance optimizations for subset of users during peak hours',
		'[3,5]', '{"optimized":"performance-mode","standard":"normal-mode"}', 'standard',
		'10:00:00', '14:00:00', 'America/New_York', '[1,2,3,4,5]', 60);

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @GlobalName AS ApplicationName, @GlobalVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, ExpirationDate)
	VALUES (@FlagKey, @GlobalName, @GlobalVersion, '{"service":"performance","type":"optimization","constraint":"peak-hours","status":"time-window-with-user-percentage"}', DATEADD(YEAR, 1, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @GlobalName, @GlobalVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

------------------------------------------------------------------------
SET @FlagKey = 'api-maintenance';

MERGE INTO FeatureFlags AS target
USING (SELECT @FlagKey AS [Key], @GlobalName AS ApplicationName, @GlobalVersion AS ApplicationVersion) AS source
ON target.[Key] = source.[Key] AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN MATCHED THEN
	UPDATE SET
		Name = 'API Maintenance Mode',
		Description = 'When enabled, API endpoints return maintenance responses - disabled by default',
		EvaluationModes = '[0]',
		Scope = @GlobalScope
WHEN NOT MATCHED THEN
	INSERT ([Key], ApplicationName, ApplicationVersion, Scope, Name, Description, EvaluationModes)
	VALUES (@FlagKey, @GlobalName, @GlobalVersion, @GlobalScope,
		'API Maintenance Mode',
		'When enabled, API endpoints return maintenance responses - disabled by default',
		'[0]');

MERGE INTO FeatureFlagsMetadata AS target
USING (SELECT @FlagKey AS FlagKey, @GlobalName AS ApplicationName, @GlobalVersion AS ApplicationVersion) AS source
ON target.FlagKey = source.FlagKey AND target.ApplicationName = source.ApplicationName AND target.ApplicationVersion = source.ApplicationVersion
WHEN NOT MATCHED THEN
	INSERT (FlagKey, ApplicationName, ApplicationVersion, Tags, IsPermanent, ExpirationDate)
	VALUES (@FlagKey, @GlobalName, @GlobalVersion, '{"service":"api","type":"maintenance","status":"disabled"}', 1, DATEADD(YEAR, 10, GETUTCDATE()));

INSERT INTO FeatureFlagsAudit (FlagKey, ApplicationName, ApplicationVersion, [Action], Actor, [Timestamp], Notes)
VALUES (@FlagKey, @GlobalName, @GlobalVersion, 'flag-created', @AppName, GETUTCDATE(), 'Initial creation from seed script');

PRINT 'Demo seed data inserted successfully';
GO
