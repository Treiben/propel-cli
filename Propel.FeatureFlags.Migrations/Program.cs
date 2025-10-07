using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Propel.FeatureFlags.Migrations.CLI;
using Propel.FeatureFlags.Migrations.Providers;
using Propel.FeatureFlags.Migrations.Services;
using System.CommandLine;

namespace Propel.FeatureFlags.Migrations;

class Program
{
	static async Task<int> Main(string[] args)
	{
		// Setup dependency injection
		var services = new ServiceCollection();
		ConfigureServices(services);
		var serviceProvider = services.BuildServiceProvider();

		// Create root command
		var rootCommand = new RootCommand("Feature Flags Database Migration CLI")
		{
			Description = "Database migration tool for Feature Flags system. Supports SQL Server and PostgreSQL."
		};

		// Add version option
		var versionOption = new Option<bool>(
			aliases: ["--cli-version"],
			description: "Show version information");

		rootCommand.AddGlobalOption(versionOption);

		// Build and add commands
		var migrateCommand = new MigrateCommand(serviceProvider);
		rootCommand.AddCommand(migrateCommand.Build());

		var rollbackCommand = new RollbackCommand(serviceProvider);
		rootCommand.AddCommand(rollbackCommand.Build());

		var statusCommand = new StatusCommand(serviceProvider);
		rootCommand.AddCommand(statusCommand.Build());

		var baselineCommand = new BaselineCommand(serviceProvider);
		rootCommand.AddCommand(baselineCommand.Build());

		var seedCommand = new SeedCommand(serviceProvider);
		rootCommand.AddCommand(seedCommand.Build());

		// Handle version flag
		rootCommand.SetHandler((bool showVersion) =>
		{
			if (showVersion)
			{
				var version = typeof(Program).Assembly.GetName().Version;
				Console.WriteLine($"Feature Flags Migration CLI v{version}");
				Console.WriteLine("https://github.com/yourorg/propel-featureflags-migrations");
			}
		}, versionOption);

		// Invoke command line parser
		return await rootCommand.InvokeAsync(args);
	}

	private static void ConfigureServices(IServiceCollection services)
	{
		// Configure logging
		services.AddLogging(configure =>
		{
			configure.AddConsole();
			configure.SetMinimumLevel(LogLevel.Information);
		});

		// Register services
		services.AddSingleton<IDatabaseProviderFactory, DatabaseProviderFactory>();
		services.AddSingleton<IMigrationService, MigrationService>();
		services.AddSingleton<ISeedService, SeedService>();

		// Register command classes (for dependency injection)
		services.AddTransient<MigrateCommand>();
		services.AddTransient<RollbackCommand>();
		services.AddTransient<StatusCommand>();
		services.AddTransient<BaselineCommand>();
		services.AddTransient<SeedCommand>();
	}
}