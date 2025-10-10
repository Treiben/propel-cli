using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Propel.FeatureFlags.Admin.Models;
using System.Text.Json;

namespace Propel.FeatureFlags.Admin.Providers;

public sealed class SqlServerProvider(ILogger<SqlServerProvider> logger) : IDatabaseProvider
{
	public async Task<bool> DatabaseExistsAsync(string connectionString)
	{
		var builder = new SqlConnectionStringBuilder(connectionString);
		var databaseName = builder.InitialCatalog;
		builder.InitialCatalog = "master";

		await using var connection = new SqlConnection(builder.ToString());
		await connection.OpenAsync();

		await using var command = connection.CreateCommand();
		command.CommandText = "SELECT 1 FROM sys.databases WHERE name = @databaseName";
		command.Parameters.AddWithValue("@databaseName", databaseName);

		var result = await command.ExecuteScalarAsync();
		return result != null;
	}

	public async Task CreateFlagAsync(string connectionString, FeatureFlag flag, FeatureFlagMetadata metadata, FeatureFlagAudit audit)
	{
		await using var connection = new SqlConnection(connectionString);
		await connection.OpenAsync();

		await using var transaction = await connection.BeginTransactionAsync();

		try
		{
			// Insert into FeatureFlags
			await using (var command = connection.CreateCommand())
			{
				command.Transaction = (SqlTransaction)transaction;
				command.CommandText = @"
					INSERT INTO FeatureFlags (
						[Key], ApplicationName, ApplicationVersion, Scope,
						Name, Description, EvaluationModes,
						ScheduledEnableDate, ScheduledDisableDate,
						WindowStartTime, WindowEndTime, TimeZone, WindowDays,
						TargetingRules,
						EnabledUsers, DisabledUsers, UserPercentageEnabled,
						EnabledTenants, DisabledTenants, TenantPercentageEnabled,
						Variations, DefaultVariation
					) VALUES (
						@key, @application_name, @application_version, @scope,
						@name, @description, @evaluation_modes,
						@scheduled_enable_date, @scheduled_disable_date,
						@window_start_time, @window_end_time, @time_zone, @window_days,
						@targeting_rules,
						@enabled_users, @disabled_users, @user_percentage_enabled,
						@enabled_tenants, @disabled_tenants, @tenant_percentage_enabled,
						@variations, @default_variation
					)";

				command.Parameters.AddWithValue("@key", flag.Key);
				command.Parameters.AddWithValue("@application_name", flag.ApplicationName);
				command.Parameters.AddWithValue("@application_version", flag.ApplicationVersion);
				command.Parameters.AddWithValue("@scope", flag.Scope);
				command.Parameters.AddWithValue("@name", flag.Name);
				command.Parameters.AddWithValue("@description", flag.Description);
				command.Parameters.AddWithValue("@evaluation_modes", JsonSerializer.Serialize(flag.EvaluationModes));
				command.Parameters.AddWithValue("@scheduled_enable_date", (object?)flag.ScheduledEnableDate ?? DBNull.Value);
				command.Parameters.AddWithValue("@scheduled_disable_date", (object?)flag.ScheduledDisableDate ?? DBNull.Value);
				command.Parameters.AddWithValue("@window_start_time", (object?)flag.WindowStartTime ?? DBNull.Value);
				command.Parameters.AddWithValue("@window_end_time", (object?)flag.WindowEndTime ?? DBNull.Value);
				command.Parameters.AddWithValue("@time_zone", (object?)flag.TimeZone ?? DBNull.Value);
				command.Parameters.AddWithValue("@window_days", JsonSerializer.Serialize(flag.WindowDays));
				command.Parameters.AddWithValue("@targeting_rules", flag.TargetingRules ?? "[]");
				command.Parameters.AddWithValue("@enabled_users", JsonSerializer.Serialize(flag.EnabledUsers));
				command.Parameters.AddWithValue("@disabled_users", JsonSerializer.Serialize(flag.DisabledUsers));
				command.Parameters.AddWithValue("@user_percentage_enabled", flag.UserPercentageEnabled);
				command.Parameters.AddWithValue("@enabled_tenants", JsonSerializer.Serialize(flag.EnabledTenants));
				command.Parameters.AddWithValue("@disabled_tenants", JsonSerializer.Serialize(flag.DisabledTenants));
				command.Parameters.AddWithValue("@tenant_percentage_enabled", flag.TenantPercentageEnabled);
				command.Parameters.AddWithValue("@variations", JsonSerializer.Serialize(flag.Variations ?? []));
				command.Parameters.AddWithValue("@default_variation", flag.DefaultVariation);

				await command.ExecuteNonQueryAsync();
			}

			// Insert into FeatureFlagsMetadata
			await using (var command = connection.CreateCommand())
			{
				command.Transaction = (SqlTransaction)transaction;
				command.CommandText = @"
					INSERT INTO FeatureFlagsMetadata (
						FlagKey, ApplicationName, ApplicationVersion,
						IsPermanent, ExpirationDate, Tags
					) VALUES (
						@flag_key, @application_name, @application_version,
						@is_permanent, @expiration_date, @tags
					)";

				command.Parameters.AddWithValue("@flag_key", metadata.FlagKey);
				command.Parameters.AddWithValue("@application_name", metadata.ApplicationName);
				command.Parameters.AddWithValue("@application_version", metadata.ApplicationVersion);
				command.Parameters.AddWithValue("@is_permanent", metadata.IsPermanent);
				command.Parameters.AddWithValue("@expiration_date", metadata.ExpirationDate);
				command.Parameters.AddWithValue("@tags", JsonSerializer.Serialize(metadata.Tags ?? []));

				await command.ExecuteNonQueryAsync();
			}

			// Insert into FeatureFlagsAudit
			await using (var command = connection.CreateCommand())
			{
				command.Transaction = (SqlTransaction)transaction;
				command.CommandText = @"
					INSERT INTO FeatureFlagsAudit (
						FlagKey, ApplicationName, ApplicationVersion,
						Action, Actor, Timestamp, Notes
					) VALUES (
						@flag_key, @application_name, @application_version,
						@action, @actor, @timestamp, @notes
					)";

				command.Parameters.AddWithValue("@flag_key", audit.FlagKey);
				command.Parameters.AddWithValue("@application_name", audit.ApplicationName ?? "global");
				command.Parameters.AddWithValue("@application_version", audit.ApplicationVersion);
				command.Parameters.AddWithValue("@action", audit.Action);
				command.Parameters.AddWithValue("@actor", audit.Actor);
				command.Parameters.AddWithValue("@timestamp", audit.Timestamp);
				command.Parameters.AddWithValue("@notes", (object?)audit.Notes ?? DBNull.Value);

				await command.ExecuteNonQueryAsync();
			}

			await transaction.CommitAsync();
			logger.LogInformation("Flag {FlagKey} created successfully", flag.Key);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			logger.LogError(ex, "Failed to create flag {FlagKey}", flag.Key);
			throw;
		}
	}

