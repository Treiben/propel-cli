using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Propel.FeatureFlags.Migrations.Services;
using System.CommandLine;

namespace Propel.FeatureFlags.Migrations.CLI;

public class SeedCommand(IServiceProvider serviceProvider)
{
	private readonly ILogger<SeedCommand> _logger = serviceProvider.GetRequiredService<ILogger<SeedCommand>>();

	public async Task ExecuteAsync(
		string? connectionString,
		string? provider,
		string seedsPath,
		string? host,
		string? database,
		string? username,
		string? password,
		int? port,
		AuthenticationMode authMode)
	{
		try
		{
			_logger.LogInformation("Starting database seeding...");

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
			_logger.LogInformation("Seeds Path: {SeedsPath}", seedsPath);

			var seedService = serviceProvider.GetRequiredService<ISeedService>();
			await seedService.SeedAsync(finalConnectionString, finalProvider, seedsPath);

			_logger.LogInformation("Seeding completed successfully!");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Seeding failed: {Message}", ex.Message);
			Environment.Exit(1);
		}
	}

	public Command Build()
	{
		var seedCommand = new Command("seed", "Run database seeds");

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

		var seedsPathOption = new Option<string>("--seeds-path")
		{
			Description = "Path to seeds directory",
			IsRequired = false
		};
		seedsPathOption.SetDefaultValue("./Seeds");

		seedCommand.AddOption(connectionStringOption);
		seedCommand.AddOption(hostOption);
		seedCommand.AddOption(databaseOption);
		seedCommand.AddOption(usernameOption);
		seedCommand.AddOption(passwordOption);
		seedCommand.AddOption(portOption);
		seedCommand.AddOption(providerOption);
		seedCommand.AddOption(authModeOption);
		seedCommand.AddOption(seedsPathOption);

		seedCommand.SetHandler(async (context) =>
		{
			var connectionString = context.ParseResult.GetValueForOption(connectionStringOption);
			var provider = context.ParseResult.GetValueForOption(providerOption);
			var seedsPath = context.ParseResult.GetValueForOption(seedsPathOption)!;
			var host = context.ParseResult.GetValueForOption(hostOption);
			var database = context.ParseResult.GetValueForOption(databaseOption);
			var username = context.ParseResult.GetValueForOption(usernameOption);
			var password = context.ParseResult.GetValueForOption(passwordOption);
			var port = context.ParseResult.GetValueForOption(portOption);
			var authMode = context.ParseResult.GetValueForOption(authModeOption);

			await ExecuteAsync(connectionString, provider, seedsPath,
				host, database, username, password, port, authMode);
		});

		return seedCommand;
	}
}