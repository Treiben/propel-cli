using System.Text.Json.Serialization;

namespace Propel.FeatureFlags.Admin.Models;

public class FeatureFlagAudit
{
	[JsonPropertyName("id")]
	public Guid Id { get; set; }

	[JsonPropertyName("flag_key")]
	public string FlagKey { get; set; } = null!;

	[JsonPropertyName("application_name")]
	public string? ApplicationName { get; set; } = "global";

	[JsonPropertyName("application_version")]
	public string ApplicationVersion { get; set; } = "0.0.0.0";

	[JsonPropertyName("action")]
	public string Action { get; set; } = null!;

	[JsonPropertyName("actor")]
	public string Actor { get; set; } = null!;

	[JsonPropertyName("timestamp")]
	public DateTime Timestamp { get; set; }

	[JsonPropertyName("notes")]
	public string? Notes { get; set; }
}