	public async Task<List<FeatureFlag>> GetFlagsByKeyAndApplicationAsync(string connectionString, string flagKey, string applicationName)
	{
		await using var connection = new SqlConnection(connectionString);
		await connection.OpenAsync();

		await using var command = connection.CreateCommand();
		command.CommandText = @"
			SELECT 
				[Key], ApplicationName, ApplicationVersion, Scope,
				Name, Description, EvaluationModes,
				ScheduledEnableDate, ScheduledDisableDate,
				WindowStartTime, WindowEndTime, TimeZone, WindowDays,
				TargetingRules,
				EnabledUsers, DisabledUsers, UserPercentageEnabled,
				EnabledTenants, DisabledTenants, TenantPercentageEnabled,
				Variations, DefaultVariation
			FROM FeatureFlags
			WHERE [Key] = @key AND ApplicationName = @application_name";

		command.Parameters.AddWithValue("@key", flagKey);
		command.Parameters.AddWithValue("@application_name", applicationName);

		await using var reader = await command.ExecuteReaderAsync();

		var flags = new List<FeatureFlag>();
		while (await reader.ReadAsync())
		{
			flags.Add(MapFeatureFlag(reader));
		}

		return flags;
	}

	public async Task DeleteFlagAsync(string connectionString, string flagKey, FeatureFlagAudit audit)
	{
		await using var connection = new SqlConnection(connectionString);
		await connection.OpenAsync();

		await using var transaction = await connection.BeginTransactionAsync();

		try
		{
			// Delete from FeatureFlagsMetadata
			await using (var command = connection.CreateCommand())
			{
				command.Transaction = (SqlTransaction)transaction;
				command.CommandText = @"
					DELETE FROM FeatureFlagsMetadata 
					WHERE FlagKey = @flag_key 
					AND ApplicationName = @application_name";

				command.Parameters.AddWithValue("@flag_key", flagKey);
				command.Parameters.AddWithValue("@application_name", audit.ApplicationName);
				await command.ExecuteNonQueryAsync();
			}

			// Delete from FeatureFlags
			await using (var command = connection.CreateCommand())
			{
				command.Transaction = (SqlTransaction)transaction;
				command.CommandText = @"
					DELETE FROM FeatureFlags 
					WHERE [Key] = @key 
					AND ApplicationName = @application_name";

				command.Parameters.AddWithValue("@key", flagKey);
				command.Parameters.AddWithValue("@application_name", audit.ApplicationName);
				var rowsAffected = await command.ExecuteNonQueryAsync();

				if (rowsAffected == 0)
				{
					throw new InvalidOperationException($"Flag '{flagKey}' not found");
				}
			}

			// Insert audit record
			await using (var command = connection.CreateCommand())
			{
				command.Transaction = (SqlTransaction)transaction;
				command.CommandText = @"
					INSERT INTO FeatureFlagsAudit (
						FlagKey, ApplicationName, ApplicationVersion,
						Action, Actor, Timestamp, Notes
					) VALUES (
						@flag_key, @application_name, @application_version,
						@action, @actor, @timestamp, @notes
					)";

				command.Parameters.AddWithValue("@flag_key", audit.FlagKey);
				command.Parameters.AddWithValue("@application_name", audit.ApplicationName);
				command.Parameters.AddWithValue("@application_version", audit.ApplicationVersion);
				command.Parameters.AddWithValue("@action", audit.Action);
				command.Parameters.AddWithValue("@actor", audit.Actor);
				command.Parameters.AddWithValue("@timestamp", audit.Timestamp);
				command.Parameters.AddWithValue("@notes", (object?)audit.Notes ?? DBNull.Value);

				await command.ExecuteNonQueryAsync();
			}

			await transaction.CommitAsync();
			logger.LogInformation("Flag {FlagKey} deleted successfully", flagKey);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			logger.LogError(ex, "Failed to delete flag {FlagKey}", flagKey);
			throw;
		}
	}

