using System.Text.Json.Serialization;

namespace Propel.FeatureFlags.Admin.Models;

public class FeatureFlagMetadata
{
	[JsonPropertyName("id")]
	public Guid Id { get; set; }

	[JsonPropertyName("flag_key")]
	public string FlagKey { get; set; } = null!;

	[JsonPropertyName("application_name")]
	public string ApplicationName { get; set; } = "global";

	[JsonPropertyName("application_version")]
	public string ApplicationVersion { get; set; } = "0.0.0.0";

	[JsonPropertyName("is_permanent")]
	public bool IsPermanent { get; set; } = false;

	[JsonPropertyName("expiration_date")]
	public DateTime ExpirationDate { get; set; }

	[JsonPropertyName("tags")]
	public Dictionary<string, string>? Tags { get; set; }
}
