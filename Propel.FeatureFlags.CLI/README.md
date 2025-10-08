# Feature Flags Migration CLI

A powerful, cross-platform database migration tool for the Propel Feature Flags system. Supports SQL Server and PostgreSQL with automatic provider detection, flexible authentication, and CI/CD integration.

## Features

- ✅ **Multi-Database Support**: SQL Server and PostgreSQL
- ✅ **Auto-Detection**: Automatically detects database provider from connection string
- ✅ **Flexible Authentication**: Username/password, Windows Auth, Azure Managed Identity, and more
- ✅ **Embedded Migrations**: No external files needed in production
- ✅ **CI/CD Ready**: Environment variable support and minimal dependencies
- ✅ **Rollback Support**: Safe rollback to any previous version
- ✅ **Version Tracking**: Built-in migration history tracking
- ✅ **Seed Data**: Support for database seeding

## Installation

### Option 1: .NET Global Tool (Recommended)

```bash
dotnet tool install -g Propel.FeatureFlags.MigrationsCLI
```

### Option 2: Download Binary

Download the latest release for your platform from [GitHub Releases](https://github.com/yourorg/propel-featureflags-migrations/releases):

- **Linux x64**: `migrations-cli-linux-x64.tar.gz`
- **Linux ARM64**: `migrations-cli-linux-arm64.tar.gz`
- **Windows x64**: `migrations-cli-win-x64.zip`
- **macOS Intel**: `migrations-cli-osx-x64.tar.gz`
- **macOS Apple Silicon**: `migrations-cli-osx-arm64.tar.gz`

Extract and add to your PATH.

## Quick Start

### Using Username and Password

```bash
# SQL Server
migrations-cli migrate \
  --host myserver.database.windows.net \
  --database FeatureFlags \
  --username myuser \
  --password mypassword

# PostgreSQL
migrations-cli migrate \
  --host localhost \
  --database featureflags \
  --username postgres \
  --password postgres
```

### Using Connection String

```bash
# Provider is auto-detected from connection string
migrations-cli migrate \
  --connection-string "Host=localhost;Database=featureflags;Username=postgres;Password=postgres"
```

### Using Environment Variables

```bash
export DB_HOST=localhost
export DB_DATABASE=featureflags
export DB_USERNAME=postgres
export DB_PASSWORD=postgres

migrations-cli migrate
```

### Using Custom PostgreSQL Schema

```bash
# Specify schema via command line
migrations-cli migrate \
  --host localhost \
  --database featureflags \
  --username postgres \
  --password postgres \
  --schema myschema

# Or via environment variable
export DB_SCHEMA=myschema
migrations-cli migrate --host localhost --database featureflags --username postgres --password postgres

# Or in connection string
migrations-cli migrate \
  --connection-string "Host=localhost;Database=featureflags;Username=postgres;Password=postgres;SearchPath=myschema"
```

## Commands

### migrate

Run pending migrations.

```bash
migrations-cli migrate [options]
```

**Options:**
- `--connection-string` - Full connection string (optional)
- `--host` - Database host
- `--database` - Database name
- `--username` - Database username
- `--password` - Database password
- `--port` - Database port (default: 1433 for SQL Server, 5432 for PostgreSQL)
- `--schema` - PostgreSQL schema name (search_path). Defaults to 'public' if not specified. Only applicable for PostgreSQL.
- `--provider` - Database provider: `sqlserver` or `postgresql` (auto-detected if not specified)
- `--auth-mode` - Authentication mode (default: `UserPassword`)
- `--target-version` - Migrate to specific version
- `--migrations-path` - Custom migrations path (optional, for development)

**Examples:**

```bash
# Migrate to latest
migrations-cli migrate --connection-string "Server=localhost;Database=MyDb;User Id=sa;Password=Pass123"

# Migrate to specific version
migrations-cli migrate --host localhost --database mydb --username sa --password Pass123 --target-version 20250928033300

# Using Windows Authentication
migrations-cli migrate --host localhost --database mydb --auth-mode IntegratedSecurity
```

### status

Show migration status.

```bash
migrations-cli status [options]
```

**Example:**

```bash
migrations-cli status --connection-string "Host=localhost;Database=featureflags;Username=postgres;Password=postgres"
```

**Output:**
```
Migration Status:
================
Version              Status     Applied At           Description
--------------------------------------------------------------------------------
20250928033300       Applied    2025-10-07 14:30:00  Initial
20251015120000       Pending    Not Applied          Add Indexes
```

### rollback

Rollback migrations to a specific version.

```bash
migrations-cli rollback --version <version> [options]
```

**Example:**

```bash
# Rollback to initial version
migrations-cli rollback \
  --version 00000000000000 \
  --connection-string "Server=localhost;Database=MyDb;User Id=sa;Password=Pass123"
```

### baseline

Create a baseline migration entry without running scripts. Useful for existing databases.

```bash
migrations-cli baseline --version <version> [options]
```

**Example:**

```bash
migrations-cli baseline \
  --version 20250928033300 \
  --connection-string "Server=localhost;Database=MyDb;User Id=sa;Password=Pass123"
```

### seed

Run database seed scripts.

```bash
migrations-cli seed [options]
```

**Options:**
- All connection options (same as migrate)
- `--seeds-path` - Path to seeds directory (default: `./Seeds`)

**Example:**

```bash
migrations-cli seed \
  --connection-string "Host=localhost;Database=featureflags;Username=postgres;Password=postgres" \
  --seeds-path ./Seeds
```

## Authentication Modes

The CLI supports multiple authentication methods:

### UserPassword (Default)
Standard username and password authentication.

```bash
migrations-cli migrate --host localhost --database mydb --username user --password pass
```

### IntegratedSecurity
Windows Authentication for SQL Server.

```bash
migrations-cli migrate --host localhost --database mydb --auth-mode IntegratedSecurity
```

### AzureManagedIdentity
Azure Managed Identity for Azure SQL Database.

```bash
migrations-cli migrate \
  --host myserver.database.windows.net \
  --database mydb \
  --auth-mode AzureManagedIdentity
```

### AzureActiveDirectory
Azure Active Directory Interactive authentication.

```bash
migrations-cli migrate \
  --host myserver.database.windows.net \
  --database mydb \
  --username user@domain.com \
  --auth-mode AzureActiveDirectory
```

### SslCertificate
SSL Certificate authentication for PostgreSQL.

```bash
migrations-cli migrate \
  --host localhost \
  --database mydb \
  --username certuser \
  --auth-mode SslCertificate
```

## Environment Variables

The CLI supports the following environment variables:

| Variable | Description | Example |
|----------|-------------|---------|
| `DB_CONNECTION_STRING` | Full connection string | `Host=localhost;Database=mydb;...` |
| `DATABASE_URL` | Alternative connection string | `postgres://user:pass@host/db` |
| `DB_HOST` | Database host | `localhost` |
| `DB_DATABASE` | Database name | `featureflags` |
| `DB_USERNAME` | Database username | `postgres` |
| `DB_PASSWORD` | Database password | `secretpassword` |
| `DB_PORT` | Database port | `5432` |
| `DB_SCHEMA` | PostgreSQL schema (search_path) | `myschema` |
| `POSTGRES_SCHEMA` | Alternative PostgreSQL schema variable | `myschema` |
| `DB_PROVIDER` | Database provider | `postgresql` or `sqlserver` |
| `MIGRATIONS_PATH` | Custom migrations path | `./CustomMigrations` |
| `SEEDS_PATH` | Custom seeds path | `./CustomSeeds` |

## CI/CD Integration

### GitHub Actions

```yaml
name: Database Migration

on:
  push:
    branches: [main]

jobs:
  migrate:
    runs-on: ubuntu-latest
    steps:
      - name: Install Migration CLI
        run: dotnet tool install -g Propel.FeatureFlags.MigrationsCLI --version 1.0.0
      
      - name: Run Migrations
        run: migrations-cli migrate
        env:
          DB_CONNECTION_STRING: ${{ secrets.DB_CONNECTION_STRING }}
```

### GitLab CI

```yaml
migrate:
  image: mcr.microsoft.com/dotnet/sdk:8.0
  script:
    - dotnet tool install -g Propel.FeatureFlags.MigrationsCLI --version 1.0.0
    - export PATH="$PATH:/root/.dotnet/tools"
    - migrations-cli migrate
  variables:
    DB_CONNECTION_STRING: $DB_CONNECTION_STRING
```

### Azure DevOps

```yaml
trigger:
  - main

pool:
  vmImage: 'ubuntu-latest'

steps:
  - task: UseDotNet@2
    inputs:
      version: '8.0.x'
  
  - script: |
      dotnet tool install -g Propel.FeatureFlags.MigrationsCLI --version 1.0.0
      export PATH="$PATH:$HOME/.dotnet/tools"
      migrations-cli migrate
    env:
      DB_CONNECTION_STRING: $(DbConnectionString)
    displayName: 'Run Migrations'
```

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:8.0

# Install the CLI
RUN dotnet tool install -g Propel.FeatureFlags.MigrationsCLI --version 1.0.0

ENV PATH="${PATH}:/root/.dotnet/tools"

ENTRYPOINT ["migrations-cli"]
CMD ["migrate"]
```

Usage:
```bash
docker run -e DB_CONNECTION_STRING="..." your-migration-image migrate
```

### Using Binary (No .NET Required)

```yaml
# GitHub Actions
- name: Download Migration CLI
  run: |
    wget https://github.com/yourorg/propel-featureflags-migrations/releases/download/v1.0.0/migrations-cli-linux-x64.tar.gz
    tar -xzf migrations-cli-linux-x64.tar.gz
    chmod +x migrations-cli

- name: Run Migrations
  run: ./migrations-cli migrate
  env:
    DB_CONNECTION_STRING: ${{ secrets.DB_CONNECTION_STRING }}
```

## Creating Migrations

Migrations are embedded in the CLI binary. To add new migrations:

1. Create migration files in provider-specific folders:
   ```
   Migrations/
   ├── SqlServer/
   │   └── 20251101120000_AddIndexes.sql
   └── PostgreSQL/
       └── 20251101120000_AddIndexes.sql
   ```

2. Use the naming convention: `YYYYMMDDHHMMSS_Description.sql`

3. Include rollback script with `-- DOWN` marker:
   ```sql
   -- UP script
   CREATE TABLE my_table (...);
   
   -- DOWN
   -- Rollback script
   DROP TABLE IF EXISTS my_table;
   ```

4. Rebuild the CLI:
   ```bash
   dotnet build
   ```

## Development

### Prerequisites
- .NET 8.0 SDK
- SQL Server or PostgreSQL for testing

### Build

```bash
dotnet build
```

### Run Locally

```bash
dotnet run -- migrate --connection-string "your-connection-string"
```

### Pack as Tool

```bash
dotnet pack -c Release
```

### Install Locally

```bash
dotnet tool install -g --add-source ./bin/Release Propel.FeatureFlags.MigrationsCLI
```

### Publish Self-Contained Binary

```bash
# Linux
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true

# Windows
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true

# macOS
dotnet publish -c Release -r osx-x64 --self-contained -p:PublishSingleFile=true
```

## Troubleshooting

### "Could not detect database provider"

**Solution:** Explicitly specify the provider:
```bash
migrations-cli migrate --provider postgresql --connection-string "..."
```

### "No embedded migration resources found"

**Cause:** Migration files not embedded correctly.

**Solution:**
1. Verify folder structure: `Migrations/SqlServer/` and `Migrations/PostgreSQL/`
2. Check `.csproj` has: `<EmbeddedResource Include="Migrations\**\*.sql" />`
3. Rebuild: `dotnet clean && dotnet build`

### Connection Timeout

**Solution:** Increase timeout in connection string:
```
Server=myserver;Database=mydb;Connection Timeout=60;...
```

### Permission Denied

**Cause:** Database user lacks permissions.

**Solution:** Ensure user has:
- SQL Server: `CREATE DATABASE`, `CREATE TABLE`, `INSERT`, `UPDATE`, `DELETE`
- PostgreSQL: `CREATE DATABASE`, `CREATE SCHEMA`, `CREATE TABLE`, `INSERT`, `UPDATE`, `DELETE`

## License

MIT License - see LICENSE file for details

## Support

- **Issues**: [GitHub Issues](https://github.com/yourorg/propel-featureflags-migrations/issues)
- **Documentation**: [GitHub Wiki](https://github.com/yourorg/propel-featureflags-migrations/wiki)

## Contributing

Contributions are welcome! Please read our [Contributing Guide](CONTRIBUTING.md) first.

## Changelog

### Version 1.0.0 (2025-10-07)

- Initial release
- SQL Server support
- PostgreSQL support
- Auto-detection of database provider
- Multiple authentication modes
- Embedded migrations
- CI/CD integration
- Rollback support
- Seed data support
