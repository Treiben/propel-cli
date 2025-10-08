# CI/CD Integration Examples

Examples for integrating Propel CLI into various CI/CD platforms.

## Table of Contents

- [GitHub Actions](#github-actions)
- [GitLab CI](#gitlab-ci)
- [Azure DevOps](#azure-devops)
- [Jenkins](#jenkins)
- [CircleCI](#circleci)
- [Kubernetes CronJob](#kubernetes-cronjob)
- [Docker Compose](#docker-compose)
- [Terraform](#terraform)

---

## GitHub Actions

### Basic Migration

```yaml
name: Deploy Database

on:
  push:
    branches: [main]

jobs:
  migrate:
    runs-on: ubuntu-latest
    
    steps:
      - name: Checkout
        uses: actions/checkout@v4
      
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

### Multi-Environment Deployment

```yaml
name: Deploy to Multiple Environments

on:
  push:
    branches: [main, staging, production]

jobs:
  migrate:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        environment: [dev, staging, production]
        exclude:
          - environment: production
            branch: ${{ github.ref != 'refs/heads/main' }}
    
    environment: ${{ matrix.environment }}
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      
      - name: Install Migration CLI
        run: dotnet tool install -g Propel.FeatureFlags.CLI --version 1.0.0
      
      - name: Run Migrations (${{ matrix.environment }})
        run: propel-cli migrate
        env:
          DB_HOST: ${{ secrets.DB_HOST }}
          DB_DATABASE: ${{ secrets.DB_DATABASE }}
          DB_USERNAME: ${{ secrets.DB_USERNAME }}
          DB_PASSWORD: ${{ secrets.DB_PASSWORD }}
      
      - name: Check Migration Status
        run: propel-cli status
        env:
          DB_HOST: ${{ secrets.DB_HOST }}
          DB_DATABASE: ${{ secrets.DB_DATABASE }}
          DB_USERNAME: ${{ secrets.DB_USERNAME }}
          DB_PASSWORD: ${{ secrets.DB_PASSWORD }}
```

### Using Self-Contained Binary (No .NET Required)

```yaml
name: Migrate Database (Standalone)

on:
  push:
    branches: [main]

jobs:
  migrate:
    runs-on: ubuntu-latest
    
    steps:
      - name: Download Migration CLI
        run: |
          wget https://github.com/Treiben/propel-cli/releases/download/v1.0.0/propel-cli-linux-x64.tar.gz
          tar -xzf propel-cli-linux-x64.tar.gz
          chmod +x propel-cli
      
      - name: Run Migrations
        run: ./propel-cli migrate
        env:
          DB_CONNECTION_STRING: ${{ secrets.DB_CONNECTION_STRING }}
```

### With Caching

```yaml
name: Migrate with Cache

on:
  push:
    branches: [main]

jobs:
  migrate:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Cache .NET Tools
        uses: actions/cache@v3
        with:
          path: ~/.dotnet/tools
          key: ${{ runner.os }}-dotnet-tools-${{ hashFiles('**/global.json') }}
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      
      - name: Install or Update CLI
        run: |
          if ! command -v propel-cli &> /dev/null; then
            dotnet tool install -g Propel.FeatureFlags.CLI --version 1.0.0
          else
            dotnet tool update -g Propel.FeatureFlags.CLI --version 1.0.0
          fi
      
      - name: Run Migrations
        run: propel-cli migrate
        env:
          DB_CONNECTION_STRING: ${{ secrets.DB_CONNECTION_STRING }}
```

---

## GitLab CI

### Basic Migration

```yaml
stages:
  - migrate

migrate-database:
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

### Multi-Environment

```yaml
stages:
  - migrate

.migrate-template: &migrate-template
  image: mcr.microsoft.com/dotnet/sdk:8.0
  script:
    - dotnet tool install -g Propel.FeatureFlags.CLI --version 1.0.0
    - export PATH="$PATH:/root/.dotnet/tools"
    - propel-cli migrate --host $DB_HOST --database $DB_DATABASE --username $DB_USERNAME --password $DB_PASSWORD
    - propel-cli status --host $DB_HOST --database $DB_DATABASE --username $DB_USERNAME --password $DB_PASSWORD

migrate-dev:
  <<: *migrate-template
  stage: migrate
  environment:
    name: development
  variables:
    DB_HOST: $DEV_DB_HOST
    DB_DATABASE: $DEV_DB_DATABASE
    DB_USERNAME: $DEV_DB_USERNAME
    DB_PASSWORD: $DEV_DB_PASSWORD
  only:
    - develop

migrate-prod:
  <<: *migrate-template
  stage: migrate
  environment:
    name: production
  variables:
    DB_HOST: $PROD_DB_HOST
    DB_DATABASE: $PROD_DB_DATABASE
    DB_USERNAME: $PROD_DB_USERNAME
    DB_PASSWORD: $PROD_DB_PASSWORD
  only:
    - main
  when: manual
```

### Using Alpine (Minimal Image)

```yaml
migrate-database:
  stage: migrate
  image: alpine:latest
  before_script:
    - apk add --no-cache curl
  script:
    - curl -L https://github.com/Treiben/propel-cli/releases/download/v1.0.0/propel-cli-linux-x64.tar.gz | tar xz
    - chmod +x propel-cli
    - ./propel-cli migrate
  variables:
    DB_CONNECTION_STRING: $DB_CONNECTION_STRING
```

---

## Azure DevOps

### Basic Pipeline

```yaml
trigger:
  - main

pool:
  vmImage: 'ubuntu-latest'

steps:
  - task: UseDotNet@2
    displayName: 'Install .NET SDK'
    inputs:
      version: '8.0.x'
  
  - script: |
      dotnet tool install -g Propel.FeatureFlags.CLI --version 1.0.0
      export PATH="$PATH:$HOME/.dotnet/tools"
      propel-cli migrate
    displayName: 'Run Database Migrations'
    env:
      DB_CONNECTION_STRING: $(DbConnectionString)
```

### Multi-Stage Pipeline

```yaml
trigger:
  - main
  - develop

stages:
  - stage: Migrate_Dev
    condition: eq(variables['Build.SourceBranch'], 'refs/heads/develop')
    jobs:
      - job: MigrateDev
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
            displayName: 'Migrate Dev Database'
            env:
              DB_HOST: $(DevDbHost)
              DB_DATABASE: $(DevDbDatabase)
              DB_USERNAME: $(DevDbUsername)
              DB_PASSWORD: $(DevDbPassword)

  - stage: Migrate_Prod
    condition: eq(variables['Build.SourceBranch'], 'refs/heads/main')
    jobs:
      - deployment: MigrateProd
        environment: production
        pool:
          vmImage: 'ubuntu-latest'
        strategy:
          runOnce:
            deploy:
              steps:
                - task: UseDotNet@2
                  inputs:
                    version: '8.0.x'
                
                - script: |
                    dotnet tool install -g Propel.FeatureFlags.CLI --version 1.0.0
                    export PATH="$PATH:$HOME/.dotnet/tools"
                    propel-cli migrate
                    propel-cli status
                  displayName: 'Migrate Production Database'
                  env:
                    DB_CONNECTION_STRING: $(ProdDbConnectionString)
```

---

## Jenkins

### Declarative Pipeline

```groovy
pipeline {
    agent {
        docker {
            image 'mcr.microsoft.com/dotnet/sdk:8.0'
        }
    }
    
    environment {
        DB_CONNECTION_STRING = credentials('db-connection-string')
    }
    
    stages {
        stage('Install CLI') {
            steps {
                sh 'dotnet tool install -g Propel.FeatureFlags.CLI --version 1.0.0'
            }
        }
        
        stage('Migrate') {
            steps {
                sh '''
                    export PATH="$PATH:/root/.dotnet/tools"
                    propel-cli migrate
                '''
            }
        }
        
        stage('Status Check') {
            steps {
                sh '''
                    export PATH="$PATH:/root/.dotnet/tools"
                    propel-cli status
                '''
            }
        }
    }
    
    post {
        success {
            echo 'Migration completed successfully!'
        }
        failure {
            echo 'Migration failed!'
        }
    }
}
```

### Scripted Pipeline with Multiple Environments

```groovy
node {
    docker.image('mcr.microsoft.com/dotnet/sdk:8.0').inside {
        stage('Install CLI') {
            sh 'dotnet tool install -g Propel.FeatureFlags.CLI --version 1.0.0'
        }
        
        stage('Migrate Dev') {
            withCredentials([string(credentialsId: 'dev-db-connection', variable: 'DB_CONNECTION_STRING')]) {
                sh '''
                    export PATH="$PATH:/root/.dotnet/tools"
                    propel-cli migrate
                '''
            }
        }
        
        stage('Approval') {
            input message: 'Deploy to Production?', ok: 'Deploy'
        }
        
        stage('Migrate Prod') {
            withCredentials([string(credentialsId: 'prod-db-connection', variable: 'DB_CONNECTION_STRING')]) {
                sh '''
                    export PATH="$PATH:/root/.dotnet/tools"
                    propel-cli migrate
                '''
            }
        }
    }
}
```

---

## CircleCI

### Basic Configuration

```yaml
version: 2.1

orbs:
  dotnet: circleci/dotnet@1.0.0

jobs:
  migrate:
    docker:
      - image: mcr.microsoft.com/dotnet/sdk:8.0
    steps:
      - checkout
      
      - run:
          name: Install Migration CLI
          command: dotnet tool install -g Propel.FeatureFlags.CLI --version 1.0.0
      
      - run:
          name: Run Migrations
          command: |
            export PATH="$PATH:/root/.dotnet/tools"
            propel-cli migrate
          environment:
            DB_CONNECTION_STRING: ${DB_CONNECTION_STRING}

workflows:
  version: 2
  deploy:
    jobs:
      - migrate:
          context: production
          filters:
            branches:
              only: main
```

---

## Kubernetes CronJob

### Scheduled Migrations

```yaml
apiVersion: batch/v1
kind: CronJob
metadata:
  name: feature-flags-migrations
  namespace: production
spec:
  schedule: "0 2 * * *"  # Daily at 2 AM
  jobTemplate:
    spec:
      template:
        spec:
          containers:
          - name: migrations
            image: mcr.microsoft.com/dotnet/runtime:8.0
            command:
            - /bin/sh
            - -c
            - |
              dotnet tool install -g Propel.FeatureFlags.CLI --version 1.0.0
              export PATH="$PATH:/root/.dotnet/tools"
              propel-cli migrate
            env:
            - name: DB_CONNECTION_STRING
              valueFrom:
                secretKeyRef:
                  name: database-credentials
                  key: connection-string
          restartPolicy: OnFailure
```

### Init Container for Deployments

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: feature-flags-api
spec:
  template:
    spec:
      initContainers:
      - name: run-migrations
        image: mcr.microsoft.com/dotnet/runtime:8.0
        command:
        - /bin/sh
        - -c
        - |
          dotnet tool install -g Propel.FeatureFlags.CLI --version 1.0.0
          export PATH="$PATH:/root/.dotnet/tools"
          propel-cli migrate
        env:
        - name: DB_HOST
          valueFrom:
            secretKeyRef:
              name: db-credentials
              key: host
        - name: DB_DATABASE
          valueFrom:
            secretKeyRef:
              name: db-credentials
              key: database
        - name: DB_USERNAME
          valueFrom:
            secretKeyRef:
              name: db-credentials
              key: username
        - name: DB_PASSWORD
          valueFrom:
            secretKeyRef:
              name: db-credentials
              key: password
      
      containers:
      - name: api
        image: your-api-image:latest
        # ... rest of container spec
```

---

## Docker Compose

### Development Environment

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
    volumes:
      - postgres-data:/var/lib/postgresql/data
  
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
  
  api:
    build: .
    depends_on:
      migrations:
        condition: service_completed_successfully
    ports:
      - "8080:8080"

volumes:
  postgres-data:
```

---

## Terraform

### AWS RDS Migration

```hcl
resource "null_resource" "run_migrations" {
  depends_on = [aws_db_instance.feature_flags]
  
  provisioner "local-exec" {
    command = <<EOT
      dotnet tool install -g Propel.FeatureFlags.CLI --version 1.0.0
      export PATH="$PATH:$HOME/.dotnet/tools"
      propel-cli migrate \
        --host ${aws_db_instance.feature_flags.endpoint} \
        --database ${aws_db_instance.feature_flags.db_name} \
        --username ${var.db_username} \
        --password ${var.db_password}
    EOT
  }
  
  triggers = {
    db_instance_id = aws_db_instance.feature_flags.id
    migration_version = var.migration_version
  }
}
```

---

## Best Practices

1. **Use Secrets Management**: Never hardcode credentials
2. **Version Pin**: Always specify CLI version: `--version 1.0.0`
3. **Status Checks**: Run `status` after migrations to verify
4. **Rollback Plan**: Have a rollback strategy ready
5. **Test First**: Run migrations in non-production first
6. **Idempotent**: Ensure migrations can run multiple times safely
7. **Monitoring**: Add logging and alerting around migrations
8. **Backup**: Always backup databases before migrations

## Troubleshooting CI/CD

### Path Issues

If `propel-cli: command not found`:

```bash
export PATH="$PATH:$HOME/.dotnet/tools"  # Linux/macOS
export PATH="$PATH:/root/.dotnet/tools"  # Docker containers
```

### Timeout Issues

Increase connection timeout:
```bash
propel-cli migrate --connection-string "Server=...; Connection Timeout=120; ..."
```

### Permission Issues

Ensure CI/CD service account has necessary database permissions.
