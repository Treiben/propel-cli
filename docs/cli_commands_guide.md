# Feature Flags CLI - Command Reference Guide
This CLI manages feature flags in your database. It's designed for simple administrative tasks and emergency toggles.

## Important Concepts

### Global vs Application Flags
- **Global Flags**: Created via CLI, shared across all applications (scope=global)
- **Application Flags**: Defined in application code, restored automatically on app startup

### CLI Limitations
- ✅ Creates **simple** On/Off flags with basic properties
- ✅ Creates **advanced** flags via JSON (scheduling, targeting, rollouts)
- ❌ Cannot update existing flags (use toggle for On/Off, or use JSON/database for complex updates)
- ❌ Cannot permanently delete application flags (they restore from code)
- ❌ Cannot create application flags. Only **GLOBAL** feature flags (scope=global, version=0.0.0.0) creation is supported by CLI

---
## Documentation

- **[Setup Guide](./setup_guide.md)** - Installation and first migration
- **[Schema Support](./schema_support_summary.md)** - PostgreSQL custom schema support

## Commands

### 1. `create` - Create a Global Feature Flag

Creates a new global feature flag with default settings.

#### Basic Usage (Simple On/Off Flag)
```bash
propel-cli create \
  --connection-string "Host=localhost;Database=mydb;Username=user;Password=pass" \
  --key my-feature-flag \
  --name "My Feature" \
  --description "Enables my feature" \
  --status off \
  --username john.doe
```

#### Advanced Usage (JSON for Complex Flags)
```bash
propel-cli create \
  --connection-string "Host=localhost;Database=mydb;Username=user;Password=pass" \
  --json '{
    "key": "scheduled-feature",
    "name": "Scheduled Feature",
    "description": "Goes live at midnight",
    "evaluation_modes": [2],
    "scheduled_enable_date": "2025-12-01T00:00:00Z",
    "scheduled_disable_date": "2025-12-31T23:59:59Z"
  }' \
  --username john.doe
```

#### Options
| Option | Required | Description |
|--------|----------|-------------|
| `--connection-string` | ✅ | Database connection string |
| `--json` | ⬜ | Complete flag as JSON (advanced mode) |
| `--key` | ⬜* | Flag key (kebab-case recommended) |
| `--name` | ⬜ | Human-readable name (defaults to key) |
| `--description` | ⬜ | Flag description |
| `--tags` | ⬜ | Tags as JSON object |
| `--status` | ⬜ | Initial status: 'on' or 'off' (default: off) |
| `--username` | ✅ | Username for audit trail |

\* Required if not using `--json`

#### Defaults Applied
- `scope`: 0 (global)
- `application_name`: "global"
- `application_version`: "0.0.0.0"
- `is_permanent`: false
- `expiration_date`: 45 days from creation
- `evaluation_modes`: [0] (Off) if status not specified

#### ⚠️ Important Notes
- **Cannot create duplicate keys** - Each flag key must be unique for global scope
- **Simple mode** creates basic On/Off flags only
- **JSON mode** supports all advanced features:
  - Scheduling (`evaluation_modes: [2]`)
  - Time windows (`evaluation_modes: [3]`)
  - User targeting (`evaluation_modes: [4]`)
  - User rollout % (`evaluation_modes: [5]`)
  - Tenant rollout % (`evaluation_modes: [6]`)
  - Tenant targeting (`evaluation_modes: [7]`)
  - Targeting rules (`evaluation_modes: [8]`)

---

### 2. `delete` - Delete a Feature Flag

Deletes a flag from the database.

#### Usage
```bash
propel-cli delete \
  --connection-string "Host=localhost;Database=mydb;Username=user;Password=pass" \
  --key my-feature-flag \
  --username john.doe
```

#### Options
| Option | Required | Description |
|--------|----------|-------------|
| `--connection-string` | ✅ | Database connection string |
| `--key` | ✅ | Exact flag key to delete |
| `--username` | ✅ | Username for audit trail |

#### ⚠️ CRITICAL WARNINGS

**For Global Flags (CLI-created):**
- ⚠️ **HIGH RISK** - Use with extreme caution
- Global flags affect all applications using the database
- Consider toggling to 'off' instead of deleting

**For Application Flags:**
- ❌ **Database deletion is TEMPORARY**
- Flags will **automatically restore** when application starts
- To permanently remove:
  1. Delete from application code
  2. Deploy application
  3. Then optionally clean up database

**When to Use:**
- ✅ Cleaning up CLI-created test flags
- ✅ Removing expired promotional flags
- ❌ NOT for application-managed flags
- ❌ NOT for production global flags without approval

---

### 3. `toggle` - Toggle Flag On/Off

Switches a flag between On and Off states.

#### Usage - Enable Flag
```bash
propel-cli toggle \
  --connection-string "Host=localhost;Database=mydb;Username=user;Password=pass" \
  --key my-feature-flag \
  --on \
  --username john.doe
```

