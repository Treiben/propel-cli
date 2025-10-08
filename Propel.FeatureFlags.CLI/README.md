# Propel CLI

A cross-platform CLI for managing the Propel Feature Flags system. Currently supports database migrations with automatic provider detection, flexible authentication, and CI/CD integration. Future releases will include feature flag management capabilities.

## Features

- ✅ **Multi-Database Support**: SQL Server and PostgreSQL
- ✅ **Auto-Detection**: Automatically detects database provider from connection string
- ✅ **Flexible Authentication**: Username/password, Windows Auth, Azure Managed Identity, and more
- ✅ **Embedded Migrations**: No external files needed in production
- ✅ **PostgreSQL Schema Support**: Custom schema support via `--schema` parameter
- ✅ **CI/CD Ready**: Environment variable support and minimal dependencies
- ✅ **Rollback Support**: Safe rollback to any previous version
- ✅ **Version Tracking**: Built-in migration history tracking

## Quick Start

### Installation

```bash
# .NET Global Tool (Recommended)
dotnet tool install -g Propel.FeatureFlags.CLI

# Verify
propel-cli --version
```

For other installation options, see the [Setup Guide](./docs/setup_guide.md).

### Run Your First Migration

```bash
# Using connection string (auto-detects provider)
propel-cli migrate --connection-string "Host=localhost;Database=featureflags;Username=postgres;Password=postgres"

# Using individual parameters
propel-cli migrate \
  --host localhost \
  --database featureflags \
  --username postgres \
  --password postgres

# Using environment variables
export DB_CONNECTION_STRING="Host=localhost;Database=featureflags;Username=postgres;Password=postgres"
propel-cli migrate
```

## Commands

| Command | Description |
|---------|-------------|
| `migrate` | Run pending database migrations |
| `rollback` | Rollback migrations to a specific version |
| `status` | Show migration status |
| `baseline` | Create baseline migration entry for existing databases |
| `seed` | Run database seed scripts |

### Common Options

All commands support these connection options:

- `--connection-string` - Full connection string (auto-detects provider)
- `--host`, `--database`, `--username`, `--password` - Individual connection parameters
- `--port` - Database port (defaults: 5432 for PostgreSQL, 1433 for SQL Server)
- `--schema` - PostgreSQL schema name (defaults to `public`)
- `--provider` - Database provider: `sqlserver` or `postgresql` (auto-detected if not specified)
- `--auth-mode` - Authentication mode (see [Authentication](#authentication))

Run `propel-cli <command> --help` for detailed command options.

## Authentication

The CLI supports multiple authentication methods:

| Mode | Description | Usage |
|------|-------------|-------|
| `UserPassword` | Username/password (default) | `--username user --password pass` |
| `IntegratedSecurity` | Windows Authentication (SQL Server) | `--auth-mode IntegratedSecurity` |
| `AzureManagedIdentity` | Azure Managed Identity | `--auth-mode AzureManagedIdentity` |
| `AzureActiveDirectory` | Azure AD Interactive | `--auth-mode AzureActiveDirectory` |
| `SslCertificate` | SSL Certificate (PostgreSQL) | `--auth-mode SslCertificate` |

## Environment Variables

The CLI reads from environment variables:

| Variable | Description |
|----------|-------------|
| `DB_CONNECTION_STRING` | Full connection string |
| `DB_HOST`, `DB_DATABASE`, `DB_USERNAME`, `DB_PASSWORD` | Individual connection parameters |
| `DB_PORT` | Database port |
| `DB_SCHEMA` | PostgreSQL schema (search_path) |
| `DB_PROVIDER` | Database provider (`sqlserver` or `postgresql`) |

## PostgreSQL Schema Support

Run migrations in custom schemas:

```bash
# Via parameter
propel-cli migrate --host localhost --database mydb --username user --password pass --schema mycompany

# Via environment variable
export DB_SCHEMA=mycompany
propel-cli migrate

# Via connection string
propel-cli migrate --connection-string "Host=localhost;Database=mydb;Username=user;Password=pass;SearchPath=mycompany"
```

The CLI automatically creates the schema if it doesn't exist. See [Schema Support Documentation](./docs/schema_support_summary.md) for details.

## Documentation

- **[Setup Guide](./docs/setup_guide.md)** - Installation and first migration
- **[CI/CD Integration](./docs/cicd_examples.md)** - Examples for GitHub Actions, GitLab CI, Azure DevOps, Jenkins, etc.
- **[Schema Support](./docs/schema_support_summary.md)** - PostgreSQL custom schema support

## CI/CD Quick Example

```yaml
# GitHub Actions
- name: Install CLI
  run: dotnet tool install -g Propel.FeatureFlags.CLI
  
- name: Run Migrations
  run: propel-cli migrate
  env:
    DB_CONNECTION_STRING: ${{ secrets.DB_CONNECTION_STRING }}
```

See [CI/CD Examples](./docs/cicd_examples.md) for GitLab CI, Azure DevOps, Jenkins, CircleCI, Kubernetes, and more.

## Development

### Build

```bash
dotnet build
```

### Run Locally

```bash
dotnet run -- migrate --connection-string "your-connection-string"
```

### Publish Self-Contained Binary

```bash
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true
```

## Creating Custom Migrations

Migrations are embedded in the CLI binary. To add new migrations:

1. Create migration files in provider-specific folders:
   ```
   migrations/
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
   DROP TABLE IF EXISTS my_table;
   ```

4. Rebuild: `dotnet build`

## Troubleshooting

### "Could not detect database provider"
Explicitly specify: `--provider postgresql` or `--provider sqlserver`

### "No embedded migration resources found"
1. Verify folder structure: `migrations/SqlServer/` and `migrations/PostgreSQL/`
2. Check `.csproj` has: `<EmbeddedResource Include="migrations\**\*.sql" />`
3. Rebuild: `dotnet clean && dotnet build`

### "propel-cli: command not found"
Add .NET tools to PATH:
```bash
export PATH="$PATH:$HOME/.dotnet/tools"
```

For more troubleshooting, see the [Setup Guide](./docs/setup_guide.md#troubleshooting-first-time-setup).

## Roadmap

- ✅ Database migrations (SQL Server, PostgreSQL)
- ✅ Schema support for PostgreSQL
- 🔄 Feature flag management commands (coming soon)
- 🔄 MySQL support (planned)
- 🔄 SQLite support (planned)

## Contributing

Contributions are welcome! Please open an issue or submit a pull request on [GitHub](https://github.com/Treiben/propel-cli).

## License

Apache-2.0 License - see LICENSE file for details

## Support

- **Issues**: [GitHub Issues](https://github.com/Treiben/propel-cli/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Treiben/propel-cli/discussions)

---
