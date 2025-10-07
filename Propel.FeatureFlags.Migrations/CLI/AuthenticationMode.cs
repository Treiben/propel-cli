namespace Propel.FeatureFlags.Migrations.CLI;

/// <summary>
/// Authentication modes supported by the migration CLI
/// </summary>
public enum AuthenticationMode
{
	/// <summary>
	/// Standard username and password authentication
	/// </summary>
	UserPassword,

	/// <summary>
	/// Windows Authentication / Integrated Security (SQL Server)
	/// </summary>
	IntegratedSecurity,

	/// <summary>
	/// Azure Managed Identity authentication
	/// </summary>
	AzureManagedIdentity,

	/// <summary>
	/// Azure Active Directory Interactive authentication
	/// </summary>
	AzureActiveDirectory,

	/// <summary>
	/// AWS IAM authentication (RDS)
	/// </summary>
	AwsIam,

	/// <summary>
	/// SSL Certificate authentication (PostgreSQL)
	/// </summary>
	SslCertificate
}
