using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Propel.FeatureFlags.Admin.Services;
using Propel.FeatureFlags.Domain;
using Propel.FeatureFlags.Migrations.CLI;
using System.CommandLine;

namespace Propel.FeatureFlags.Admin.CLI;

public class CreateFeatureFlag : IFeatureFlag
{
	public string Key { get; set; } = null!;
	public string? Name { get; set; }
	public string? Description { get; set; }
	public Dictionary<string, string>? Tags { get; set; }
	public EvaluationMode OnOffMode { get; set; }
}

public sealed class CreateCommand(IServiceProvider serviceProvider)
{
	private readonly ILogger<CreateCommand> _logger = serviceProvider.GetRequiredService<ILogger<CreateCommand>>();

	public async Task ExecuteAsync(
						string? connectionString,
						string? jsonString,
						string? key,
						string? name,
						string? description,
						string? tags,
						string? status,
						string? username)
	{
		try
		{
			ArgumentNullException.ThrowIfNull(connectionString, nameof(connectionString));
			ArgumentNullException.ThrowIfNull(username, nameof(username));

			_logger.LogInformation("Creating new global flag...");

			var evaluationMode = string.Equals(status, "on", StringComparison.OrdinalIgnoreCase) ? EvaluationMode.On : EvaluationMode.Off;

			string finalJson = jsonString ?? System.Text.Json.JsonSerializer.Serialize(new CreateFeatureFlag
			{
				Key = key ?? throw new ArgumentNullException(nameof(key), "Flag key is required when not using --json"),
				Name = name ?? key,
				Description = description ?? $"Created via CLI by {username}",
				Tags = string.IsNullOrEmpty(tags) ? null : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(tags),
				OnOffMode = evaluationMode
			});

			// Auto-detect provider if not specified
			var provider = ConnectionStringHelper.DetectProvider(connectionString);

			_logger.LogInformation("Provider: {Provider}", provider);

			var administrationService = serviceProvider.GetRequiredService<IAdministrationService>();
			await administrationService.CreateAsync(connectionString, provider, finalJson, username);

			Console.WriteLine($"\nSuccess: Global flag '{key ?? "from JSON"}' created");
			Console.WriteLine("Note: Flag defaults - scope=global, version=0.0.0.0, expiration=45 days");
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
		var createCommand = new Command("create",
			"Create a new GLOBAL feature flag. " +
			"Simple mode: Use --key, --name, --status for basic On/Off flags. " +
			"Advanced mode: Use --json for flags with scheduling, time windows, targeting, or rollout percentages. " +
			"Defaults: scope=global, version=0.0.0.0, is_permanent=false, expiration=45 days.");

		// Connection string option
		var connectionStringOption = new Option<string?>("--connection-string")
		{
			Description = "Database connection string (required)",
			IsRequired = true
		};

		// JSON option for advanced flags
		var jsonOption = new Option<string?>("--json")
		{
			Description = "Create flag from JSON with advanced features (scheduling, time windows, user/tenant targeting, rollout percentages, variations). " +
						  "When using --json, all other flag options (--key, --name, etc.) are ignored.",
			IsRequired = false
		};

		// Simple flag options
		var keyOption = new Option<string?>("--key")
		{
			Description = "Flag key (required if not using --json). Use kebab-case: 'my-feature-flag'",
			IsRequired = false
		};

		var nameOption = new Option<string?>("--name")
		{
			Description = "Human-readable flag name (optional, defaults to key value)",
			IsRequired = false
		};

		var descriptionOption = new Option<string?>("--description")
		{
			Description = "Flag description explaining its purpose (optional)",
			IsRequired = false
		};

		var tagsOption = new Option<string?>("--tags")
		{
			Description = "Tags as JSON object (optional). Example: {\"team\":\"platform\",\"type\":\"maintenance\"}",
			IsRequired = false
		};

		var statusOption = new Option<string?>("--status")
		{
			Description = "Initial flag status: 'on' or 'off' (optional, defaults to 'off'). Note: Creates simple On/Off flag only.",
			IsRequired = false
		};

		var usernameOption = new Option<string?>("--username")
		{
			Description = "Username for audit trail (required). Used to track who created the flag.",
			IsRequired = true
		};

		createCommand.AddOption(connectionStringOption);
		createCommand.AddOption(jsonOption);
		createCommand.AddOption(keyOption);
		createCommand.AddOption(nameOption);
		createCommand.AddOption(descriptionOption);
		createCommand.AddOption(tagsOption);
		createCommand.AddOption(statusOption);
		createCommand.AddOption(usernameOption);

		createCommand.SetHandler(async (context) =>
		{
			var connectionString = context.ParseResult.GetValueForOption(connectionStringOption);
			var jsonString = context.ParseResult.GetValueForOption(jsonOption);
			var key = context.ParseResult.GetValueForOption(keyOption);
			var name = context.ParseResult.GetValueForOption(nameOption);
			var description = context.ParseResult.GetValueForOption(descriptionOption);
			var tags = context.ParseResult.GetValueForOption(tagsOption);
			var status = context.ParseResult.GetValueForOption(statusOption);
			var username = context.ParseResult.GetValueForOption(usernameOption);

			await ExecuteAsync(connectionString, jsonString, key, name,
				description, tags, status, username);
		});

		return createCommand;
	}
}