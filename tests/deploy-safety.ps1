# Offline regression tests: an existing installation must fail before any write,
# credential generation or confirmation prompt. No Azure connection is used.
$ErrorActionPreference='Stop'
$script:commands=[Collections.Generic.List[string]]::new()
function global:az {
 $script:commands.Add(($args -join ' '))
 $global:LASTEXITCODE=0
 switch ($args[0]+' '+$args[1]) {
  'account show' { return 'Test subscription' }
  'group exists' { return 'true' }
  'resource list' {
   if($script:queryFails){$global:LASTEXITCODE=1;return}
   return '/subscriptions/test/resourceGroups/test/providers/Microsoft.Sql/servers/existing'
  }
  default {throw 'Unexpected Azure command: '+($args -join ' ')}
 }
}
function global:Read-Host {throw 'Unexpected prompt before safety checks'}
try {
 foreach($queryFails in @($false,$true)) {
  $script:queryFails=$queryFails;$script:commands.Clear();$caught=$null
  try {& "$PSScriptRoot/../scripts/deploy-azure.ps1" -ResourceGroup test -Location brazilsouth -Prefix coracao -AdminEmail test@example.test}
  catch {$caught=$_.Exception.Message}
  if(!$caught){throw 'Expected provisioning to stop'}
  if(!$queryFails -and $caught -notlike '*Instalação existente detectada*'){throw "Unexpected error: $caught"}
  if($queryFails -and $caught -notlike '*comando anterior falhou*'){throw "Failed query was not handled: $caught"}
  if($script:commands.Count -ne 3){throw 'Safety check did not stop immediately'}
  if($script:commands | Where-Object {$_ -match '^(deployment|group create|keyvault)'}){throw 'A write was attempted'}
 }
 Write-Output 'PASS: existing resources and failed queries stop before mutation or prompts.'
} finally {Remove-Item Function:\az;Remove-Item Function:\Read-Host}