#### Usage - Disable Flag
```bash
propel-cli toggle \
  --connection-string "Host=localhost;Database=mydb;Username=user;Password=pass" \
  --key my-feature-flag \
  --off \
  --username john.doe
```

#### Options
| Option | Required | Description |
|--------|----------|-------------|
| `--connection-string` | ✅ | Database connection string |
| `--key` | ✅ | Exact flag key to toggle |
| `--on` | ⬜* | Enable flag (set evaluation_modes to [1]) |
| `--off` | ⬜* | Disable flag (set evaluation_modes to [0]) |
| `--username` | ✅ | Username for audit trail |

\* Exactly one of `--on` or `--off` required

#### 🔥 CRITICAL WARNING: Destructive Operation

**This command REPLACES all evaluation modes with simple On/Off:**

What Gets **REMOVED**:
- ❌ Schedules (`scheduled_enable_date`, `scheduled_disable_date`)
- ❌ Time windows (`window_start_time`, `window_end_time`, `time_zone`, `window_days`)
- ❌ User targeting (`enabled_users`, `disabled_users`, `targeting_rules`)
- ❌ Tenant targeting (`enabled_tenants`, `disabled_tenants`)
- ❌ Rollout percentages (`user_percentage_enabled`, `tenant_percentage_enabled`)

**Before:**
```json
{
  "evaluation_modes": [2, 3, 4],
  "scheduled_enable_date": "2025-12-01T00:00:00Z",
  "window_start_time": "09:00:00",
  "enabled_users": ["user1", "user2"]
}
```

**After toggle --on:**
```json
{
  "evaluation_modes": [1]
  // All other fields remain but are ignored
}
```

**When to Use:**
- ✅ Emergency kill switch (immediate disable)
- ✅ Simple On/Off flags
- ✅ Quick production hotfix
- ❌ NOT for flags with complex evaluation logic
- ❌ NOT when you need to preserve scheduling/targeting

**To Restore Advanced Modes:**
- Use `create --json` with full flag definition
- Or update database directly with SQL
- Or re-seed from migration scripts

---

### 4. `find` - Search for Flags

Search flags by key, name, or description.

#### Usage - Search by Key (Exact)
```bash
propel-cli find \
  --connection-string "Host=localhost;Database=mydb;Username=user;Password=pass" \
  --key my-feature-flag
```

#### Usage - Search by Name (Partial)
```bash
propel-cli find \
  --connection-string "Host=localhost;Database=mydb;Username=user;Password=pass" \
  --name "payment"
```

#### Usage - Multiple Criteria (OR Logic)
```bash
propel-cli find \
  --connection-string "Host=localhost;Database=mydb;Username=user;Password=pass" \
  --name "checkout" \
  --description "processor" \
  --format json
```

#### Options
| Option | Required | Description |
|--------|----------|-------------|
| `--connection-string` | ✅ | Database connection string |
| `--key` | ⬜* | Exact key match (case-sensitive) |
| `--name` | ⬜* | Partial name match (case-insensitive) |
| `--description` | ⬜* | Partial description match (case-insensitive) |
| `--format` | ⬜ | Output format: 'table' (default) or 'json' |

\* At least one search criterion required

#### Search Logic

**Key Search:**
- ✅ Exact match only
- ✅ Case-sensitive
- Example: `--key checkout-v2` matches only "checkout-v2"

**Name/Description Search:**
- ✅ Partial match (contains)
- ✅ Case-insensitive
- Example: `--name payment` matches:
  - "New Payment Processor"
  - "Legacy Payment System"
  - "payment-gateway-v2"

**Multiple Criteria:**
- Combined with **OR** logic (any match returns result)
- Example: `--name payment --description checkout`
  - Returns flags with "payment" in name **OR** "checkout" in description

#### Output Formats

**Table Format (Default):**
```
Total: 3 flag(s)

Key                    Name                   Modes              Description
--------------------------------------------------------------------------------
payment-processor      Payment Processor      On                 Handles payments
checkout-v2            New Checkout           Scheduled          Goes live Dec 1st
user-targeting-test    User Test              UserTargeted       Testing for admins
```

**JSON Format:**
```json
[
  {
    "key": "payment-processor",
    "name": "Payment Processor",
    "evaluation_modes": [1],
    "description": "Handles payment processing",
    ...
  }
]
```

---

### 5. `list` - List All Global Flags

Shows all global flags in the database.

#### Usage - Table Format
```bash
propel-cli list \
  --connection-string "Host=localhost;Database=mydb;Username=user;Password=pass"
```

#### Usage - JSON Format
```bash
propel-cli list \
  --connection-string "Host=localhost;Database=mydb;Username=user;Password=pass" \
  --format json
```

#### Options
| Option | Required | Description |
|--------|----------|-------------|
| `--connection-string` | ✅ | Database connection string |
| `--format` | ⬜ | Output format: 'table' (default) or 'json' |

