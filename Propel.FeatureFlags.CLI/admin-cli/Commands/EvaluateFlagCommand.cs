using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Propel.FeatureFlags.Admin.Services;
using Propel.FeatureFlags.Migrations.CLI;
using System.CommandLine;

namespace Propel.FeatureFlags.Admin.CLI;

public sealed class EvaluateFlagCommand
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<EvaluateFlagCommand> _logger;

	public EvaluateFlagCommand(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
		_logger = serviceProvider.GetRequiredService<ILogger<EvaluateFlagCommand>>();
	}

	public async Task ExecuteAsync(
						string? connectionString,
						string? key,
						string? attributes)
	{
		try
		{
			ArgumentNullException.ThrowIfNull(connectionString, nameof(connectionString));
			ArgumentNullException.ThrowIfNull(key, nameof(key));

			_logger.LogInformation("Checking flag state...");

			// Auto-detect provider if not specified
			var provider = ConnectionStringHelper.DetectProvider(connectionString);

			_logger.LogInformation("Provider: {Provider}", provider);

			var administrationService = _serviceProvider.GetRequiredService<IAdministrationService>();
			await administrationService.EvaluateAsync(connectionString, provider, key, attributes);

			Console.WriteLine("\nFeature flag current status:");
			Console.WriteLine("================");

			//TODO: implement output of flag status
			//if (!status.Any())
			//{
			//	Console.WriteLine("No migrations found.");
			//	return;
			//}

			//Console.WriteLine($"{"Version",-20} {"Status",-10} {"Applied At",-20} {"Description"}");
			//Console.WriteLine(new string('-', 80));

			//foreach (var migration in status.OrderBy(m => m.Version))
			//{
			//	var appliedAt = migration.AppliedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Not Applied";
			//	var status_text = migration.IsApplied ? "Applied" : "Pending";
			//	Console.WriteLine($"{migration.Version,-20} {status_text,-10} {appliedAt,-20} {migration.Description}");
			//}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Operation failed: {Message}", ex.Message);

			Console.WriteLine($"Error: {ex.Message}");
			Environment.Exit(1);
		}
	}

	public Command Build()
	{
		var evaluateCommand = new Command("evaluate", "Evaluate existing flag status (enabled or disabled)");

		// Connection string options
		var connectionStringOption = new Option<string?>("--connection-string")
		{
			Description = "Full database connection string (required)",
			IsRequired = true
		};

		var keyOption = new Option<string?>("--key")
		{
			Description = "Flag key",
			IsRequired = true
		};

		var attributesOption = new Option<string?>("--attributes")
		{
			Description = "Optional attributes to evaluate with flag, for example {\"user_id\": \"test-user\", \"tenant-id\": \"test-tenant\", \"attributes\": {\"user-role\": \"admin\"} }",
			IsRequired = false
		};

		evaluateCommand.AddOption(connectionStringOption);
		evaluateCommand.AddOption(keyOption);
		evaluateCommand.AddOption(attributesOption);


		evaluateCommand.SetHandler(async (context) =>
		{
			var connectionString = context.ParseResult.GetValueForOption(connectionStringOption);
			var key = context.ParseResult.GetValueForOption(keyOption);
			var attributes = context.ParseResult.GetValueForOption(attributesOption);

			await ExecuteAsync(connectionString, key, attributes);
		});

		return evaluateCommand;
	}
}
