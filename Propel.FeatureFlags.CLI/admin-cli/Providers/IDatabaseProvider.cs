using Propel.FeatureFlags.Admin.Models;

namespace Propel.FeatureFlags.Admin.Providers;

public interface IDatabaseProvider
{
	Task<bool> DatabaseExistsAsync(string connectionString);

	Task CreateFlagAsync(string connectionString, FeatureFlag flag, FeatureFlagMetadata metadata, FeatureFlagAudit audit);

	Task<FeatureFlag?> GetFlagByKeyAsync(string connectionString, string flagKey);

	Task DeleteFlagAsync(string connectionString, string flagKey, FeatureFlagAudit audit);

	Task UpdateFlagEvaluationModeAsync(string connectionString, string flagKey, int[] evaluationModes, FeatureFlagAudit audit);

	Task<List<FeatureFlag>> SearchFlagsAsync(string connectionString, string? flagKey, string? name, string? description);

	Task<List<FeatureFlag>> ListAllFlagsAsync(string connectionString);
}