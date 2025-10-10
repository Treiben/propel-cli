using System.Text.Json.Serialization;

namespace Propel.FeatureFlags.Admin.Models;

public class FeatureFlag
{
	[JsonPropertyName("key")]
	public string Key { get; set; } = null!;

	[JsonPropertyName("application_name")]
	public string ApplicationName { get; set; } = "global";

	[JsonPropertyName("application_version")]
	public string ApplicationVersion { get; set; } = "0.0.0.0";

	[JsonPropertyName("scope")]
	public int Scope { get; set; } = 0;

	[JsonPropertyName("name")]
	public string Name { get; set; } = null!;

	[JsonPropertyName("description")]
	public string Description { get; set; } = string.Empty;

	[JsonPropertyName("evaluation_modes")]
	public int[] EvaluationModes { get; set; } = [];

	[JsonPropertyName("scheduled_enable_date")]
	public DateTime? ScheduledEnableDate { get; set; }

	[JsonPropertyName("scheduled_disable_date")]
	public DateTime? ScheduledDisableDate { get; set; }

	[JsonPropertyName("window_start_time")]
	public TimeSpan? WindowStartTime { get; set; }

	[JsonPropertyName("window_end_time")]
	public TimeSpan? WindowEndTime { get; set; }

	[JsonPropertyName("time_zone")]
	public string? TimeZone { get; set; }

	[JsonPropertyName("window_days")]
	public int[] WindowDays { get; set; } = [];

	[JsonPropertyName("targeting_rules")]
	public string? TargetingRules { get; set; }

	[JsonPropertyName("enabled_users")]
	public string[] EnabledUsers { get; set; } = [];

	[JsonPropertyName("disabled_users")]
	public string[] DisabledUsers { get; set; } = [];

	[JsonPropertyName("user_percentage_enabled")]
	public int UserPercentageEnabled { get; set; } = 100;

	[JsonPropertyName("enabled_tenants")]
	public string[] EnabledTenants { get; set; } = [];

	[JsonPropertyName("disabled_tenants")]
	public string[] DisabledTenants { get; set; } = [];

	[JsonPropertyName("tenant_percentage_enabled")]
	public int TenantPercentageEnabled { get; set; } = 100;

	[JsonPropertyName("variations")]
	public Dictionary<string, string>? Variations { get; set; }

	[JsonPropertyName("default_variation")]
	public string DefaultVariation { get; set; } = "off";
}
