@description('The environment name (dev, stg, prod)')
param environment string

@description('The Azure region for resources')
param location string = resourceGroup().location

@description('The SQL Server administrator login')
@secure()
param sqlAdminLogin string

@description('The SQL Server administrator password')
@secure()
param sqlAdminPassword string

@description('The Azure AD tenant ID')
param azureAdTenantId string

@description('The Azure AD client ID for the portal')
param azureAdClientId string

@description('The Azure AD client ID for Graph API')
param graphApiClientId string

@description('The Azure AD client secret for Graph API')
@secure()
param graphApiClientSecret string

@description('The email address for sending alerts')
param senderEmail string

var prefix = 'intunealerts'
var uniqueSuffix = uniqueString(resourceGroup().id)
var appServicePlanName = '${prefix}-asp-${environment}'
var webAppName = '${prefix}-api-${environment}'
var sqlServerName = '${prefix}-sql-${environment}-${uniqueSuffix}'
var sqlDatabaseName = '${prefix}-db-${environment}'
var keyVaultName = '${prefix}-kv-${environment}-${uniqueSuffix}'
var appInsightsName = '${prefix}-ai-${environment}'
var logAnalyticsName = '${prefix}-law-${environment}'
var staticWebAppName = '${prefix}-swa-${environment}'

// Log Analytics Workspace
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logAnalyticsName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// Application Insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

// Key Vault
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
  }
}

// Store secrets in Key Vault
resource sqlPasswordSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'SqlAdminPassword'
  properties: {
    value: sqlAdminPassword
  }
}

resource graphApiClientSecretKv 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'GraphApiClientSecret'
  properties: {
    value: graphApiClientSecret
  }
}

// SQL Server
resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    version: '12.0'
    minimalTlsVersion: '1.2'
  }
}

// SQL Database
resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 2147483648
  }
}

// Allow Azure services to access SQL
resource sqlFirewallRule 'Microsoft.Sql/servers/firewallRules@2023-05-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
  }
  properties: {
    reserved: false
  }
}

// Web App (API)
resource webApp 'Microsoft.Web/sites@2023-01-01' = {
  name: webAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      alwaysOn: true
      cors: {
        allowedOrigins: [
          'https://${staticWebApp.properties.defaultHostname}'
        ]
        supportCredentials: true
      }
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'AzureAd__Instance'
          value: 'https://login.microsoftonline.com/'
        }
        {
          name: 'AzureAd__TenantId'
          value: azureAdTenantId
        }
        {
          name: 'AzureAd__ClientId'
          value: azureAdClientId
        }
        {
          name: 'AzureAd__Audience'
          value: 'api://${azureAdClientId}'
        }
        {
          name: 'AzureAd__GraphApi__TenantId'
          value: azureAdTenantId
        }
        {
          name: 'AzureAd__GraphApi__ClientId'
          value: graphApiClientId
        }
        {
          name: 'AzureAd__GraphApi__ClientSecret'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=GraphApiClientSecret)'
        }
        {
          name: 'Email__SenderEmail'
          value: senderEmail
        }
        {
          name: 'Cors__AllowedOrigins__0'
          value: 'https://${staticWebApp.properties.defaultHostname}'
        }
      ]
      connectionStrings: [
        {
          name: 'DefaultConnection'
          connectionString: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabaseName};Persist Security Info=False;User ID=${sqlAdminLogin};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
          type: 'SQLAzure'
        }
      ]
    }
  }
}

// Grant Key Vault access to Web App
resource keyVaultAccessPolicy 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, webApp.id, 'Key Vault Secrets User')
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6') // Key Vault Secrets User
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Static Web App (SPA)
resource staticWebApp 'Microsoft.Web/staticSites@2023-01-01' = {
  name: staticWebAppName
  location: 'westus2' // Static Web Apps have limited regions
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {
    stagingEnvironmentPolicy: 'Enabled'
    allowConfigFileUpdates: true
  }
}

// Outputs
output webAppName string = webApp.name
output webAppHostname string = webApp.properties.defaultHostName
output staticWebAppName string = staticWebApp.name
output staticWebAppHostname string = staticWebApp.properties.defaultHostname
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output keyVaultName string = keyVault.name
output appInsightsName string = appInsights.name
output appInsightsInstrumentationKey string = appInsights.properties.InstrumentationKey
