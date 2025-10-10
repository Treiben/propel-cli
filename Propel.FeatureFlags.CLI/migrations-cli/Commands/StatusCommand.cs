using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Propel.FeatureFlags.Migrations.Services;
using System.CommandLine;

namespace Propel.FeatureFlags.Migrations.CLI;

public sealed class StatusCommand
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<StatusCommand> _logger;

	public StatusCommand(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
		_logger = serviceProvider.GetRequiredService<ILogger<StatusCommand>>();
	}

	public async Task ExecuteAsync(
		string? connectionString,
		string? provider,
		string? migrationsPath,
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
			_logger.LogInformation("Checking migration status...");

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

			var migrationService = _serviceProvider.GetRequiredService<IMigrationService>();
			var status = await migrationService.GetStatusAsync(finalConnectionString, finalProvider, migrationsPath);

			Console.WriteLine("\nMigration Status:");
			Console.WriteLine("================");

			if (!status.Any())
			{
				Console.WriteLine("No migrations found.");
				return;
			}

			Console.WriteLine($"{"Version",-20} {"Status",-10} {"Applied At",-20} {"Description"}");
			Console.WriteLine(new string('-', 80));

			foreach (var migration in status.OrderBy(m => m.Version))
			{
				var appliedAt = migration.AppliedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Not Applied";
				var status_text = migration.IsApplied ? "Applied" : "Pending";
				Console.WriteLine($"{migration.Version,-20} {status_text,-10} {appliedAt,-20} {migration.Description}");
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Status check failed: {Message}", ex.Message);
			Environment.Exit(1);
		}
	}

	public Command Build()
	{
		var statusCommand = new Command("status", "Show migration status");

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

		statusCommand.AddOption(connectionStringOption);
		statusCommand.AddOption(hostOption);
		statusCommand.AddOption(databaseOption);
		statusCommand.AddOption(usernameOption);
		statusCommand.AddOption(passwordOption);
		statusCommand.AddOption(portOption);
		statusCommand.AddOption(schemaOption);
		statusCommand.AddOption(providerOption);
		statusCommand.AddOption(authModeOption);
		statusCommand.AddOption(migrationsPathOption);

		statusCommand.SetHandler(async (context) =>
		{
			var connectionString = context.ParseResult.GetValueForOption(connectionStringOption);
			var provider = context.ParseResult.GetValueForOption(providerOption);
			var migrationsPath = context.ParseResult.GetValueForOption(migrationsPathOption);
			var host = context.ParseResult.GetValueForOption(hostOption);
			var database = context.ParseResult.GetValueForOption(databaseOption);
			var username = context.ParseResult.GetValueForOption(usernameOption);
			var password = context.ParseResult.GetValueForOption(passwordOption);
			var port = context.ParseResult.GetValueForOption(portOption);
			var schema = context.ParseResult.GetValueForOption(schemaOption);
			var authMode = context.ParseResult.GetValueForOption(authModeOption);

			await ExecuteAsync(connectionString, provider, migrationsPath,
				host, database, username, password, port, schema, authMode);
		});

		return statusCommand;
	}
}