#### What Gets Listed
- ✅ All flags with `application_name='global'` and `application_version='0.0.0.0'`
- ✅ Flags created via CLI
- ✅ Application flags designated as global
- ❌ Application-specific flags (different scope/version)

#### Output Formats

Same as `find` command - see above for examples.

---

## Best Practices

### Creating Flags

**Simple Flags (On/Off only):**
```bash
# Development/testing
propel-cli create --key test-feature --status off --username dev.user

# Production (prefer detailed info)
propel-cli create \
  --key production-feature \
  --name "Production Feature" \
  --description "Enables XYZ functionality for all users" \
  --status off \
  --username ops.team
```

**Complex Flags (Use JSON):**
```bash
propel-cli create --json '{
  "key": "scheduled-launch",
  "name": "Product Launch",
  "description": "New product launches Dec 1st",
  "evaluation_modes": [2],
  "scheduled_enable_date": "2025-12-01T00:00:00Z"
}' --username marketing.team
```

### Managing Flags

**DO:**
- ✅ Use descriptive kebab-case keys: `new-payment-processor`
- ✅ Add meaningful names and descriptions
- ✅ Include username for audit trail
- ✅ Test in non-production first
- ✅ Use toggle for simple On/Off switches
- ✅ Use JSON for complex evaluation logic

**DON'T:**
- ❌ Delete global flags without team approval
- ❌ Toggle flags with complex schedules (you'll lose the schedule)
- ❌ Use spaces or special characters in keys
- ❌ Create duplicate keys
- ❌ Delete application flags (they restore automatically)

### Emergency Procedures

**Kill Switch (Immediate Disable):**
```bash
propel-cli toggle --key problematic-feature --off --username ops.oncall
```

**Emergency Enable:**
```bash
propel-cli toggle --key critical-fix --on --username ops.oncall
```

**Verify Changes:**
```bash
propel-cli find --key feature-name --format json
```

---

## Evaluation Modes Reference

| Mode | Value | Name | Description |
|------|-------|------|-------------|
| Off | 0 | Off | Flag disabled for everyone |
| On | 1 | On | Flag enabled for everyone |
| Scheduled | 2 | Scheduled | Enable/disable at specific dates |
| TimeWindow | 3 | TimeWindow | Enable during specific hours/days |
| UserTargeted | 4 | UserTargeted | Enable for specific users |
| UserRollout% | 5 | UserRolloutPercentage | Enable for X% of users |
| TenantRollout% | 6 | TenantRolloutPercentage | Enable for X% of tenants |
| TenantTargeted | 7 | TenantTargeted | Enable for specific tenants |
| TargetingRules | 8 | TargetingRules | Custom attribute-based rules |

**Modes can be combined:**
```json
{
  "evaluation_modes": [2, 3, 4],
  "scheduled_enable_date": "2025-12-01T00:00:00Z",
  "window_start_time": "09:00:00",
  "window_end_time": "17:00:00",
  "enabled_users": ["admin", "beta-tester"]
}
```

---

## Troubleshooting

### "Database does not exist"
**Cause:** Migrations haven't been run  
**Solution:** Run `propel-cli migrate` first

### "Flag already exists"
**Cause:** Duplicate key  
**Solution:** Use different key or delete existing flag

### "Flag not found"
**Cause:** Wrong key or flag doesn't exist  
**Solution:** Use `list` or `find` to verify flag exists

### "Must specify either --on or --off"
**Cause:** Toggle command needs direction  
**Solution:** Add `--on` or `--off` flag

### Application flag keeps coming back after deletion
**Cause:** Normal behavior - app restores its flags  
**Solution:** Delete from application code, not database

---

## Examples

### Create Simple Flag
```bash
propel-cli create \
  --connection-string "Host=localhost;Database=flags;Username=admin;Password=secret" \
  --key enable-chat \
  --name "Live Chat" \
  --description "Enable live chat widget" \
  --status off \
  --username john.doe
```

### Create Scheduled Flag
```bash
propel-cli create \
  --connection-string "Host=localhost;Database=flags;Username=admin;Password=secret" \
  --json '{
    "key": "black-friday-sale",
    "name": "Black Friday Sale",
    "description": "Special pricing for Black Friday",
    "evaluation_modes": [2],
    "scheduled_enable_date": "2025-11-29T00:00:00Z",
    "scheduled_disable_date": "2025-11-30T23:59:59Z"
  }' \
  --username marketing.team
```

### Emergency Disable
```bash
propel-cli toggle \
  --connection-string $PROD_CONNECTION_STRING \
  --key broken-feature \
  --off \
  --username ops.oncall
```

### Find All Payment Flags
```bash
propel-cli find \
  --connection-string $CONNECTION_STRING \
  --name payment \
  --format table
```

### Export All Flags as JSON
```bash
propel-cli list \
  --connection-string $CONNECTION_STRING \
  --format json > flags-backup.json
```
