targetScope = 'resourceGroup'
@minLength(3)
@maxLength(11)
param prefix string = 'coracao'
param location string = resourceGroup().location
param sqlAdmin string = 'ca_provision'
@secure()
@minLength(16)
param sqlPassword string
@secure()
@minLength(16)
param runtimePassword string
param bootstrapEmail string
@secure()
@minLength(16)
param bootstrapPassword string
var suffix = uniqueString(resourceGroup().id)
var appName = '${prefix}-${suffix}'
resource plan 'Microsoft.Web/serverfarms@2024-04-01' = {
 name: '${prefix}-plan'
 location: location
 kind: 'linux'
 sku: {name:'B1',tier:'Basic',capacity:1}
 properties: {reserved:true}
}
resource network 'Microsoft.Network/virtualNetworks@2024-05-01' = {
 name: '${prefix}-vnet'
 location: location
 properties: {
  addressSpace: {addressPrefixes:['10.42.0.0/16']}
  subnets:[{
   name:'app'
   properties:{
    addressPrefix:'10.42.1.0/24'
    delegations:[{name:'web',properties:{serviceName:'Microsoft.Web/serverFarms'}}]
    serviceEndpoints:[{service:'Microsoft.Sql'},{service:'Microsoft.Storage'}]
   }
  }]
 }
}
var subnetId = resourceId('Microsoft.Network/virtualNetworks/subnets',network.name,'app')
resource sql 'Microsoft.Sql/servers@2023-08-01' = {
 name:'${prefix}-sql-${suffix}'
 location:location
 properties:{administratorLogin:sqlAdmin,administratorLoginPassword:sqlPassword,version:'12.0',minimalTlsVersion:'1.2',publicNetworkAccess:'Enabled'}
}
resource sqlNetwork 'Microsoft.Sql/servers/virtualNetworkRules@2023-08-01' = {
 parent:sql
 name:'app-subnet'
 properties:{virtualNetworkSubnetId:subnetId,ignoreMissingVnetServiceEndpoint:false}
}
resource database 'Microsoft.Sql/servers/databases@2023-08-01' = {
 parent:sql
 name:'coracaoanimal'
 location:location
 sku:{name:'Basic',tier:'Basic',capacity:5}
 properties:{maxSizeBytes:2147483648,collation:'Latin1_General_100_CI_AS_SC_UTF8'}
}
resource backup 'Microsoft.Sql/servers/databases/backupShortTermRetentionPolicies@2023-08-01' = {
 parent:database
 name:'default'
 properties:{retentionDays:7}
}
resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
 name:'${prefix}${suffix}'
 location:location
 sku:{name:'Standard_LRS'}
 kind:'StorageV2'
 properties:{minimumTlsVersion:'TLS1_2',supportsHttpsTrafficOnly:true,allowBlobPublicAccess:false,networkAcls:{defaultAction:'Deny',bypass:'None',virtualNetworkRules:[{id:subnetId,action:'Allow'}]}}
}
resource blobs 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {parent:storage,name:'default',properties:{deleteRetentionPolicy:{enabled:true,days:7}}}
resource photos 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {parent:blobs,name:'fotos',properties:{publicAccess:'None'}}
resource vault 'Microsoft.KeyVault/vaults@2023-07-01' = {
 name:'${prefix}-${suffix}'
 location:location
 properties:{tenantId:subscription().tenantId,sku:{name:'standard',family:'A'},enableRbacAuthorization:true,enableSoftDelete:true,softDeleteRetentionInDays:7,publicNetworkAccess:'Enabled'}
}
var sqlBase = 'Server=tcp:${sql.properties.fullyQualifiedDomainName},1433;Database=${database.name};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
resource runtimeSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {parent:vault,name:'sql-runtime',properties:{value:'${sqlBase}User ID=ca_runtime;Password="${runtimePassword}";'}}
resource adminSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {parent:vault,name:'sql-provision',properties:{value:'${sqlBase}User ID=${sqlAdmin};Password="${sqlPassword}";'}}
resource photoSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {parent:vault,name:'blob',properties:{value:'DefaultEndpointsProtocol=https;AccountName=${storage.name};AccountKey=${storage.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'}}
resource bootstrapSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {parent:vault,name:'bootstrap',properties:{value:bootstrapPassword}}
resource app 'Microsoft.Web/sites@2024-04-01' = {
 name:appName
 location:location
 kind:'app,linux'
 identity:{type:'SystemAssigned'}
 properties:{
  serverFarmId:plan.id
  httpsOnly:true
  virtualNetworkSubnetId:subnetId
  siteConfig:{
   linuxFxVersion:'DOTNETCORE|10.0'
   alwaysOn:true
   minTlsVersion:'1.2'
   ftpsState:'Disabled'
   vnetRouteAllEnabled:true
   healthCheckPath:'/health/ready'
   appSettings:[
    {name:'ASPNETCORE_ENVIRONMENT',value:'Production'}
    {name:'ASPNETCORE_FORWARDEDHEADERS_ENABLED',value:'true'}
    {name:'Database__Provider',value:'SqlServer'}
    {name:'Database__Initialize',value:'true'}
    {name:'Database__AdminConnection',value:'@Microsoft.KeyVault(SecretUri=${adminSecret.properties.secretUri})'}
    {name:'ConnectionStrings__CoracaoAnimal',value:'@Microsoft.KeyVault(SecretUri=${runtimeSecret.properties.secretUri})'}
    {name:'Storage__ConnectionString',value:'@Microsoft.KeyVault(SecretUri=${photoSecret.properties.secretUri})'}
    {name:'Storage__Container',value:'fotos'}
    {name:'Bootstrap__Email',value:bootstrapEmail}
    {name:'Bootstrap__Password',value:'@Microsoft.KeyVault(SecretUri=${bootstrapSecret.properties.secretUri})'}
    {name:'WEBSITE_RUN_FROM_PACKAGE',value:'1'}
   ]
  }
 }
}
resource vaultAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
 name:guid(vault.id,app.id,'secrets-user')
 scope:vault
 properties:{roleDefinitionId:subscriptionResourceId('Microsoft.Authorization/roleDefinitions','4633458b-17de-408a-b874-0445c86b69e6'),principalId:app.identity.principalId,principalType:'ServicePrincipal'}
}
resource logs 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {name:'${prefix}-logs',location:location,properties:{sku:{name:'PerGB2018'},retentionInDays:30}}
resource diagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
 name:'application-logs'
 scope:app
 properties:{workspaceId:logs.id,logs:[{category:'AppServiceConsoleLogs',enabled:true},{category:'AppServiceHTTPLogs',enabled:true}],metrics:[{category:'AllMetrics',enabled:true}]}
}
output siteUrl string = 'https://${app.properties.defaultHostName}'
output appName string = app.name
output sqlServer string = sql.name
output databaseName string = database.name
output vaultName string = vault.name
