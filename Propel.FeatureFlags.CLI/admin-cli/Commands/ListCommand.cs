using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Propel.FeatureFlags.Admin.Services;
using Propel.FeatureFlags.Migrations.CLI;
using System.CommandLine;

namespace Propel.FeatureFlags.Admin.CLI;

public sealed class ListCommand(IServiceProvider serviceProvider)
{
	private readonly ILogger<ListCommand> _logger = serviceProvider.GetRequiredService<ILogger<ListCommand>>();

	public async Task ExecuteAsync(string? connectionString, string? format)
	{
		try
		{
			ArgumentNullException.ThrowIfNull(connectionString, nameof(connectionString));

			_logger.LogInformation("Listing all global flags...");

			// Auto-detect provider if not specified
			var provider = ConnectionStringHelper.DetectProvider(connectionString);

			_logger.LogInformation("Provider: {Provider}", provider);

			var administrationService = serviceProvider.GetRequiredService<IAdministrationService>();
			var flags = await administrationService.ListAllAsync(connectionString, provider);

			// Default to table format if not specified or invalid
			var outputFormat = string.IsNullOrWhiteSpace(format) ||
							   (!string.Equals(format, "json", StringComparison.OrdinalIgnoreCase) &&
								!string.Equals(format, "table", StringComparison.OrdinalIgnoreCase))
				? "table"
				: format;

			FlagOutputFormatter.DisplayFlags(flags, outputFormat);

			Console.WriteLine("Showing: All global flags (scope=global, version=0.0.0.0)");
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
		var listCommand = new Command("list",
			"List all global feature flags in the database. " +
			"Shows flags with scope=global and version=0.0.0.0 (flags created via CLI or designated as global in application code). " +
			"Application-specific flags (with different scope/version) are not included. " +
			"Results can be displayed as a table or JSON.");

		// Connection string option
		var connectionStringOption = new Option<string?>("--connection-string")
		{
			Description = "Database connection string (required)",
			IsRequired = true
		};

		var formatOption = new Option<string?>("--format")
		{
			Description = "Output format: 'table' (default) or 'json'. Table format shows key columns, JSON shows complete flag data including all properties.",
			IsRequired = false
		};

		listCommand.AddOption(connectionStringOption);
		listCommand.AddOption(formatOption);

		listCommand.SetHandler(async (context) =>
		{
			var connectionString = context.ParseResult.GetValueForOption(connectionStringOption);
			var format = context.ParseResult.GetValueForOption(formatOption);

			await ExecuteAsync(connectionString, format);
		});

		return listCommand;
	}
}