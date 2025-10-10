using Microsoft.Extensions.Logging;
using Npgsql;
using Propel.FeatureFlags.Admin.Models;
using System.Text.Json;

namespace Propel.FeatureFlags.Admin.Providers;

public sealed class PostgreSqlProvider(ILogger<PostgreSqlProvider> logger) : IDatabaseProvider
{
	public async Task<bool> DatabaseExistsAsync(string connectionString)
	{
		var builder = new NpgsqlConnectionStringBuilder(connectionString);
		var databaseName = builder.Database;
		builder.Database = "postgres";

		await using var connection = new NpgsqlConnection(builder.ToString());
		await connection.OpenAsync();

		await using var command = connection.CreateCommand();
		command.CommandText = "SELECT 1 FROM pg_database WHERE datname = @databaseName";
		command.Parameters.AddWithValue("@databaseName", databaseName!);

		var result = await command.ExecuteScalarAsync();
		return result != null;
	}

	public async Task CreateFlagAsync(string connectionString, FeatureFlag flag, FeatureFlagMetadata metadata, FeatureFlagAudit audit)
	{
		await using var connection = new NpgsqlConnection(connectionString);
		await connection.OpenAsync();

		await using var transaction = await connection.BeginTransactionAsync();

		try
		{
			// Insert into feature_flags
			await using (var command = connection.CreateCommand())
			{
				command.Transaction = transaction;
				command.CommandText = @"
					INSERT INTO feature_flags (
						key, application_name, application_version, scope,
						name, description, evaluation_modes,
						scheduled_enable_date, scheduled_disable_date,
						window_start_time, window_end_time, time_zone, window_days,
						targeting_rules,
						enabled_users, disabled_users, user_percentage_enabled,
						enabled_tenants, disabled_tenants, tenant_percentage_enabled,
						variations, default_variation
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
				command.Parameters.AddWithValue("@evaluation_modes", NpgsqlTypes.NpgsqlDbType.Jsonb, JsonSerializer.Serialize(flag.EvaluationModes));
				command.Parameters.AddWithValue("@scheduled_enable_date", (object?)flag.ScheduledEnableDate ?? DBNull.Value);
				command.Parameters.AddWithValue("@scheduled_disable_date", (object?)flag.ScheduledDisableDate ?? DBNull.Value);
				command.Parameters.AddWithValue("@window_start_time", (object?)flag.WindowStartTime ?? DBNull.Value);
				command.Parameters.AddWithValue("@window_end_time", (object?)flag.WindowEndTime ?? DBNull.Value);
				command.Parameters.AddWithValue("@time_zone", (object?)flag.TimeZone ?? DBNull.Value);
				command.Parameters.AddWithValue("@window_days", NpgsqlTypes.NpgsqlDbType.Jsonb, JsonSerializer.Serialize(flag.WindowDays));
				command.Parameters.AddWithValue("@targeting_rules", NpgsqlTypes.NpgsqlDbType.Jsonb, flag.TargetingRules ?? "[]");
				command.Parameters.AddWithValue("@enabled_users", NpgsqlTypes.NpgsqlDbType.Jsonb, JsonSerializer.Serialize(flag.EnabledUsers));
				command.Parameters.AddWithValue("@disabled_users", NpgsqlTypes.NpgsqlDbType.Jsonb, JsonSerializer.Serialize(flag.DisabledUsers));
				command.Parameters.AddWithValue("@user_percentage_enabled", flag.UserPercentageEnabled);
				command.Parameters.AddWithValue("@enabled_tenants", NpgsqlTypes.NpgsqlDbType.Jsonb, JsonSerializer.Serialize(flag.EnabledTenants));
				command.Parameters.AddWithValue("@disabled_tenants", NpgsqlTypes.NpgsqlDbType.Jsonb, JsonSerializer.Serialize(flag.DisabledTenants));
				command.Parameters.AddWithValue("@tenant_percentage_enabled", flag.TenantPercentageEnabled);
				command.Parameters.AddWithValue("@variations", NpgsqlTypes.NpgsqlDbType.Jsonb, JsonSerializer.Serialize(flag.Variations ?? []));
				command.Parameters.AddWithValue("@default_variation", flag.DefaultVariation);

				await command.ExecuteNonQueryAsync();
			}

			// Insert into feature_flags_metadata
			await using (var command = connection.CreateCommand())
			{
				command.Transaction = transaction;
				command.CommandText = @"
					INSERT INTO feature_flags_metadata (
						flag_key, application_name, application_version,
						is_permanent, expiration_date, tags
					) VALUES (
						@flag_key, @application_name, @application_version,
						@is_permanent, @expiration_date, @tags
					)";

				command.Parameters.AddWithValue("@flag_key", metadata.FlagKey);
				command.Parameters.AddWithValue("@application_name", metadata.ApplicationName);
				command.Parameters.AddWithValue("@application_version", metadata.ApplicationVersion);
				command.Parameters.AddWithValue("@is_permanent", metadata.IsPermanent);
				command.Parameters.AddWithValue("@expiration_date", metadata.ExpirationDate);
				command.Parameters.AddWithValue("@tags", NpgsqlTypes.NpgsqlDbType.Jsonb, JsonSerializer.Serialize(metadata.Tags ?? []));

				await command.ExecuteNonQueryAsync();
			}

			// Insert into feature_flags_audit
			await using (var command = connection.CreateCommand())
			{
				command.Transaction = transaction;
				command.CommandText = @"
					INSERT INTO feature_flags_audit (
						flag_key, application_name, application_version,
						action, actor, timestamp, notes
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
		await using var connection = new NpgsqlConnection(connectionString);
		await connection.OpenAsync();

		await using var command = connection.CreateCommand();
		command.CommandText = @"
			SELECT 
				key, application_name, application_version, scope,
				name, description, evaluation_modes,
				scheduled_enable_date, scheduled_disable_date,
				window_start_time, window_end_time, time_zone, window_days,
				targeting_rules,
				enabled_users, disabled_users, user_percentage_enabled,
				enabled_tenants, disabled_tenants, tenant_percentage_enabled,
				variations, default_variation
			FROM feature_flags
			WHERE key = @key and application_name = @application_name";

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
		await using var connection = new NpgsqlConnection(connectionString);
		await connection.OpenAsync();

		await using var transaction = await connection.BeginTransactionAsync();

		try
		{
			// Delete from feature_flags_metadata
			await using (var command = connection.CreateCommand())
			{
				command.Transaction = transaction;
				command.CommandText = @"
					DELETE FROM feature_flags_metadata 
					WHERE flag_key = @flag_key 
					AND application_name = @application_name";

				command.Parameters.AddWithValue("@flag_key", flagKey);
				command.Parameters.AddWithValue("@application_name", audit.ApplicationName);
				await command.ExecuteNonQueryAsync();
			}

			// Delete from feature_flags
			await using (var command = connection.CreateCommand())
			{
				command.Transaction = transaction;
				command.CommandText = @"
					DELETE FROM feature_flags 
					WHERE key = @key 
					AND application_name = @application_name";

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
				command.Transaction = transaction;
				command.CommandText = @"
					INSERT INTO feature_flags_audit (
						flag_key, application_name, application_version,
						action, actor, timestamp, notes
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
		await using var connection = new NpgsqlConnection(connectionString);
		await connection.OpenAsync();

		await using var transaction = await connection.BeginTransactionAsync();

		try
		{
			await using (var command = connection.CreateCommand())
			{
				command.Transaction = transaction;
				command.CommandText = @"
					UPDATE feature_flags 
					SET evaluation_modes = @evaluation_modes
					WHERE key = @key 
					AND application_name = @application_name";

				command.Parameters.AddWithValue("@key", flagKey);
				command.Parameters.AddWithValue("@application_name", audit.ApplicationName);
				command.Parameters.AddWithValue("@evaluation_modes", NpgsqlTypes.NpgsqlDbType.Jsonb, JsonSerializer.Serialize(evaluationModes));

				var rowsAffected = await command.ExecuteNonQueryAsync();

				if (rowsAffected == 0)
				{
					throw new InvalidOperationException($"Flag '{flagKey}' not found");
				}
			}

			// Insert audit record
			await using (var command = connection.CreateCommand())
			{
				command.Transaction = transaction;
				command.CommandText = @"
					INSERT INTO feature_flags_audit (
						flag_key, application_name, application_version,
						action, actor, timestamp, notes
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
		await using var connection = new NpgsqlConnection(connectionString);
		await connection.OpenAsync();

		var conditions = new List<string>();
		var command = connection.CreateCommand();

		// Add search conditions (OR logic)
		var searchConditions = new List<string>();

		if (!string.IsNullOrWhiteSpace(flagKey))
		{
			searchConditions.Add("key = @flagKey");
			command.Parameters.AddWithValue("@flagKey", flagKey);
		}

		if (!string.IsNullOrWhiteSpace(name))
		{
			searchConditions.Add("name ILIKE @name");
			command.Parameters.AddWithValue("@name", $"%{name}%");
		}

		if (!string.IsNullOrWhiteSpace(description))
		{
			searchConditions.Add("description ILIKE @description");
			command.Parameters.AddWithValue("@description", $"%{description}%");
		}

		if (searchConditions.Count != 0)
		{
			conditions.Add($"({string.Join(" OR ", searchConditions)})");
		}

		command.CommandText = $@"
			SELECT 
				key, application_name, application_version, scope,
				name, description, evaluation_modes,
				scheduled_enable_date, scheduled_disable_date,
				window_start_time, window_end_time, time_zone, window_days,
				targeting_rules,
				enabled_users, disabled_users, user_percentage_enabled,
				enabled_tenants, disabled_tenants, tenant_percentage_enabled,
				variations, default_variation
			FROM feature_flags
			WHERE {string.Join(" AND ", conditions)}
			ORDER BY key";

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
		await using var connection = new NpgsqlConnection(connectionString);
		await connection.OpenAsync();

		await using var command = connection.CreateCommand();
		command.CommandText = @"
			SELECT 
				key, application_name, application_version, scope,
				name, description, evaluation_modes,
				scheduled_enable_date, scheduled_disable_date,
				window_start_time, window_end_time, time_zone, window_days,
				targeting_rules,
				enabled_users, disabled_users, user_percentage_enabled,
				enabled_tenants, disabled_tenants, tenant_percentage_enabled,
				variations, default_variation
			FROM feature_flags
			ORDER BY key";

		var flags = new List<FeatureFlag>();

		await using var reader = await command.ExecuteReaderAsync();
		while (await reader.ReadAsync())
		{
			flags.Add(MapFeatureFlag(reader));
		}

		return flags;
	}

	private static FeatureFlag MapFeatureFlag(NpgsqlDataReader reader)
	{
		return new FeatureFlag
		{
			Key = reader.GetString(reader.GetOrdinal("key")),
			ApplicationName = reader.GetString(reader.GetOrdinal("application_name")),
			ApplicationVersion = reader.GetString(reader.GetOrdinal("application_version")),
			Scope = reader.GetInt32(reader.GetOrdinal("scope")),
			Name = reader.GetString(reader.GetOrdinal("name")),
			Description = reader.GetString(reader.GetOrdinal("description")),
			EvaluationModes = JsonSerializer.Deserialize<int[]>(reader.GetString(reader.GetOrdinal("evaluation_modes"))) ?? [],
			ScheduledEnableDate = reader.IsDBNull(reader.GetOrdinal("scheduled_enable_date")) ? null : reader.GetDateTime(reader.GetOrdinal("scheduled_enable_date")),
			ScheduledDisableDate = reader.IsDBNull(reader.GetOrdinal("scheduled_disable_date")) ? null : reader.GetDateTime(reader.GetOrdinal("scheduled_disable_date")),
			WindowStartTime = reader.IsDBNull(reader.GetOrdinal("window_start_time")) ? null : reader.GetTimeSpan(reader.GetOrdinal("window_start_time")),
			WindowEndTime = reader.IsDBNull(reader.GetOrdinal("window_end_time")) ? null : reader.GetTimeSpan(reader.GetOrdinal("window_end_time")),
			TimeZone = reader.IsDBNull(reader.GetOrdinal("time_zone")) ? null : reader.GetString(reader.GetOrdinal("time_zone")),
			WindowDays = JsonSerializer.Deserialize<int[]>(reader.GetString(reader.GetOrdinal("window_days"))) ?? [],
			TargetingRules = reader.IsDBNull(reader.GetOrdinal("targeting_rules")) ? null : reader.GetString(reader.GetOrdinal("targeting_rules")),
			EnabledUsers = JsonSerializer.Deserialize<string[]>(reader.GetString(reader.GetOrdinal("enabled_users"))) ?? [],
			DisabledUsers = JsonSerializer.Deserialize<string[]>(reader.GetString(reader.GetOrdinal("disabled_users"))) ?? [],
			UserPercentageEnabled = reader.GetInt32(reader.GetOrdinal("user_percentage_enabled")),
			EnabledTenants = JsonSerializer.Deserialize<string[]>(reader.GetString(reader.GetOrdinal("enabled_tenants"))) ?? [],
			DisabledTenants = JsonSerializer.Deserialize<string[]>(reader.GetString(reader.GetOrdinal("disabled_tenants"))) ?? [],
			TenantPercentageEnabled = reader.GetInt32(reader.GetOrdinal("tenant_percentage_enabled")),
			Variations = JsonSerializer.Deserialize<Dictionary<string, string>>(reader.GetString(reader.GetOrdinal("variations"))),
			DefaultVariation = reader.GetString(reader.GetOrdinal("default_variation"))
		};
	}
}