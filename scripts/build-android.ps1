param([string]$KeyStore,[string]$KeyAlias='coracaoanimal')
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
if($KeyStore){
 if(!$env:CA_SIGNING_PASSWORD){throw 'Defina CA_SIGNING_PASSWORD no ambiente; não grave a senha no código.'}
 dotnet publish "$root/src/CoracaoAnimal.App" -f net10.0-android -c Release -p:AndroidPackageFormats=apk -p:AndroidKeyStore=true "-p:AndroidSigningKeyStore=$KeyStore" "-p:AndroidSigningKeyAlias=$KeyAlias" -p:AndroidSigningKeyPass=env:CA_SIGNING_PASSWORD -p:AndroidSigningStorePass=env:CA_SIGNING_PASSWORD
}else{
 dotnet build "$root/src/CoracaoAnimal.App" -f net10.0-android -c Debug -t:SignAndroidPackage
}
if($LASTEXITCODE -ne 0){throw 'Compilação falhou.'}
