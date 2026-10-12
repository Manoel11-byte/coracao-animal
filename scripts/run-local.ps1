$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { Write-Host 'Instale o SDK .NET 10 e abra novamente INICIAR-SERVIDOR.cmd.' -ForegroundColor Yellow; exit 1 }
$env:ASPNETCORE_ENVIRONMENT='Development'
$env:ASPNETCORE_URLS='http://localhost:5080'
Write-Host 'CORACAO ANIMAL - SERVIDOR LOCAL' -ForegroundColor Yellow
Write-Host 'Mantenha esta janela aberta enquanto usa o site e os aplicativos.'
$env:Bootstrap__Email=Read-Host 'E-mail do administrador inicial'
$secret=Read-Host 'Senha do administrador (mínimo 10 caracteres)' -AsSecureString
$env:Bootstrap__Password=[Net.NetworkCredential]::new('',$secret).Password
try {Write-Host 'Preparando. Quando aparecer Now listening on, abra http://localhost:5080'; dotnet run --project "$root/src/CoracaoAnimal.Api"}finally{Remove-Item Env:Bootstrap__Password -ErrorAction SilentlyContinue}
