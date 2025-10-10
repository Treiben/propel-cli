namespace Propel.FeatureFlags.Admin.Models;

/// <summary>
/// Feature flag evaluation modes
/// </summary>
public enum EvaluationMode
{
	/// <summary>
	/// Flag is disabled
	/// </summary>
	Off = 0,

	/// <summary>
	/// Flag is enabled
	/// </summary>
	On = 1,

	/// <summary>
	/// Flag is enabled/disabled based on schedule
	/// </summary>
	Scheduled = 2,

	/// <summary>
	/// Flag is enabled during specific time windows
	/// </summary>
	TimeWindow = 3,

	/// <summary>
	/// Flag is enabled for specific users
	/// </summary>
	UserTargeted = 4,

	/// <summary>
	/// Flag is enabled for a percentage of users
	/// </summary>
	UserRolloutPercentage = 5,

	/// <summary>
	/// Flag is enabled for a percentage of tenants
	/// </summary>
	TenantRolloutPercentage = 6,

	/// <summary>
	/// Flag is enabled for specific tenants
	/// </summary>
	TenantTargeted = 7,

	/// <summary>
	/// Flag evaluation based on custom targeting rules
	/// </summary>
	TargetingRules = 8
}