	public async Task UpdateFlagEvaluationModeAsync(string connectionString, string flagKey, int[] evaluationModes, FeatureFlagAudit audit)
	{
		await using var connection = new SqlConnection(connectionString);
		await connection.OpenAsync();

		await using var transaction = await connection.BeginTransactionAsync();

		try
		{
			await using (var command = connection.CreateCommand())
			{
				command.Transaction = (SqlTransaction)transaction;
				command.CommandText = @"
					UPDATE FeatureFlags 
					SET EvaluationModes = @evaluation_modes
					WHERE [Key] = @key 
					AND ApplicationName = @application_name";

				command.Parameters.AddWithValue("@key", flagKey);
				command.Parameters.AddWithValue("@application_name", audit.ApplicationName);
				command.Parameters.AddWithValue("@evaluation_modes", JsonSerializer.Serialize(evaluationModes));

				var rowsAffected = await command.ExecuteNonQueryAsync();

				if (rowsAffected == 0)
				{
					throw new InvalidOperationException($"Flag '{flagKey}' not found");
				}
			}

			// Insert audit record
			await using (var command = connection.CreateCommand())
			{
				command.Transaction = (SqlTransaction)transaction;
				command.CommandText = @"
					INSERT INTO FeatureFlagsAudit (
						FlagKey, ApplicationName, ApplicationVersion,
						Action, Actor, Timestamp, Notes
					) VALUES (
						@flag_key, @application_name, @application_version,
						@action, @actor, @timestamp, @notes
					)";

				command.Parameters.AddWithValue("@flag_key", audit.FlagKey);
				command.Parameters.AddWithValue("@application_name", audit.ApplicationName);
				command.Parameters.AddWithValue("@application_version", audit.ApplicationVersion);
				command.Parameters.AddWithValue("@action", audit.Action);
				command.Parameters.AddWithValue("@actor", audit.Actor);
				command.Parameters.AddWithValue("@timestamp", audit.Timestamp);
				command.Parameters.AddWithValue("@notes", (object?)audit.Notes ?? DBNull.Value);

				await command.ExecuteNonQueryAsync();
			}

			await transaction.CommitAsync();
			logger.LogInformation("Flag {FlagKey} evaluation mode updated successfully", flagKey);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			logger.LogError(ex, "Failed to update flag {FlagKey} evaluation mode", flagKey);
			throw;
		}
	}

	public async Task<List<FeatureFlag>> SearchFlagsAsync(string connectionString, string? flagKey, string? name, string? description)
	{
		await using var connection = new SqlConnection(connectionString);
		await connection.OpenAsync();

		var conditions = new List<string>();
		var command = connection.CreateCommand();

		// Add search conditions (OR logic)
		var searchConditions = new List<string>();

		if (!string.IsNullOrWhiteSpace(flagKey))
		{
			searchConditions.Add("[Key] = @flagKey");
			command.Parameters.AddWithValue("@flagKey", flagKey);
		}

		if (!string.IsNullOrWhiteSpace(name))
		{
			searchConditions.Add("Name LIKE @name");
			command.Parameters.AddWithValue("@name", $"%{name}%");
		}

		if (!string.IsNullOrWhiteSpace(description))
		{
			searchConditions.Add("Description LIKE @description");
			command.Parameters.AddWithValue("@description", $"%{description}%");
		}

		if (searchConditions.Count != 0)
		{
			conditions.Add($"({string.Join(" OR ", searchConditions)})");
		}

		command.CommandText = $@"
			SELECT 
				[Key], ApplicationName, ApplicationVersion, Scope,
				Name, Description, EvaluationModes,
				ScheduledEnableDate, ScheduledDisableDate,
				WindowStartTime, WindowEndTime, TimeZone, WindowDays,
				TargetingRules,
				EnabledUsers, DisabledUsers, UserPercentageEnabled,
				EnabledTenants, DisabledTenants, TenantPercentageEnabled,
				Variations, DefaultVariation
			FROM FeatureFlags
			WHERE {string.Join(" AND ", conditions)}
			ORDER BY [Key]";

		var flags = new List<FeatureFlag>();

		await using var reader = await command.ExecuteReaderAsync();
		while (await reader.ReadAsync())
		{
			flags.Add(MapFeatureFlag(reader));
		}

		return flags;
	}

