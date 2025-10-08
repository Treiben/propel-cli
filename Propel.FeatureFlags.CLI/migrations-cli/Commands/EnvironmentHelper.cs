namespace Propel.FeatureFlags.Migrations.CLI;

public static class EnvironmentHelper
{
	/// <summary>
	/// Gets connection string from environment variable if not provided directly
	/// Environment variable names: DB_CONNECTION_STRING, DATABASE_URL, ConnectionStrings__Default
	/// </summary>
	public static string? GetConnectionStringFromEnvironment()
	{
		return Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ??
			   Environment.GetEnvironmentVariable("DATABASE_URL") ??
			   Environment.GetEnvironmentVariable("ConnectionStrings__Default");
	}

	/// <summary>
	/// Gets database host from environment variable
	/// Environment variable name: DB_HOST
	/// </summary>
	public static string? GetHostFromEnvironment()
	{
		return Environment.GetEnvironmentVariable("DB_HOST");
	}

	/// <summary>
	/// Gets database name from environment variable
	/// Environment variable name: DB_DATABASE, DB_NAME
	/// </summary>
	public static string? GetDatabaseFromEnvironment()
	{
		return Environment.GetEnvironmentVariable("DB_DATABASE") ??
			   Environment.GetEnvironmentVariable("DB_NAME");
	}

	/// <summary>
	/// Gets database username from environment variable
	/// Environment variable name: DB_USERNAME, DB_USER
	/// </summary>
	public static string? GetUsernameFromEnvironment()
	{
		return Environment.GetEnvironmentVariable("DB_USERNAME") ??
			   Environment.GetEnvironmentVariable("DB_USER");
	}

	/// <summary>
	/// Gets database password from environment variable
	/// Environment variable name: DB_PASSWORD, DB_PASS
	/// </summary>
	public static string? GetPasswordFromEnvironment()
	{
		return Environment.GetEnvironmentVariable("DB_PASSWORD") ??
			   Environment.GetEnvironmentVariable("DB_PASS");
	}

	/// <summary>
	/// Gets database port from environment variable
	/// Environment variable name: DB_PORT
	/// </summary>
	public static int? GetPortFromEnvironment()
	{
		var portStr = Environment.GetEnvironmentVariable("DB_PORT");
		if (int.TryParse(portStr, out var port))
		{
			return port;
		}
		return null;
	}

	/// <summary>
	/// Gets database provider from environment variable if not provided directly
	/// Environment variable name: DB_PROVIDER
	/// </summary>
	public static string? GetProviderFromEnvironment()
	{
		return Environment.GetEnvironmentVariable("DB_PROVIDER");
	}

	/// <summary>
	/// Gets PostgreSQL schema from environment variable
	/// Environment variable name: DB_SCHEMA, POSTGRES_SCHEMA
	/// </summary>
	public static string? GetSchemaFromEnvironment()
	{
		return Environment.GetEnvironmentVariable("DB_SCHEMA") ??
			   Environment.GetEnvironmentVariable("POSTGRES_SCHEMA");
	}

	/// <summary>
	/// Gets migrations path from environment variable if not provided directly
	/// Environment variable name: MIGRATIONS_PATH
	/// </summary>
	public static string GetMigrationsPath(string? migrationsPath)
	{
		if (!string.IsNullOrEmpty(migrationsPath))
		{
			return migrationsPath;
		}

		return Environment.GetEnvironmentVariable("MIGRATIONS_PATH") ?? "./Migrations";
	}

	/// <summary>
	/// Gets seeds path from environment variable if not provided directly
	/// Environment variable name: SEEDS_PATH
	/// </summary>
	public static string GetSeedsPath(string? seedsPath)
	{
		if (!string.IsNullOrEmpty(seedsPath))
		{
			return seedsPath;
		}

		return Environment.GetEnvironmentVariable("SEEDS_PATH") ?? "./Seeds";
	}

	/// <summary>
	/// Checks if running in CI/CD environment
	/// </summary>
	public static bool IsRunningInCiCd()
	{
		var ciIndicators = new[]
		{
			"CI", "CONTINUOUS_INTEGRATION", "BUILD_ID", "BUILD_NUMBER",
			"GITHUB_ACTIONS", "GITLAB_CI", "AZURE_PIPELINE", "JENKINS_URL",
			"TEAMCITY_VERSION", "TRAVIS", "CIRCLECI"
		};

		return ciIndicators.Any(indicator =>
			!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(indicator)));
	}

	/// <summary>
	/// Gets all environment variables that start with DB_ for debugging
	/// </summary>
	public static Dictionary<string, string> GetDatabaseEnvironmentVariables()
	{
		return Environment.GetEnvironmentVariables()
			.Cast<System.Collections.DictionaryEntry>()
			.Where(entry => entry.Key.ToString()?.StartsWith("DB_", StringComparison.OrdinalIgnoreCase) == true)
			.ToDictionary(
				entry => entry.Key.ToString()!,
				entry => entry.Value?.ToString() ?? string.Empty
			);
	}
}