$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, System.Drawing
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class NativeWindow {
 [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left, Top, Right, Bottom; }
 [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
 [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr h, IntPtr dc, uint flags);
 [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr h, IntPtr after, int x, int y, int w, int hgt, uint flags);
}
'@
New-Item -ItemType Directory -Force artifacts/windows-review | Out-Null
$process = Start-Process ./artifacts/windows/CoracaoAnimal.exe -PassThru
function Find-Control([string] $name) {
 $root = [System.Windows.Automation.AutomationElement]::FromHandle($process.MainWindowHandle)
 $condition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::NameProperty, $name)
 return $root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
}
function Wait-Control([string] $name) {
 for ($attempt=0; $attempt -lt 120; $attempt++) {
  $process.Refresh()
  if ($process.HasExited) { throw "Aplicativo encerrou: $($process.ExitCode)" }
  if ($process.MainWindowHandle -ne [IntPtr]::Zero) {
   $control = Find-Control $name
   if ($null -ne $control -and $control.Current.IsEnabled) { return $control }
  }
  Start-Sleep -Milliseconds 500
 }
 throw "Controle indisponivel: $name"
}
function Invoke-Control([string] $name) {
 $control = Wait-Control $name
 $pattern = $control.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
 $pattern.Invoke()
}
function Save-Window([string] $name) {
 Start-Sleep -Milliseconds 800
 $rect = New-Object NativeWindow+RECT
 [void][NativeWindow]::GetWindowRect($process.MainWindowHandle, [ref]$rect)
 $bitmap = New-Object System.Drawing.Bitmap(($rect.Right-$rect.Left), ($rect.Bottom-$rect.Top))
 $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
 $dc = $graphics.GetHdc()
 try { if (-not [NativeWindow]::PrintWindow($process.MainWindowHandle,$dc,2)) { throw 'Falha ao capturar janela' } }
 finally { $graphics.ReleaseHdc($dc); $graphics.Dispose() }
 try { $bitmap.Save((Join-Path $pwd "artifacts/windows-review/$name.png"),[System.Drawing.Imaging.ImageFormat]::Png) }
 finally { $bitmap.Dispose() }
}
try {
 [void](Wait-Control 'Buscar animais')
 Save-Window 'catalogo'
 [void][NativeWindow]::SetWindowPos($process.MainWindowHandle,[IntPtr]::Zero,0,0,900,650,4)
 Save-Window 'catalogo-900'
 Invoke-Control 'Como ajudar'
 [void](Wait-Control 'Registrar uma doação')
 [void](Wait-Control 'Quero ser voluntário')
 Save-Window 'ajudar'
 Invoke-Control 'Minha conta'
 [void](Wait-Control 'Entrar')
 foreach ($name in @('E-mail','Senha','Entrar','Criar conta')) {
  $control = Wait-Control $name
  if ($control.Current.IsOffscreen) { throw "Controle fora da tela: $name" }
 }
 Save-Window 'entrar'
 Invoke-Control 'Criar conta'
 [void](Wait-Control 'Cadastrar')
 Save-Window 'cadastro'
 Write-Output 'PASS: catalogo, janela 900x650, ajuda, login e cadastro; nenhuma escrita na API.'
} catch {
 if (-not $process.HasExited -and $process.MainWindowHandle -ne [IntPtr]::Zero) { Save-Window 'falha' }
 throw
} finally {
 $process.Refresh()
 if (-not $process.HasExited) { Stop-Process -Id $process.Id }
}