	public async Task<List<FeatureFlag>> ListAllFlagsAsync(string connectionString)
	{
		await using var connection = new SqlConnection(connectionString);
		await connection.OpenAsync();

		await using var command = connection.CreateCommand();
		command.CommandText = @"
			SELECT 
				[Key], ApplicationName, ApplicationVersion, Scope,
				Name, Description, EvaluationModes,
				ScheduledEnableDate, ScheduledDisableDate,
				WindowStartTime, WindowEndTime, TimeZone, WindowDays,
				TargetingRules,
				EnabledUsers, DisabledUsers, UserPercentageEnabled,
				EnabledTenants, DisabledTenants, TenantPercentageEnabled,
				Variations, DefaultVariation
			FROM FeatureFlags
			ORDER BY [Key]";

		var flags = new List<FeatureFlag>();

		await using var reader = await command.ExecuteReaderAsync();
		while (await reader.ReadAsync())
		{
			flags.Add(MapFeatureFlag(reader));
		}

		return flags;
	}

	private static FeatureFlag MapFeatureFlag(SqlDataReader reader)
	{
		return new FeatureFlag
		{
			Key = reader.GetString(reader.GetOrdinal("Key")),
			ApplicationName = reader.GetString(reader.GetOrdinal("ApplicationName")),
			ApplicationVersion = reader.GetString(reader.GetOrdinal("ApplicationVersion")),
			Scope = reader.GetInt32(reader.GetOrdinal("Scope")),
			Name = reader.GetString(reader.GetOrdinal("Name")),
			Description = reader.GetString(reader.GetOrdinal("Description")),
			EvaluationModes = JsonSerializer.Deserialize<int[]>(reader.GetString(reader.GetOrdinal("EvaluationModes"))) ?? [],
			ScheduledEnableDate = reader.IsDBNull(reader.GetOrdinal("ScheduledEnableDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ScheduledEnableDate")),
			ScheduledDisableDate = reader.IsDBNull(reader.GetOrdinal("ScheduledDisableDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ScheduledDisableDate")),
			WindowStartTime = reader.IsDBNull(reader.GetOrdinal("WindowStartTime")) ? null : reader.GetTimeSpan(reader.GetOrdinal("WindowStartTime")),
			WindowEndTime = reader.IsDBNull(reader.GetOrdinal("WindowEndTime")) ? null : reader.GetTimeSpan(reader.GetOrdinal("WindowEndTime")),
			TimeZone = reader.IsDBNull(reader.GetOrdinal("TimeZone")) ? null : reader.GetString(reader.GetOrdinal("TimeZone")),
			WindowDays = JsonSerializer.Deserialize<int[]>(reader.GetString(reader.GetOrdinal("WindowDays"))) ?? [],
			TargetingRules = reader.IsDBNull(reader.GetOrdinal("TargetingRules")) ? null : reader.GetString(reader.GetOrdinal("TargetingRules")),
			EnabledUsers = JsonSerializer.Deserialize<string[]>(reader.GetString(reader.GetOrdinal("EnabledUsers"))) ?? [],
			DisabledUsers = JsonSerializer.Deserialize<string[]>(reader.GetString(reader.GetOrdinal("DisabledUsers"))) ?? [],
			UserPercentageEnabled = reader.GetInt32(reader.GetOrdinal("UserPercentageEnabled")),
			EnabledTenants = JsonSerializer.Deserialize<string[]>(reader.GetString(reader.GetOrdinal("EnabledTenants"))) ?? [],
			DisabledTenants = JsonSerializer.Deserialize<string[]>(reader.GetString(reader.GetOrdinal("DisabledTenants"))) ?? [],
			TenantPercentageEnabled = reader.GetInt32(reader.GetOrdinal("TenantPercentageEnabled")),
			Variations = JsonSerializer.Deserialize<Dictionary<string, string>>(reader.GetString(reader.GetOrdinal("Variations"))),
			DefaultVariation = reader.GetString(reader.GetOrdinal("DefaultVariation"))
		};
	}
}