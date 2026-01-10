# Deploy to Azure Static Web Apps with Environment Management

This composite action wraps the `Azure/static-web-apps-deploy` action and adds automatic preview environment management to prevent hitting Azure Static Web Apps' preview environment limits.

## Features

- Deploys to Azure Static Web Apps using the official action
- Automatically checks preview environment count before deployment
- Deletes the oldest preview environment when the limit is reached
- Configurable maximum environment limit
- Optional environment cleanup (can be disabled)
- Graceful degradation if Azure credentials are not provided

## Usage

### Basic Usage (without environment management)

```yaml
- name: Deploy to Azure Static Web Apps
  uses: ./.github/actions/deploy-static-web-app
  with:
    azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN }}
    app_location: "/wwwroot"
    api_location: ""
    output_location: ""
```

### Full Usage (with environment management)

```yaml
- name: Deploy to Azure Static Web Apps
  uses: ./.github/actions/deploy-static-web-app
  with:
    # Required: Azure Static Web Apps deployment token
    azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN }}
    
    # Standard Azure Static Web Apps parameters
    repo_token: ${{ secrets.GITHUB_TOKEN }}
    action: "upload"
    app_location: "Meziantou.OnlineTools/bin/Release/net10.0/publish/wwwroot"
    api_location: ""
    output_location: ""
    skip_app_build: "true"
    
    # Optional: Environment management parameters
    azure_credentials: ${{ secrets.AZURE_CREDENTIALS }}
    azure_static_web_app_name: ${{ secrets.AZURE_STATIC_WEB_APP_NAME }}
    azure_resource_group: ${{ secrets.AZURE_RESOURCE_GROUP }}
    max_environments: "3"
    cleanup_environments: "true"
```

## Inputs

### Azure Static Web Apps Deploy Inputs

| Input | Description | Required | Default |
|-------|-------------|----------|---------|
| `azure_static_web_apps_api_token` | Azure Static Web Apps API token | Yes | - |
| `repo_token` | GitHub token for commenting on PRs | No | - |
| `action` | Action to perform: upload or close | No | `upload` |
| `app_location` | Directory location of the application source code | No | `/` |
| `api_location` | Directory location of the API source code | No | `` |
| `output_location` | Directory location of the build output relative to app_location | No | `` |
| `skip_app_build` | Skip building the app | No | `false` |
| `skip_api_build` | Skip building the API | No | `false` |
| `routes_location` | Directory location of the routes configuration file | No | `` |
| `config_file_location` | Directory location of the staticwebapp.config.json file | No | `` |
| `deployment_environment` | Environment name for the deployment | No | `` |
| `production_branch` | Production branch name | No | `` |

### Environment Management Inputs

| Input | Description | Required | Default |
|-------|-------------|----------|---------|
| `azure_credentials` | Azure service principal credentials for environment management | No | `` |
| `azure_static_web_app_name` | Name of the Azure Static Web App resource | No | `` |
| `azure_resource_group` | Azure resource group name | No | `` |
| `max_environments` | Maximum number of preview environments to maintain | No | `3` |
| `cleanup_environments` | Enable automatic cleanup of old environments when limit is reached | No | `true` |

## Outputs

| Output | Description |
|--------|-------------|
| `static_web_app_url` | URL of the deployed static web app |

## How It Works

1. **Environment Check**: Before deployment, if `cleanup_environments` is enabled and Azure credentials are provided, the action checks the current number of preview environments.

2. **Cleanup**: If the number of preview environments is at or above `max_environments`, the action deletes the oldest environment (sorted by creation date).

3. **Deploy**: The action then calls `Azure/static-web-apps-deploy@v1` with the provided parameters.

4. **Graceful Degradation**: If Azure credentials are not provided or if the cleanup step fails, the action continues with the deployment (uses `continue-on-error: true`).

## Required Secrets

For environment management to work, you need to configure these secrets in your repository:

- `AZURE_CREDENTIALS`: Azure service principal credentials in JSON format
- `AZURE_STATIC_WEB_APP_NAME`: Name of the Azure Static Web App resource
- `AZURE_RESOURCE_GROUP`: Name of the Azure resource group containing the Static Web App

### Creating Azure Service Principal

```bash
az ad sp create-for-rbac \
  --name "github-actions-swa" \
  --role contributor \
  --scopes /subscriptions/{subscription-id}/resourceGroups/{resource-group} \
  --sdk-auth
```

## Notes

- Environment management only runs for pull request events
- The production/default environment is never deleted
- If Azure credentials are not configured, the action works normally without environment management
- The action uses `continue-on-error: true` for the cleanup steps to ensure deployment continues even if cleanup fails
