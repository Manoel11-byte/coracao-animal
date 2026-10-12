param([Parameter(Mandatory)][string]$ResourceGroup,[Parameter(Mandatory)][string]$AppName)
$ErrorActionPreference='Stop'
$hostName=az webapp show -g $ResourceGroup -n $AppName --query defaultHostName -o tsv
if($LASTEXITCODE -ne 0){throw 'Não foi possível consultar o site.'}
Invoke-RestMethod "https://$hostName/health/ready" | Out-Null
if((Read-Host 'Você confirmou o acesso administrativo no site? Digite SIM') -cne 'SIM'){return}
az webapp config appsettings delete -g $ResourceGroup -n $AppName --setting-names Database__AdminConnection Bootstrap__Password Bootstrap__Email --output none
if($LASTEXITCODE -ne 0){throw 'Não foi possível remover as configurações.'}
az webapp config appsettings set -g $ResourceGroup -n $AppName --settings Database__Initialize=false --output none
if($LASTEXITCODE -ne 0){throw 'Não foi possível desativar a inicialização.'}
Write-Host 'Configurações temporárias removidas. O aplicativo usa somente o usuário SQL ca_runtime.'
