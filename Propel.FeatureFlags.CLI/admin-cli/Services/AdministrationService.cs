using Microsoft.Extensions.Logging;
using Propel.FeatureFlags.Admin.Models;
using System.Text.Json;

namespace Propel.FeatureFlags.Admin.Services;

public interface IAdministrationService
{
	Task CreateAsync(string connectionString, string provider, string fromJson, string username);
	Task DeleteAsync(string connectionString, string provider, string flagKey, string username);
	Task ToggleModeAsync(string connectionString, string provider, string flagKey, bool enable, string username);
	Task<List<Models.FeatureFlag>> SearchAsync(string connectionString, string provider, string? flagKey, string? name, string? description);
	Task<List<Models.FeatureFlag>> ListAllAsync(string connectionString, string provider);
}

public sealed class AdministrationService(IDatabaseProviderFactory providerFactory, ILogger<AdministrationService> logger) : IAdministrationService
{
	private static readonly JsonSerializerOptions AdministrationServiceSerializationOptions = new()
	{
		PropertyNameCaseInsensitive = true
	};
	public async Task CreateAsync(string connectionString, string provider, string fromJson, string username)
	{
		var dbProvider = providerFactory.CreateProvider(provider);

		if (!await dbProvider.DatabaseExistsAsync(connectionString))
		{
			throw new InvalidOperationException("Database does not exist. Please run migrations first.");
		}

		// Deserialize the JSON into a FeatureFlag object
		var flag = JsonSerializer.Deserialize<FeatureFlag>(fromJson, AdministrationServiceSerializationOptions) ?? throw new InvalidOperationException("Invalid JSON format for feature flag");

		// Ensure global scope
		flag.ApplicationName = "global";
		flag.ApplicationVersion = "0.0.0.0";

		// Check if flag already exists
		var existingFlag = await dbProvider.GetFlagByKeyAsync(connectionString, flag.Key);
		if (existingFlag != null)
		{
			throw new InvalidOperationException($"Flag with key '{flag.Key}' already exists");
		}

		// Create metadata with defaults
		var metadata = new FeatureFlagMetadata
		{
			FlagKey = flag.Key,
			ApplicationName = "global",
			ApplicationVersion = "0.0.0.0",
			IsPermanent = false,
			ExpirationDate = DateTime.UtcNow.AddDays(45),
			Tags = new Dictionary<string, string>
			{
				{ "created_by", username },
				{ "created_via", "cli" }
			}
		};

		// Create audit record
		var audit = new FeatureFlagAudit
		{
			FlagKey = flag.Key,
			ApplicationName = "global",
			ApplicationVersion = "0.0.0.0",
			Action = "flag-created",
			Actor = username,
			Timestamp = DateTime.UtcNow,
			Notes = $"Flag created via CLI by {username}"
		};

		await dbProvider.CreateFlagAsync(connectionString, flag, metadata, audit);

		logger.LogInformation("Flag {FlagKey} created successfully", flag.Key);
		Console.WriteLine($"Success: Flag '{flag.Key}' created");
	}

	public async Task DeleteAsync(string connectionString, string provider, string flagKey, string username)
	{
		var dbProvider = providerFactory.CreateProvider(provider);

		if (!await dbProvider.DatabaseExistsAsync(connectionString))
		{
			throw new InvalidOperationException("Database does not exist. Please run migrations first.");
		}

		// Check if flag exists
		_ = await dbProvider.GetFlagByKeyAsync(connectionString, flagKey) ?? throw new InvalidOperationException($"Flag with key '{flagKey}' not found");

		// Create audit record
		var audit = new FeatureFlagAudit
		{
			FlagKey = flagKey,
			ApplicationName = "global",
			ApplicationVersion = "0.0.0.0",
			Action = "flag-deleted",
			Actor = username,
			Timestamp = DateTime.UtcNow,
			Notes = $"Flag deleted via CLI by {username}"
		};

		await dbProvider.DeleteFlagAsync(connectionString, flagKey, audit);

		logger.LogInformation("Flag {FlagKey} deleted successfully", flagKey);
		Console.WriteLine($"Success: Flag '{flagKey}' deleted");
	}

	public async Task ToggleModeAsync(string connectionString, string provider, string flagKey, bool enable, string username)
	{
		var dbProvider = providerFactory.CreateProvider(provider);

		if (!await dbProvider.DatabaseExistsAsync(connectionString))
		{
			throw new InvalidOperationException("Database does not exist. Please run migrations first.");
		}

		// Check if flag exists
		_ = await dbProvider.GetFlagByKeyAsync(connectionString, flagKey) ?? throw new InvalidOperationException($"Flag with key '{flagKey}' not found");

		// Determine new evaluation mode
		// 0 = Off, 1 = On
		int[] newEvaluationModes = enable ? [1] : [0];

		// Create audit record
		var audit = new FeatureFlagAudit
		{
			FlagKey = flagKey,
			ApplicationName = "global",
			ApplicationVersion = "0.0.0.0",
			Action = enable ? "flag-enabled" : "flag-disabled",
			Actor = username,
			Timestamp = DateTime.UtcNow,
			Notes = $"Flag {(enable ? "enabled" : "disabled")} via CLI by {username}"
		};

		await dbProvider.UpdateFlagEvaluationModeAsync(connectionString, flagKey, newEvaluationModes, audit);

		logger.LogInformation("Flag {FlagKey} {Action}", flagKey, enable ? "enabled" : "disabled");
		Console.WriteLine($"Success: Flag '{flagKey}' {(enable ? "enabled" : "disabled")}");
	}

	public async Task<List<Models.FeatureFlag>> SearchAsync(string connectionString, string provider, string? flagKey, string? name, string? description)
	{
		var dbProvider = providerFactory.CreateProvider(provider);

		if (!await dbProvider.DatabaseExistsAsync(connectionString))
		{
			throw new InvalidOperationException("Database does not exist. Please run migrations first.");
		}

		var flags = await dbProvider.SearchFlagsAsync(connectionString, flagKey, name, description);

		logger.LogInformation("Search completed, found {Count} flags", flags.Count);

		return flags;
	}

	public async Task<List<Models.FeatureFlag>> ListAllAsync(string connectionString, string provider)
	{
		var dbProvider = providerFactory.CreateProvider(provider);

		if (!await dbProvider.DatabaseExistsAsync(connectionString))
		{
			throw new InvalidOperationException("Database does not exist. Please run migrations first.");
		}

		var flags = await dbProvider.ListAllFlagsAsync(connectionString);

		logger.LogInformation("Listed {Count} flags", flags.Count);

		return flags;
	}
}