using Microsoft.Data.SqlClient;
using Npgsql;

namespace Propel.FeatureFlags.Migrations.CLI;

/// <summary>
/// Builds database connection strings from individual parameters
/// </summary>
public static class ConnectionStringBuilder
{
	/// <summary>
	/// Builds a connection string from individual parameters or returns provided connection string
	/// </summary>
	public static string BuildConnectionString(
		string? connectionString,
		string? host,
		string? database,
		string? username,
		string? password,
		int? port,
		string? provider,
		string? schema = null,
		AuthenticationMode authMode = AuthenticationMode.UserPassword,
		Dictionary<string, string>? additionalParams = null)
	{
		// Priority 1: Full connection string provided - use as-is
		if (!string.IsNullOrEmpty(connectionString))
		{
			return connectionString;
		}

		// Priority 2: Check environment variables
		connectionString = EnvironmentHelper.GetConnectionStringFromEnvironment();
		if (!string.IsNullOrEmpty(connectionString))
		{
			return connectionString;
		}

		// Priority 3: Build from parameters
		if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(database))
		{
			throw new ArgumentException(
				"Must provide either --connection-string or --host and --database parameters. " +
				"Alternatively, set DB_CONNECTION_STRING environment variable.");
		}

		// Detect provider if not specified
		var detectedProvider = provider ??
			ConnectionStringHelper.DetectProviderFromPort(port) ??
			throw new ArgumentException(
				"Cannot determine database provider. Please specify --provider (sqlserver or postgresql)");

		return detectedProvider.ToLowerInvariant() switch
		{
			"postgresql" => BuildPostgreSqlConnection(
				host, database, port ?? 5432, username, password, schema, authMode, additionalParams),
			"sqlserver" => BuildSqlServerConnection(
				host, database, port ?? 1433, username, password, authMode, additionalParams),
			_ => throw new ArgumentException($"Unsupported provider: {detectedProvider}")
		};
	}

	private static string BuildSqlServerConnection(
		string host,
		string database,
		int port,
		string? username,
		string? password,
		AuthenticationMode authMode,
		Dictionary<string, string>? additionalParams)
	{
		var builder = new SqlConnectionStringBuilder
		{
			DataSource = port == 1433 ? host : $"{host},{port}",
			InitialCatalog = database,
			TrustServerCertificate = true, // Default for simplicity
			ConnectTimeout = 30
		};

		switch (authMode)
		{
			case AuthenticationMode.UserPassword:
				if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
				{
					throw new ArgumentException(
						"Username and password are required for UserPassword authentication mode");
				}
				builder.UserID = username;
				builder.Password = password;
				break;

			case AuthenticationMode.IntegratedSecurity:
				builder.IntegratedSecurity = true;
				break;

			case AuthenticationMode.AzureManagedIdentity:
				builder.Authentication = SqlAuthenticationMethod.ActiveDirectoryManagedIdentity;
				break;

			case AuthenticationMode.AzureActiveDirectory:
				builder.Authentication = SqlAuthenticationMethod.ActiveDirectoryInteractive;
				if (!string.IsNullOrEmpty(username))
				{
					builder.UserID = username;
				}
				break;

			default:
				throw new ArgumentException(
					$"Authentication mode {authMode} is not supported for SQL Server");
		}

		// Apply any additional parameters
		if (additionalParams != null)
		{
			foreach (var param in additionalParams)
			{
				builder[param.Key] = param.Value;
			}
		}

		return builder.ConnectionString;
	}

	private static string BuildPostgreSqlConnection(
		string host,
		string database,
		int port,
		string? username,
		string? password,
		string? schema,
		AuthenticationMode authMode,
		Dictionary<string, string>? additionalParams)
	{
		var builder = new NpgsqlConnectionStringBuilder
		{
			Host = host,
			Database = database,
			Port = port,
			Timeout = 30
		};

		// Set schema (search_path) if provided
		if (!string.IsNullOrWhiteSpace(schema))
		{
			builder.SearchPath = schema;
		}

		switch (authMode)
		{
			case AuthenticationMode.UserPassword:
				if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
				{
					throw new ArgumentException(
						"Username and password are required for UserPassword authentication mode");
				}
				builder.Username = username;
				builder.Password = password;
				break;

			case AuthenticationMode.SslCertificate:
				builder.SslMode = SslMode.Require;
				if (additionalParams?.ContainsKey("ssl-cert") == true)
				{
					builder.SslCertificate = additionalParams["ssl-cert"];
				}
				if (additionalParams?.ContainsKey("ssl-key") == true)
				{
					builder.SslKey = additionalParams["ssl-key"];
				}
				if (!string.IsNullOrEmpty(username))
				{
					builder.Username = username;
				}
				break;

			case AuthenticationMode.AwsIam:
				if (string.IsNullOrEmpty(username))
				{
					throw new ArgumentException("Username is required for AWS IAM authentication");
				}
				builder.Username = username;
				// Note: AWS IAM requires generating a token. This is a simplified implementation.
				// In production, you would generate the IAM auth token here.
				if (!string.IsNullOrEmpty(password))
				{
					builder.Password = password;
				}
				break;

			default:
				throw new ArgumentException(
					$"Authentication mode {authMode} is not supported for PostgreSQL");
		}

		// Apply additional parameters
		if (additionalParams != null)
		{
			foreach (var param in additionalParams.Where(p =>
				!p.Key.StartsWith("ssl-") && p.Key != "aws-region"))
			{
				builder[param.Key] = param.Value;
			}
		}

		return builder.ConnectionString;
	}
}