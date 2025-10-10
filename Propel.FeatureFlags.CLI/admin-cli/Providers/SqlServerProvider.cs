using Microsoft.Extensions.Logging;
using Propel.FeatureFlags.Admin.Models;

namespace Propel.FeatureFlags.Admin.Providers;

//complete this class
public sealed class SqlServerProvider(ILogger<SqlServerProvider> logger) : IDatabaseProvider
{
	public Task CreateFlagAsync(string connectionString, FeatureFlag flag, FeatureFlagMetadata metadata, FeatureFlagAudit audit)
	{
		throw new NotImplementedException();
	}

	public Task<bool> DatabaseExistsAsync(string connectionString)
	{
		throw new NotImplementedException();
	}

	public Task DeleteFlagAsync(string connectionString, string flagKey, FeatureFlagAudit audit)
	{
		throw new NotImplementedException();
	}

	public Task<FeatureFlag?> GetFlagByKeyAsync(string connectionString, string flagKey)
	{
		throw new NotImplementedException();
	}

	public Task<List<FeatureFlag>> ListAllFlagsAsync(string connectionString)
	{
		throw new NotImplementedException();
	}

	public Task<List<FeatureFlag>> SearchFlagsAsync(string connectionString, string? flagKey, string? name, string? description)
	{
		throw new NotImplementedException();
	}

	public Task UpdateFlagEvaluationModeAsync(string connectionString, string flagKey, int[] evaluationModes, FeatureFlagAudit audit)
	{
		throw new NotImplementedException();
	}
}