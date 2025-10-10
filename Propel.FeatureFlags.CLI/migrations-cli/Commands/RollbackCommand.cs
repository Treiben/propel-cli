using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Propel.FeatureFlags.Migrations.Services;
using System.CommandLine;

namespace Propel.FeatureFlags.Migrations.CLI;

public sealed class RollbackCommand
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<RollbackCommand> _logger;

	public RollbackCommand(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
		_logger = serviceProvider.GetRequiredService<ILogger<RollbackCommand>>();
	}

	public async Task ExecuteAsync(
		string? connectionString,
		string? provider,
		string? migrationsPath,
		string version,
		string? host,
		string? database,
		string? username,
		string? password,
		int? port,
		string? schema,
		AuthenticationMode authMode)
	{
		try
		{
			_logger.LogInformation("Starting database rollback...");

			// Build connection string from parameters
			var finalConnectionString = ConnectionStringBuilder.BuildConnectionString(
				connectionString,
				host ?? EnvironmentHelper.GetHostFromEnvironment(),
				database ?? EnvironmentHelper.GetDatabaseFromEnvironment(),
				username ?? EnvironmentHelper.GetUsernameFromEnvironment(),
				password ?? EnvironmentHelper.GetPasswordFromEnvironment(),
				port ?? EnvironmentHelper.GetPortFromEnvironment(),
				provider ?? EnvironmentHelper.GetProviderFromEnvironment(),
				schema ?? EnvironmentHelper.GetSchemaFromEnvironment(),
				authMode);

			// Auto-detect provider if not specified
			var finalProvider = provider ??
								EnvironmentHelper.GetProviderFromEnvironment() ??
								ConnectionStringHelper.DetectProvider(finalConnectionString);

			_logger.LogInformation("Provider: {Provider}", finalProvider);
			_logger.LogInformation("Target Version: {Version}", version);

			var migrationService = _serviceProvider.GetRequiredService<IMigrationService>();
			await migrationService.RollbackAsync(finalConnectionString, finalProvider, migrationsPath, version);

			_logger.LogInformation("Rollback completed successfully!");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Rollback failed: {Message}", ex.Message);
			Environment.Exit(1);
		}
	}

	public Command Build()
	{
		var rollbackCommand = new Command("rollback", "Rollback database migrations");

		// Connection string options
		var connectionStringOption = new Option<string?>("--connection-string")
		{
			Description = "Full database connection string (optional if using individual parameters)",
			IsRequired = false
		};

		var hostOption = new Option<string?>("--host")
		{
			Description = "Database host",
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
			Description = "Database port",
			IsRequired = false
		};

		var schemaOption = new Option<string?>("--schema")
		{
			Description = "PostgreSQL schema name (search_path)",
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

		var rollbackVersionOption = new Option<string>("--version")
		{
			Description = "Version to rollback to",
			IsRequired = true
		};

		rollbackCommand.AddOption(connectionStringOption);
		rollbackCommand.AddOption(hostOption);
		rollbackCommand.AddOption(databaseOption);
		rollbackCommand.AddOption(usernameOption);
		rollbackCommand.AddOption(passwordOption);
		rollbackCommand.AddOption(portOption);
		rollbackCommand.AddOption(schemaOption);
		rollbackCommand.AddOption(providerOption);
		rollbackCommand.AddOption(authModeOption);
		rollbackCommand.AddOption(migrationsPathOption);
		rollbackCommand.AddOption(rollbackVersionOption);

		rollbackCommand.SetHandler(async (context) =>
		{
			var connectionString = context.ParseResult.GetValueForOption(connectionStringOption);
			var provider = context.ParseResult.GetValueForOption(providerOption);
			var migrationsPath = context.ParseResult.GetValueForOption(migrationsPathOption);
			var version = context.ParseResult.GetValueForOption(rollbackVersionOption)!;
			var host = context.ParseResult.GetValueForOption(hostOption);
			var database = context.ParseResult.GetValueForOption(databaseOption);
			var username = context.ParseResult.GetValueForOption(usernameOption);
			var password = context.ParseResult.GetValueForOption(passwordOption);
			var port = context.ParseResult.GetValueForOption(portOption);
			var schema = context.ParseResult.GetValueForOption(schemaOption);
			var authMode = context.ParseResult.GetValueForOption(authModeOption);

			await ExecuteAsync(connectionString, provider, migrationsPath, version,
				host, database, username, password, port, schema, authMode);
		});

		return rollbackCommand;
	}
}