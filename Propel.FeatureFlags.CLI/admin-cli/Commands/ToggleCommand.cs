using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Propel.FeatureFlags.Admin.Services;
using Propel.FeatureFlags.Migrations.CLI;
using System.CommandLine;

namespace Propel.FeatureFlags.Admin.CLI;

public sealed class ToggleCommand(IServiceProvider serviceProvider)
{
	private readonly ILogger<ToggleCommand> _logger = serviceProvider.GetRequiredService<ILogger<ToggleCommand>>();

	public async Task ExecuteAsync(
						string? connectionString,
						string? key,
						string? applicationName,
						bool enable,
						string? username)
	{
		try
		{
			ArgumentNullException.ThrowIfNull(connectionString, nameof(connectionString));
			ArgumentNullException.ThrowIfNull(key, nameof(key));
			ArgumentNullException.ThrowIfNull(username, nameof(username));

			_logger.LogInformation("Toggling flag...");

			// Auto-detect provider if not specified
			var provider = ConnectionStringHelper.DetectProvider(connectionString);

			_logger.LogInformation("Provider: {Provider}", provider);

			var administrationService = serviceProvider.GetRequiredService<IAdministrationService>();
			await administrationService.ToggleModeAsync(connectionString, provider, key, applicationName ?? "global", enable, username);

			string action = enable ? "ENABLED (On)" : "DISABLED (Off)";
			Console.WriteLine($"\nSuccess: Flag '{key}' is now {action}");
			Console.WriteLine("\nIMPORTANT: This operation REPLACES all evaluation modes with simple On/Off.");
			Console.WriteLine("  - Schedules: Removed");
			Console.WriteLine("  - Time windows: Removed");
			Console.WriteLine("  - User/tenant targeting: Removed");
			Console.WriteLine("  - Rollout percentages: Removed");
			Console.WriteLine("  - Targeting rules: Removed");
			Console.WriteLine("\nTo restore advanced modes, update the flag via --json or re-seed the database.");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Operation failed: {Message}", ex.Message);

			Console.WriteLine($"\nError: {ex.Message}");
			Environment.Exit(1);
		}
	}

	public Command Build()
	{
		var toggleCommand = new Command("toggle",
			"Toggle a flag between On and Off states. " +
			"CRITICAL WARNING: This REPLACES all evaluation modes with simple On/Off. " +
			"Any existing schedules, time windows, targeting rules, or rollout percentages will be REMOVED. " +
			"Use this command only for emergency on/off switches. " +
			"For granular control, use --json with the 'create' command or update database directly.");

		// Connection string option
		var connectionStringOption = new Option<string?>("--connection-string")
		{
			Description = "Database connection string (required)",
			IsRequired = true
		};

		var keyOption = new Option<string?>("--key")
		{
			Description = "Flag key to toggle (required). Must match exactly.",
			IsRequired = true
		};

		var applicationOption = new Option<string?>("--application")
		{
			Description = "Application name where flag is originated (required). Must match exactly. Global flags default to 'global' application name.",
			IsRequired = true
		};

		var usernameOption = new Option<string?>("--username")
		{
			Description = "Username for audit trail (required). Used to track who toggled the flag.",
			IsRequired = true
		};

		var onOption = new Option<bool>("--on")
		{
			Description = "Enable the flag (set to On). Mutually exclusive with --off.",
			IsRequired = false
		};

		var offOption = new Option<bool>("--off")
		{
			Description = "Disable the flag (set to Off). Mutually exclusive with --on.",
			IsRequired = false
		};

		toggleCommand.AddOption(connectionStringOption);
		toggleCommand.AddOption(keyOption);
		toggleCommand.AddOption(applicationOption);
		toggleCommand.AddOption(usernameOption);
		toggleCommand.AddOption(onOption);
		toggleCommand.AddOption(offOption);

		toggleCommand.SetHandler(async (context) =>
		{
			var connectionString = context.ParseResult.GetValueForOption(connectionStringOption);
			var key = context.ParseResult.GetValueForOption(keyOption);
			var applicationName = context.ParseResult.GetValueForOption(applicationOption);
			var username = context.ParseResult.GetValueForOption(usernameOption);
			var on = context.ParseResult.GetValueForOption(onOption);
			var off = context.ParseResult.GetValueForOption(offOption);

			if (on && off)
			{
				Console.WriteLine("\nError: Cannot specify both --on and --off");
				Environment.Exit(1);
				return;
			}

			if (!on && !off)
			{
				Console.WriteLine("\nError: Must specify either --on or --off");
				Environment.Exit(1);
				return;
			}

			var enable = on;
			await ExecuteAsync(connectionString, key, applicationName, enable, username);
		});

		return toggleCommand;
	}
}