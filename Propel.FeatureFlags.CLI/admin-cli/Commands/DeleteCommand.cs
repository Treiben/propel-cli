using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Propel.FeatureFlags.Admin.Services;
using Propel.FeatureFlags.Migrations.CLI;
using System.CommandLine;

namespace Propel.FeatureFlags.Admin.CLI;

public sealed class DeleteCommand(IServiceProvider serviceProvider)
{
	private readonly ILogger<DeleteCommand> _logger = serviceProvider.GetRequiredService<ILogger<DeleteCommand>>();

	public async Task ExecuteAsync(
						string? connectionString,
						string? key,
						string? username)
	{
		try
		{
			ArgumentNullException.ThrowIfNull(connectionString, nameof(connectionString));
			ArgumentNullException.ThrowIfNull(key, nameof(key));
			ArgumentNullException.ThrowIfNull(username, nameof(username));

			_logger.LogInformation("Deleting flag...");

			// Auto-detect provider if not specified
			var provider = ConnectionStringHelper.DetectProvider(connectionString);

			_logger.LogInformation("Provider: {Provider}", provider);

			var administrationService = serviceProvider.GetRequiredService<IAdministrationService>();
			await administrationService.DeleteAsync(connectionString, provider, key, username);

			Console.WriteLine($"\nSuccess: Flag '{key}' deleted from database");
			Console.WriteLine("\nIMPORTANT:");
			Console.WriteLine("  - Application flags WILL BE RESTORED on next app startup");
			Console.WriteLine("  - To permanently delete: Remove flag definition from application code");
			Console.WriteLine("  - Database deletion only works for flags created via CLI");
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
		var deleteCommand = new Command("delete",
			"Delete a feature flag from the database. " +
			"WARNING: Use with caution. Global flags should rarely be deleted (high risk). " +
			"Application flags deleted from database will be RESTORED automatically when the application starts. " +
			"To permanently remove application flags, delete them from your application code. " +
			"This command is primarily for cleaning up CLI-created flags.");

		// Connection string option
		var connectionStringOption = new Option<string?>("--connection-string")
		{
			Description = "Database connection string (required)",
			IsRequired = true
		};

		var keyOption = new Option<string?>("--key")
		{
			Description = "Flag key to delete (required). Must match exactly.",
			IsRequired = true
		};

		var usernameOption = new Option<string?>("--username")
		{
			Description = "Username for audit trail (required). Used to track who deleted the flag.",
			IsRequired = true
		};

		deleteCommand.AddOption(connectionStringOption);
		deleteCommand.AddOption(keyOption);
		deleteCommand.AddOption(usernameOption);

		deleteCommand.SetHandler(async (context) =>
		{
			var connectionString = context.ParseResult.GetValueForOption(connectionStringOption);
			var key = context.ParseResult.GetValueForOption(keyOption);
			var username = context.ParseResult.GetValueForOption(usernameOption);

			await ExecuteAsync(connectionString, key, username);
		});

		return deleteCommand;
	}
}