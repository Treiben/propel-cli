using Propel.FeatureFlags.Admin.Models;
using System.Text.Json;

namespace Propel.FeatureFlags.Admin.CLI;

public static class FlagOutputFormatter
{
	private readonly static JsonSerializerOptions FlagOutputFormatterOptions = new()
	{
		WriteIndented = true,
		PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
	};
	public static void DisplayFlags(List<FeatureFlag> flags, string format)
	{
		if (flags.Count == 0)
		{
			Console.WriteLine("\nNo flags found");
			return;
		}

		if (string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
		{
			DisplayAsJson(flags);
		}
		else
		{
			DisplayAsTable(flags);
		}
	}

	private static void DisplayAsJson(List<FeatureFlag> flags)
	{
		var json = JsonSerializer.Serialize(flags, FlagOutputFormatterOptions);
		Console.WriteLine($"\n{json}");
	}

	private static void DisplayAsTable(List<FeatureFlag> flags)
	{
		Console.WriteLine($"\nTotal: {flags.Count} flag(s)\n");
		Console.WriteLine($"{"Key",-45} {"Name",-45} {"Modes",-30} {"Description"}");
		Console.WriteLine(new string('-', 140));

		foreach (var flag in flags)
		{
			var modes = string.Join(", ", flag.EvaluationModes.Select(GetEvaluationModeName));
			var description = flag.Description.Length > 50
				? flag.Description[..47] + "..."
				: flag.Description;

			Console.WriteLine($"{flag.Key,-45} {flag.Name,-45} {modes,-30} {description}");
		}

		Console.WriteLine();
	}

	private static string GetEvaluationModeName(int mode)
	{
		return mode switch
		{
			0 => "Off",
			1 => "On",
			2 => "Scheduled",
			3 => "TimeWindow",
			4 => "UserTargeted",
			5 => "UserRollout%",
			6 => "TenantRollout%",
			7 => "TenantTargeted",
			8 => "TargetingRules",
			_ => mode.ToString()
		};
	}
}
