using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Propel.FeatureFlags.Admin.Services;
using Propel.FeatureFlags.Migrations.CLI;
using System.CommandLine;

namespace Propel.FeatureFlags.Admin.CLI;

public sealed class SearchCommand(IServiceProvider serviceProvider)
{
	private readonly ILogger<SearchCommand> _logger = serviceProvider.GetRequiredService<ILogger<SearchCommand>>();

	public async Task ExecuteAsync(
						string? connectionString,
						string? key,
						string? name,
						string? description,
						string? format)
	{
		try
		{
			ArgumentNullException.ThrowIfNull(connectionString, nameof(connectionString));

			// Validate that at least one search criterion is provided
			if (string.IsNullOrWhiteSpace(key) &&
				string.IsNullOrWhiteSpace(name) &&
				string.IsNullOrWhiteSpace(description))
			{
				Console.WriteLine("\nError: At least one search criterion required (--key, --name, or --description)");
				Environment.Exit(1);
				return;
			}

			_logger.LogInformation("Searching flags...");

			// Auto-detect provider if not specified
			var provider = ConnectionStringHelper.DetectProvider(connectionString);

			_logger.LogInformation("Provider: {Provider}", provider);

			var administrationService = serviceProvider.GetRequiredService<IAdministrationService>();
			var flags = await administrationService.SearchAsync(connectionString, provider, key, name, description);

			// Default to table format if not specified or invalid
			var outputFormat = string.IsNullOrWhiteSpace(format) ||
							   (!string.Equals(format, "json", StringComparison.OrdinalIgnoreCase) &&
								!string.Equals(format, "table", StringComparison.OrdinalIgnoreCase))
				? "table"
				: format;

			FlagOutputFormatter.DisplayFlags(flags, outputFormat);

			Console.WriteLine("\nSearch criteria applied:");
			if (!string.IsNullOrWhiteSpace(key))
				Console.WriteLine($"  - Key: exact match '{key}'");
			if (!string.IsNullOrWhiteSpace(name))
				Console.WriteLine($"  - Name: contains '{name}' (case-insensitive)");
			if (!string.IsNullOrWhiteSpace(description))
				Console.WriteLine($"  - Description: contains '{description}' (case-insensitive)");
			Console.WriteLine("  - Logic: OR (any criterion match)");
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
		var searchCommand = new Command("find",
			"Search for flags by key, name, or description. " +
			"Search logic: Key uses exact match, Name and Description use case-insensitive partial matching. " +
			"Multiple criteria are combined with OR logic (any match returns the flag). " +
			"Results can be displayed as a table or JSON.");

		// Connection string option
		var connectionStringOption = new Option<string?>("--connection-string")
		{
			Description = "Database connection string (required)",
			IsRequired = true
		};

		var keyOption = new Option<string?>("--key")
		{
			Description = "Search by exact flag key match. Must match the key exactly (case-sensitive).",
			IsRequired = false
		};

		var nameOption = new Option<string?>("--name")
		{
			Description = "Search by flag name. Partial match, case-insensitive. Example: 'payment' matches 'New Payment Processor'.",
			IsRequired = false
		};

		var descriptionOption = new Option<string?>("--description")
		{
			Description = "Search by flag description. Partial match, case-insensitive.",
			IsRequired = false
		};

		var formatOption = new Option<string?>("--format")
		{
			Description = "Output format: 'table' (default) or 'json'. Table format shows key columns, JSON shows complete flag data.",
			IsRequired = false
		};

		searchCommand.AddOption(connectionStringOption);
		searchCommand.AddOption(keyOption);
		searchCommand.AddOption(nameOption);
		searchCommand.AddOption(descriptionOption);
		searchCommand.AddOption(formatOption);

		searchCommand.SetHandler(async (context) =>
		{
			var connectionString = context.ParseResult.GetValueForOption(connectionStringOption);
			var key = context.ParseResult.GetValueForOption(keyOption);
			var name = context.ParseResult.GetValueForOption(nameOption);
			var description = context.ParseResult.GetValueForOption(descriptionOption);
			var format = context.ParseResult.GetValueForOption(formatOption);

			await ExecuteAsync(connectionString, key, name, description, format);
		});

		return searchCommand;
	}
}