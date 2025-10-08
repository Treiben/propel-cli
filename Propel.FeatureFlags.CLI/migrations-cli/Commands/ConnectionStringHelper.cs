namespace Propel.FeatureFlags.Migrations.CLI;

/// <summary>
/// Helper class for connection string operations
/// </summary>
public static class ConnectionStringHelper
{
	/// <summary>
	/// Detects the database provider from a connection string
	/// </summary>
	/// <param name="connectionString">The connection string to analyze</param>
	/// <returns>The detected provider name (sqlserver or postgresql)</returns>
	/// <exception cref="ArgumentException">If provider cannot be detected</exception>
	public static string DetectProvider(string connectionString)
	{
		if (string.IsNullOrWhiteSpace(connectionString))
		{
			throw new ArgumentException("Connection string cannot be empty", nameof(connectionString));
		}

		var lowerConnectionString = connectionString.ToLowerInvariant();

		// PostgreSQL patterns
		if (lowerConnectionString.StartsWith("postgres://") ||
			lowerConnectionString.StartsWith("postgresql://") ||
			lowerConnectionString.Contains("host=") ||
			(lowerConnectionString.Contains("server=") && lowerConnectionString.Contains("port=5432")))
		{
			return "postgresql";
		}

		// SQL Server patterns
		if (lowerConnectionString.Contains("data source=") ||
			lowerConnectionString.Contains("server=") ||
			lowerConnectionString.Contains("initial catalog=") ||
			lowerConnectionString.Contains("database=") ||
			lowerConnectionString.Contains("integrated security=") ||
			lowerConnectionString.Contains("trusted_connection="))
		{
			return "sqlserver";
		}

		throw new ArgumentException(
			"Could not detect database provider from connection string. " +
			"Please specify the provider explicitly using --provider (sqlserver or postgresql)",
			nameof(connectionString));
	}

	/// <summary>
	/// Detects the provider from port number if no connection string is available
	/// </summary>
	public static string? DetectProviderFromPort(int? port)
	{
		return port switch
		{
			5432 => "postgresql",
			1433 => "sqlserver",
			_ => null
		};
	}
}
