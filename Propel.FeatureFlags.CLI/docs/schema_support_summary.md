# PostgreSQL Schema Support

## Overview

The Propel CLI migrations now fully supports custom PostgreSQL schemas via the `search_path` parameter. This allows migrations to run in schemas other than the default `public` schema.

## What Was Added

### 1. Environment Variable Support
**File**: `EnvironmentHelper.cs`

Added `GetSchemaFromEnvironment()` method that checks:
- `DB_SCHEMA` environment variable
- `POSTGRES_SCHEMA` environment variable (alternative)

### 2. Connection String Builder Enhancement
**File**: `ConnectionStringBuilder.cs`

- Added `schema` parameter to `BuildConnectionString()` method
- Updated `BuildPostgreSqlConnection()` to set `SearchPath` property
- Schema is passed through to Npgsql's `SearchPath` property

### 3. Command-Line Option Added
**Files**: All command files (Migrate, Rollback, Status, Baseline, Seed)

Added `--schema` option to all commands:
- Description: "PostgreSQL schema name (search_path)"
- Optional parameter
- Only applicable to PostgreSQL (ignored for SQL Server)

### 4. Documentation Updates
**File**: `README.md`

- Added `DB_SCHEMA` and `POSTGRES_SCHEMA` to environment variables table
- Added "Using Custom PostgreSQL Schema" section with examples
- Updated command options documentation

## Usage Examples

### Command-Line Parameter

```bash
propel-cli migrate \
  --host localhost \
  --database featureflags \
  --username postgres \
  --password postgres \
  --schema mycompany
```

### Environment Variable

```bash
export DB_SCHEMA=mycompany
propel-cli migrate \
  --host localhost \
  --database featureflags \
  --username postgres \
  --password postgres
```

### Full Connection String

```bash
propel-cli migrate \
  --connection-string "Host=localhost;Database=featureflags;Username=postgres;Password=postgres;SearchPath=mycompany"
```

### Priority Order

The schema is resolved in this order:
1. `--schema` command-line parameter (highest priority)
2. `DB_SCHEMA` environment variable
3. `POSTGRES_SCHEMA` environment variable
4. `SearchPath` in connection string
5. Defaults to `public` if not specified

## How It Works

### PostgreSQL Connection String Building

When you specify a schema, the CLI:

1. Accepts the schema parameter via CLI option or environment variable
2. Passes it to `ConnectionStringBuilder.BuildConnectionString()`
3. Sets `NpgsqlConnectionStringBuilder.SearchPath = schema`
4. All subsequent queries run in the specified schema

### PostgreSQL Provider Behavior

The `PostgreSqlProvider` already had schema support built-in:
- Reads `SearchPath` from the connection string
- Uses it in all SQL operations (table creation, queries, etc.)
- Defaults to `public` if not specified

## Testing Schema Support

### Test 1: Create Custom Schema Migration

```bash
# Create the schema first (one-time)
psql -h localhost -U postgres -d featureflags -c "CREATE SCHEMA IF NOT EXISTS mycompany;"

# Run migrations in custom schema
propel-cli migrate \
  --host localhost \
  --database featureflags \
  --username postgres \
  --password postgres \
  --schema mycompany
```

**Verify:**
```sql
-- Check migration table in custom schema
SELECT * FROM mycompany.__migrationhistory;

-- Check feature flags table in custom schema
SELECT * FROM mycompany.feature_flags;
```

### Test 2: Environment Variable

```bash
export DB_HOST=localhost
export DB_DATABASE=featureflags
export DB_USERNAME=postgres
export DB_PASSWORD=postgres
export DB_SCHEMA=mycompany

propel-cli migrate
```

### Test 3: Check Status in Custom Schema

```bash
propel-cli status \
  --host localhost \
  --database featureflags \
  --username postgres \
  --password postgres \
  --schema mycompany
```

### Test 4: Rollback in Custom Schema

```bash
propel-cli rollback \
  --version 00000000000000 \
  --host localhost \
  --database featureflags \
  --username postgres \
  --password postgres \
  --schema mycompany
```

## Multi-Schema Scenarios

### Scenario 1: Different Schemas per Environment

```bash
# Development (public schema)
propel-cli migrate --schema public

# Staging (staging schema)
propel-cli migrate --schema staging

# Production (production schema)
propel-cli migrate --schema production
```

### Scenario 2: Multi-Tenant with Schema-per-Tenant

