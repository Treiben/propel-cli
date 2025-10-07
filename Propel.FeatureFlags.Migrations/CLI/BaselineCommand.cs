using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Propel.FeatureFlags.Migrations.Services;
using System.CommandLine;

namespace Propel.FeatureFlags.Migrations.CLI;

public class BaselineCommand
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<BaselineCommand> _logger;

	public BaselineCommand(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
		_logger = serviceProvider.GetRequiredService<ILogger<BaselineCommand>>();
	}

	public async Task ExecuteAsync(
		string? connectionString,
		string? provider,
		string version,
		string? host,
		string? database,
		string? username,
		string? password,
		int? port,
		AuthenticationMode authMode)
	{
		try
		{
			_logger.LogInformation("Creating baseline migration entry...");

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
			_logger.LogInformation("Baseline Version: {Version}", version);

			var migrationService = _serviceProvider.GetRequiredService<IMigrationService>();
			await migrationService.BaselineAsync(finalConnectionString, finalProvider, version);

			_logger.LogInformation("Baseline created successfully!");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Baseline creation failed: {Message}", ex.Message);
			Environment.Exit(1);
		}
	}

	public Command Build()
	{
		var baselineCommand = new Command("baseline", "Create baseline migration entry");

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

		var baselineVersionOption = new Option<string>("--version")
		{
			Description = "Baseline version",
			IsRequired = true
		};

		baselineCommand.AddOption(connectionStringOption);
		baselineCommand.AddOption(hostOption);
		baselineCommand.AddOption(databaseOption);
		baselineCommand.AddOption(usernameOption);
		baselineCommand.AddOption(passwordOption);
		baselineCommand.AddOption(portOption);
		baselineCommand.AddOption(providerOption);
		baselineCommand.AddOption(authModeOption);
		baselineCommand.AddOption(baselineVersionOption);

		baselineCommand.SetHandler(async (context) =>
		{
			var connectionString = context.ParseResult.GetValueForOption(connectionStringOption);
			var provider = context.ParseResult.GetValueForOption(providerOption);
			var version = context.ParseResult.GetValueForOption(baselineVersionOption)!;
			var host = context.ParseResult.GetValueForOption(hostOption);
			var database = context.ParseResult.GetValueForOption(databaseOption);
			var username = context.ParseResult.GetValueForOption(usernameOption);
			var password = context.ParseResult.GetValueForOption(passwordOption);
			var port = context.ParseResult.GetValueForOption(portOption);
			var authMode = context.ParseResult.GetValueForOption(authModeOption);

			await ExecuteAsync(connectionString, provider, version,
				host, database, username, password, port, authMode);
		});

		return baselineCommand;
	}
}