param([Parameter(Mandatory)][string]$ResourceGroup,[Parameter(Mandatory)][string]$Location,[Parameter(Mandatory)][ValidatePattern('^[a-z][a-z0-9]{2,10}$')][string]$Prefix,[Parameter(Mandatory)][string]$AdminEmail)
$ErrorActionPreference='Stop'
function Check-Exit { if($LASTEXITCODE -ne 0){throw 'O comando anterior falhou. A implantação foi interrompida.'} }
$root=Split-Path $PSScriptRoot -Parent
az account show --output table; Check-Exit
Write-Host 'A implantação cria recursos pagos: App Service B1, SQL Basic, armazenamento e logs.'
if((Read-Host 'Confira a assinatura acima. Digite IMPLANTAR para continuar') -cne 'IMPLANTAR'){return}
az group create --name $ResourceGroup --location $Location --output none; Check-Exit
function New-Secret {return [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(36))+'aA1!'}
$bootstrap=Read-Host 'Senha inicial do administrador do site (mínimo 16 caracteres)' -AsSecureString
$bootstrapText=[Net.NetworkCredential]::new('',$bootstrap).Password
if($bootstrapText.Length -lt 16){throw 'Senha muito curta.'}
$params=@{sqlPassword=@{value=(New-Secret)};runtimePassword=@{value=(New-Secret)};bootstrapPassword=@{value=$bootstrapText};bootstrapEmail=@{value=$AdminEmail};prefix=@{value=$Prefix}}
$temp=Join-Path ([IO.Path]::GetTempPath()) ('ca-'+[Guid]::NewGuid()+'.json')
try {
 @{parameters=$params} | ConvertTo-Json -Depth 5 | Set-Content $temp
 az deployment group what-if --resource-group $ResourceGroup --template-file "$root/infra/main.bicep" --parameters "@$temp"; Check-Exit
 if((Read-Host 'Confira os recursos previstos. Digite CONFIRMAR para criar') -cne 'CONFIRMAR'){return}
 $raw=az deployment group create --name coracaoanimal --resource-group $ResourceGroup --template-file "$root/infra/main.bicep" --parameters "@$temp" --query properties.outputs --output json; Check-Exit
 $result=$raw | ConvertFrom-Json
} finally {Remove-Item $temp -Force -ErrorAction SilentlyContinue;$bootstrapText=$null;$params=$null}
& "$PSScriptRoot/publish-web.ps1" -ResourceGroup $ResourceGroup -AppName $result.appName.value
Write-Host ('Site: '+$result.siteUrl.value)
Write-Host 'Após confirmar o primeiro acesso administrativo, execute finalize-azure.ps1 para remover a configuração de provisionamento.'