```bash
# Tenant A
propel-cli migrate --schema tenant_a

# Tenant B
propel-cli migrate --schema tenant_b

# Tenant C
propel-cli migrate --schema tenant_c
```

### Scenario 3: CI/CD with Dynamic Schemas

```yaml
# GitHub Actions
- name: Run Migrations
  run: propel-cli migrate
  env:
    DB_HOST: ${{ secrets.DB_HOST }}
    DB_DATABASE: ${{ secrets.DB_DATABASE }}
    DB_USERNAME: ${{ secrets.DB_USERNAME }}
    DB_PASSWORD: ${{ secrets.DB_PASSWORD }}
    DB_SCHEMA: ${{ vars.ENVIRONMENT_SCHEMA }}  # Different per environment
```

## Important Notes

### Schema Creation

The CLI **does automatically create schemas** if they do not exist. However, the user must have permission to create schemas.

```sql
CREATE SCHEMA IF NOT EXISTS myschema;
```

Or grant the migration user permission to create schemas:
```sql
GRANT CREATE ON DATABASE featureflags TO migration_user;
```

### SQL Server

The `--schema` option is **only applicable to PostgreSQL**. SQL Server migrations always use the `dbo` schema (hardcoded in the migrations).

For SQL Server, if you need custom schemas, you must:
1. Modify the migration SQL files directly
2. Replace `[dbo]` with your schema name

### Search Path vs Schema

In PostgreSQL:
- **Schema**: A namespace within a database (e.g., `public`, `mycompany`)
- **search_path**: The connection-level setting that determines which schema to use
- The CLI sets `search_path` via the connection string

### Default Behavior

If no schema is specified:
- PostgreSQL defaults to `public` schema
- All migrations run in `public`
- Existing behavior is preserved (backward compatible)

## Troubleshooting

### "schema does not exist"

**Cause**: Schema not created before running migrations

**Solution**:
```sql
CREATE SCHEMA IF NOT EXISTS myschema;
```

### "permission denied for schema"

**Cause**: User lacks permissions on the schema

**Solution**:
```sql
GRANT ALL ON SCHEMA myschema TO migration_user;
```

### Schema Not Being Used

**Check**:
1. Is `--schema` parameter passed correctly?
2. Is environment variable set?
3. Is schema in connection string `SearchPath`?
4. Check logs for "Schema: xxx" message

### Migrations Applied to Wrong Schema

**Cause**: Schema parameter not specified, defaulted to `public`

**Solution**: Always explicitly specify schema when not using `public`:
```bash
propel-cli migrate --schema myschema ...
```

## Migration Considerations

### Migrating Between Schemas

If you need to move data between schemas:

```sql
-- Copy migration history
INSERT INTO newschema.__migrationhistory
SELECT * FROM oldschema.__migrationhistory;

-- Copy feature flags
INSERT INTO newschema.feature_flags
SELECT * FROM oldschema.feature_flags;
```

### Schema Naming Best Practices

- Use lowercase names: `myschema` not `MySchema`
- Avoid special characters
- Keep names short and descriptive
- Use underscores for separation: `tenant_abc`

## Files Modified

All changes were made to support schema in a non-breaking way:

1. ✅ `EnvironmentHelper.cs` - Added `GetSchemaFromEnvironment()`
2. ✅ `ConnectionStringBuilder.cs` - Added schema parameter
3. ✅ `MigrateCommand.cs` - Added `--schema` option
4. ✅ `RollbackCommand.cs` - Added `--schema` option
5. ✅ `StatusCommand.cs` - Added `--schema` option
6. ✅ `BaselineCommand.cs` - Added `--schema` option
7. ✅ `SeedCommand.cs` - Added `--schema` option
8. ✅ `README.md` - Added documentation

No changes needed to `PostgreSqlProvider.cs` - it already supported schemas via `SearchPath`.

## Backward Compatibility

✅ **Fully backward compatible**

- If `--schema` not specified, defaults to `public`
- Existing connection strings continue to work
- No breaking changes to existing deployments
- Optional parameter (not required)

## Next Steps

Consider these enhancements for future versions:

1. **Auto-create schemas** - Add `--create-schema` flag
2. **Schema validation** - Verify schema exists before running migrations
3. **Multi-schema migrations** - Run migrations across multiple schemas in one command
4. **Schema migration** - Tool to migrate data between schemas

---

**Schema support is now complete and ready for production use!** 🎉
