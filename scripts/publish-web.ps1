param([Parameter(Mandatory)][string]$ResourceGroup,[Parameter(Mandatory)][string]$AppName)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'artifacts/web'
dotnet publish "$root/src/CoracaoAnimal.Api" -c Release -o $out
if($LASTEXITCODE -ne 0){throw 'Compilação falhou.'}
$zip=Join-Path $root 'artifacts/web.zip'
Compress-Archive -Path "$out/*" -DestinationPath $zip -Force
az webapp deploy --resource-group $ResourceGroup --name $AppName --src-path $zip --type zip
if($LASTEXITCODE -ne 0){throw 'Publicação falhou. Verifique os logs no Azure.'}
