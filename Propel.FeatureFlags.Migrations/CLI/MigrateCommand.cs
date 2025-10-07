using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Propel.FeatureFlags.Migrations.Services;
using System.CommandLine;

namespace Propel.FeatureFlags.Migrations.CLI;

public class MigrateCommand(IServiceProvider serviceProvider)
{
	private readonly ILogger<MigrateCommand> _logger = serviceProvider.GetRequiredService<ILogger<MigrateCommand>>();

	public async Task ExecuteAsync(
		string? connectionString,
		string? provider,
		string? migrationsPath,
		string? targetVersion,
		string? host,
		string? database,
		string? username,
		string? password,
		int? port,
		AuthenticationMode authMode)
	{
		try
		{
			_logger.LogInformation("Starting database migration...");

			// Build connection string from parameters
			var finalConnectionString = ConnectionStringBuilder.BuildConnectionString(
				connectionString,
				host ?? EnvironmentHelper.GetHostFromEnvironment(),
				database ?? EnvironmentHelper.GetDatabaseFromEnvironment(),
				username ?? EnvironmentHelper.GetUsernameFromEnvironment(),
				password ?? EnvironmentHelper.GetPasswordFromEnvironment(),
				port ?? EnvironmentHelper.GetPortFromEnvironment(),
				provider ?? EnvironmentHelper.GetProviderFromEnvironment(),
				authMode);

			// Auto-detect provider if not specified
			var finalProvider = provider ??
								EnvironmentHelper.GetProviderFromEnvironment() ??
								ConnectionStringHelper.DetectProvider(finalConnectionString);

			_logger.LogInformation("Provider: {Provider}", finalProvider);

			if (!string.IsNullOrEmpty(targetVersion))
			{
				_logger.LogInformation("Target Version: {TargetVersion}", targetVersion);
			}

			var migrationService = serviceProvider.GetRequiredService<IMigrationService>();
			await migrationService.MigrateAsync(finalConnectionString, finalProvider, migrationsPath, targetVersion);

			_logger.LogInformation("Migration completed successfully!");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Migration failed: {Message}", ex.Message);
			Environment.Exit(1);
		}
	}

	public Command Build()
	{
		var migrateCommand = new Command("migrate", "Run database migrations");

		// Connection string options
		var connectionStringOption = new Option<string?>("--connection-string")
		{
			Description = "Full database connection string (optional if using individual parameters)",
			IsRequired = false
		};

		var hostOption = new Option<string?>("--host")
		{
			Description = "Database host (e.g., localhost, myserver.database.windows.net)",
			IsRequired = false
		};

		var databaseOption = new Option<string?>("--database")
		{
			Description = "Database name",
			IsRequired = false
		};

		var usernameOption = new Option<string?>("--username")
		{
			Description = "Database username",
			IsRequired = false
		};

		var passwordOption = new Option<string?>("--password")
		{
			Description = "Database password",
			IsRequired = false
		};

		var portOption = new Option<int?>("--port")
		{
			Description = "Database port (default: 1433 for SQL Server, 5432 for PostgreSQL)",
			IsRequired = false
		};

		var providerOption = new Option<string?>("--provider")
		{
			Description = "Database provider: sqlserver or postgresql (auto-detected if not specified)",
			IsRequired = false
		};

		var authModeOption = new Option<AuthenticationMode>("--auth-mode")
		{
			Description = "Authentication mode",
			IsRequired = false
		};
		authModeOption.SetDefaultValue(AuthenticationMode.UserPassword);

		var migrationsPathOption = new Option<string?>("--migrations-path")
		{
			Description = "Path to migrations directory (optional, uses embedded resources by default)",
			IsRequired = false
		};

		var targetVersionOption = new Option<string?>("--target-version")
		{
			Description = "Target version to migrate to (optional)",
			IsRequired = false
		};

		migrateCommand.AddOption(connectionStringOption);
		migrateCommand.AddOption(hostOption);
		migrateCommand.AddOption(databaseOption);
		migrateCommand.AddOption(usernameOption);
		migrateCommand.AddOption(passwordOption);
		migrateCommand.AddOption(portOption);
		migrateCommand.AddOption(providerOption);
		migrateCommand.AddOption(authModeOption);
		migrateCommand.AddOption(migrationsPathOption);
		migrateCommand.AddOption(targetVersionOption);

		migrateCommand.SetHandler(async (context) =>
		{
			var connectionString = context.ParseResult.GetValueForOption(connectionStringOption);
			var provider = context.ParseResult.GetValueForOption(providerOption);
			var migrationsPath = context.ParseResult.GetValueForOption(migrationsPathOption);
			var targetVersion = context.ParseResult.GetValueForOption(targetVersionOption);
			var host = context.ParseResult.GetValueForOption(hostOption);
			var database = context.ParseResult.GetValueForOption(databaseOption);
			var username = context.ParseResult.GetValueForOption(usernameOption);
			var password = context.ParseResult.GetValueForOption(passwordOption);
			var port = context.ParseResult.GetValueForOption(portOption);
			var authMode = context.ParseResult.GetValueForOption(authModeOption);

			await ExecuteAsync(connectionString, provider, migrationsPath, targetVersion,
				host, database, username, password, port, authMode);
		});

		return migrateCommand;
	}
}