# Propel CLI

[![Build and Test](https://github.com/Treiben/propel-cli/actions/workflows/build.yml/badge.svg)](https://github.com/Treiben/propel-cli/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/Propel.FeatureFlags.CLI.svg)](https://www.nuget.org/packages/Propel.FeatureFlags.CLI/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Propel.FeatureFlags.CLI.svg)](https://www.nuget.org/packages/Propel.FeatureFlags.CLI/)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/download)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)

A cross-platform CLI for managing the Propel Feature Flags system. Currently supports database migrations with CI/CD integration and limited feature flags management capabilities.

## Features

- ✅ **Multi-Database Migration Support**: SQL Server and PostgreSQL
- ✅ **Feature Flags Management Support**: Simple administrative tasks and emergency toggles.

## Documentation

- **[Database Migration Guide](./docs/cli_migrations_guide.md)** - Installation and first migration
- **[Commands Guide](./docs/cli_commands_guide.md)** - Examples for CRUD operations commands

## Quick Start

### Installation

```bash
# .NET Global Tool (Recommended)
dotnet tool install -g Propel.FeatureFlags.CLI

# Verify
propel-cli --version
```

For other installation options, see the [Setup Guide](./docs/setup_guide.md).

## Troubleshooting

### "propel-cli: command not found"
Add .NET tools to PATH:
```bash
export PATH="$PATH:$HOME/.dotnet/tools"
```

For more troubleshooting, see the [Setup Guide](./docs/setup_guide.md#troubleshooting-first-time-setup).

## Roadmap

- ✅ Database migrations (SQL Server, PostgreSQL)
- ✅ Schema support for PostgreSQL
- ✅ Feature flag management commands
- 🔄 Flag evaluation support (coming soon)
- 🔄 MySQL support (planned)
- 🔄 SQLite support (planned)

## Support
- **Issues**: [GitHub Issues](https://github.com/Treiben/propel-cli/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Treiben/propel-cli/discussions)

## Contributing
Contributions are welcome! Please open an issue or submit a pull request on [GitHub](https://github.com/Treiben/propel-cli).

## License
Licensed under the Apache License, Version 2.0.
Copyright 2025 Tatyana Asriyan
