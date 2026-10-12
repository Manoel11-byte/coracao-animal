$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
dotnet publish "$root/src/CoracaoAnimal.Windows" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "$root/artifacts/windows"
if($LASTEXITCODE -ne 0){throw 'Compilação falhou.'}
Compress-Archive -Path "$root/artifacts/windows/*" -DestinationPath "$root/artifacts/CoracaoAnimal-Windows-x64.zip" -Force
