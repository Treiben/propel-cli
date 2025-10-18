# Setup Guide

Complete setup guide for the Feature Flags CLI - from installation to your first migration.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Installation](#installation)
3. [Initial Configuration](#initial-configuration)
4. [Your First Migration](#your-first-migration)
5. [Setting Up CI/CD](#setting-up-cicd)
6. [Troubleshooting First Time Setup](#troubleshooting-first-time-setup)

---

## Prerequisites

### For .NET Global Tool Installation

- **.NET 8.0 SDK** or later
  - Check: `dotnet --version`
  - Install: https://dotnet.microsoft.com/download

### For Binary Installation

- No prerequisites! Download and run.

### Database Requirements

#### SQL Server
- SQL Server 2016 or later
- Azure SQL Database
- User with permissions:
  - `CREATE DATABASE` (if database doesn't exist)
  - `CREATE TABLE`
  - `INSERT`, `UPDATE`, `DELETE`, `SELECT`

#### PostgreSQL
- PostgreSQL 12 or later
- User with permissions:
  - `CREATE DATABASE` (if database doesn't exist)
  - `CREATE SCHEMA`
  - `CREATE TABLE`
  - `INSERT`, `UPDATE`, `DELETE`, `SELECT`

---

## Installation

### Option 1: Install as .NET Global Tool (Recommended)

```bash
# Install
dotnet tool install -g Propel.FeatureFlags.CLI

# Verify installation
propel-cli --version

# Update (when new version available)
dotnet tool update -g Propel.FeatureFlags.CLI

# Uninstall
dotnet tool uninstall -g Propel.FeatureFlags.CLI
```

### Option 2: Download Binary

1. Go to [Releases](https://github.com/Treiben/propel-cli/releases/latest)
2. Download for your platform:
   - **Linux**: `propel-cli-linux-x64.tar.gz`
   - **Windows**: `propel-cli-win-x64.zip`
   - **macOS Intel**: `propel-cli-osx-x64.tar.gz`
   - **macOS Apple Silicon**: `propel-cli-osx-arm64.tar.gz`

3. Extract and install:

**Linux/macOS:**
```bash
tar -xzf propel-cli-linux-x64.tar.gz
sudo mv propel-cli /usr/local/bin/
chmod +x /usr/local/bin/propel-cli
```

**Windows:**
```powershell
# Extract propel-cli-win-x64.zip
# Add directory to PATH or copy propel-cli.exe to a directory in PATH
```

---

## Initial Configuration

### Method 1: Environment Variables (Recommended for CI/CD)

Create a `.env` file or set environment variables:

```bash
# Basic connection
export DB_HOST=localhost
export DB_DATABASE=featureflags
export DB_USERNAME=postgres
export DB_PASSWORD=your_password
export DB_PORT=5432  # Optional, defaults: 5432 for PostgreSQL, 1433 for SQL Server

# Or use full connection string
export DB_CONNECTION_STRING="Host=localhost;Database=featureflags;Username=postgres;Password=your_password"

# Optional: Explicitly set provider (auto-detected by default)
export DB_PROVIDER=postgresql  # or sqlserver
```

**For Windows (PowerShell):**
```powershell
$env:DB_HOST="localhost"
$env:DB_DATABASE="featureflags"
$env:DB_USERNAME="postgres"
$env:DB_PASSWORD="your_password"
```

### Method 2: Command-Line Parameters

You can pass connection details directly:

```bash
propel-cli migrate \
  --host localhost \
  --database featureflags \
  --username postgres \
  --password your_password
```

### Method 3: Connection String

```bash
propel-cli migrate \
  --connection-string "Host=localhost;Database=featureflags;Username=postgres;Password=your_password"
```

---

## Your First Migration

### Step 1: Verify Connection

Test that you can connect to your database:

```bash
# Using environment variables
propel-cli status

# Or with parameters
propel-cli status --host localhost --database featureflags --username postgres --password your_password
```

**Expected Output (if no migrations have run):**
```
Checking migration status...
Provider: postgresql

Migration Status:
================
Version              Status     Applied At           Description
--------------------------------------------------------------------------------
20250928033300       Pending    Not Applied          Initial
```

### Step 2: Run Initial Migration

The CLI comes with an embedded initial migration that creates the Feature Flags schema:

```bash
propel-cli migrate
```

**Expected Output:**
```
Starting database migration...
Provider: postgresql
Applying migration 20250928033300: Initial
Migration 20250928033300 applied successfully
Migration completed successfully!
```

### Step 3: Verify Migration

Check that the migration was applied:

```bash
propel-cli status
```

**Expected Output:**
```
Migration Status:
================
Version              Status     Applied At           Description
--------------------------------------------------------------------------------
20250928033300       Applied    2025-10-07 14:30:00  Initial
```

### Step 4: Verify Tables

Connect to your database and verify tables were created:

**PostgreSQL:**
```sql
\dt  -- List tables

-- You should see:
-- feature_flags
-- feature_flags_metadata
-- feature_flags_audit
-- __migrationhistory
```

**SQL Server:**
```sql
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';

-- You should see:
-- FeatureFlags
-- FeatureFlagsMetadata
-- FeatureFlagsAudit
-- __MigrationHistory
```

---

## Setting Up CI/CD

### Step 1: Create Database Secrets

Add your database credentials as secrets in your CI/CD platform:

**GitHub Actions:**
- Go to Settings → Secrets and variables → Actions
- Add secret: `DB_CONNECTION_STRING`

**GitLab CI:**
- Go to Settings → CI/CD → Variables
- Add variable: `DB_CONNECTION_STRING` (mark as masked)

**Azure DevOps:**
- Go to Pipelines → Library → Variable groups
- Add variable: `DbConnectionString` (mark as secret)

### Step 2: Create Workflow File

Choose your platform:

#### GitHub Actions

Create `.github/workflows/migrate.yml`:

```yaml
name: Database Migration

on:
  push:
    branches: [main]

jobs:
  migrate:
    runs-on: ubuntu-latest
    steps:
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      
      - name: Install Migration CLI
        run: dotnet tool install -g Propel.FeatureFlags.CLI --version 1.0.0
      
      - name: Run Migrations
        run: propel-cli migrate
        env:
          DB_CONNECTION_STRING: ${{ secrets.DB_CONNECTION_STRING }}
```

#### GitLab CI

Create `.gitlab-ci.yml`:

```yaml
stages:
  - migrate

migrate:
  stage: migrate
  image: mcr.microsoft.com/dotnet/sdk:8.0
  script:
    - dotnet tool install -g Propel.FeatureFlags.CLI --version 1.0.0
    - export PATH="$PATH:/root/.dotnet/tools"
    - propel-cli migrate
  variables:
    DB_CONNECTION_STRING: $DB_CONNECTION_STRING
  only:
    - main
```

#### Azure DevOps

Create `azure-pipelines.yml`:

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
      dotnet tool install -g Propel.FeatureFlags.CLI --version 1.0.0
      export PATH="$PATH:$HOME/.dotnet/tools"
      propel-cli migrate
    env:
      DB_CONNECTION_STRING: $(DbConnectionString)
```

### Step 3: Test Your Pipeline

1. Commit and push the workflow file
2. Monitor the pipeline execution
3. Verify migration runs successfully

---

## Common Scenarios

### Scenario 1: Local Development

```bash
# Set up local environment
export DB_HOST=localhost
export DB_DATABASE=featureflags_dev
export DB_USERNAME=dev
export DB_PASSWORD=dev

# Run migrations
propel-cli migrate

# Check status
propel-cli status
```

### Scenario 2: Azure SQL Database with Managed Identity

```bash
propel-cli migrate \
  --host myserver.database.windows.net \
  --database featureflags \
  --auth-mode AzureManagedIdentity
```

### Scenario 3: AWS RDS PostgreSQL

```bash
propel-cli migrate \
  --host mydb.abc123.us-east-1.rds.amazonaws.com \
  --database featureflags \
  --username master \
  --password $DB_PASSWORD \
  --port 5432
```

### Scenario 4: Docker Compose Development

Create `docker-compose.yml`:

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:15
    environment:
      POSTGRES_DB: featureflags
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"

  migrations:
    image: mcr.microsoft.com/dotnet/runtime:8.0
    depends_on:
      - postgres
    environment:
      DB_HOST: postgres
      DB_DATABASE: featureflags
      DB_USERNAME: postgres
      DB_PASSWORD: postgres
    command: >
      sh -c "
        dotnet tool install -g Propel.FeatureFlags.CLI --version 1.0.0 &&
        export PATH=\"$$PATH:/root/.dotnet/tools\" &&
        sleep 5 &&
        propel-cli migrate
      "
```

Run: `docker-compose up`

---

## Troubleshooting First Time Setup

### "propel-cli: command not found"

**Solution 1 (Global Tool):**
```bash
# Check if .NET tools directory is in PATH
echo $PATH | grep .dotnet

# Add to PATH if missing
export PATH="$PATH:$HOME/.dotnet/tools"

# Make permanent (Linux/macOS)
echo 'export PATH="$PATH:$HOME/.dotnet/tools"' >> ~/.bashrc
source ~/.bashrc
```

**Solution 2 (Binary):**
```bash
# Verify binary location
which propel-cli

# If not found, add directory to PATH
export PATH="$PATH:/path/to/propel-cli"
```

### "Could not detect database provider"
Explicitly specify: `--provider postgresql` or `--provider sqlserver`

### "No embedded migration resources found"
1. Verify folder structure: `migrations/SqlServer/` and `migrations/PostgreSQL/`
2. Check `.csproj` has: `<EmbeddedResource Include="migrations\**\*.sql" />`
3. Rebuild: `dotnet clean && dotnet build`

### "Could not connect to database"

**Checklist:**
1. Database server is running: `pg_isready -h localhost` (PostgreSQL)
2. Correct hostname/port
3. Valid credentials
4. Firewall allows connection
5. Database exists (or user has CREATE DATABASE permission)

**Test connection manually:**

**PostgreSQL:**
```bash
psql -h localhost -U postgres -d postgres -c "SELECT 1;"
```

**SQL Server:**
```bash
sqlcmd -S localhost -U sa -P YourPassword -Q "SELECT 1"
```

### "Permission denied"

Grant necessary permissions:

**PostgreSQL:**
```sql
-- As superuser
GRANT CREATE ON DATABASE postgres TO myuser;
GRANT ALL PRIVILEGES ON SCHEMA public TO myuser;
```

**SQL Server:**
```sql
-- As admin
USE master;
GO
GRANT CREATE DATABASE TO myuser;
GO
```

### "Migration already applied"

This is normal! Migrations are idempotent. If a migration is already applied, it will be skipped:

```bash
propel-cli status  # Check which migrations are applied
```

---

## Next Steps

1. **Explore Commands**: Try `rollback`, `baseline`, and `seed` commands
2. **Read Documentation**: Check [README.md](..\README.md) for full command reference
3. **CI/CD Integration**: Set up automated migrations using [cicd_examples.md](cicd_examples.md)
4. **Create Custom Migrations**: Learn how to add your own migrations (see README.md)


