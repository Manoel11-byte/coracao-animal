param([string]$Exe)
$ErrorActionPreference='Stop'
[Console]::InputEncoding=[Text.UTF8Encoding]::new($false)
[Console]::OutputEncoding=[Text.UTF8Encoding]::new($false)
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, System.Drawing
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class NativeDriver {
 [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left,Top,Right,Bottom; }
 [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h,out RECT r);
 [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr h,IntPtr dc,uint flags);
 [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr h,uint m,IntPtr w,IntPtr l);
}
'@
$process=Start-Process $Exe -PassThru
function Find-Control($name,$kind) {
 $p=New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ProcessIdProperty,$process.Id)
 $windows=[System.Windows.Automation.AutomationElement]::RootElement.FindAll([System.Windows.Automation.TreeScope]::Children,$p)
 foreach($window in $windows){
  $items=$window.FindAll([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.Condition]::TrueCondition)
  foreach($item in $items){if($kind -eq 'edit' -and $item.Current.ControlType -ne [System.Windows.Automation.ControlType]::Edit){continue};if($kind -eq 'button' -and $item.Current.ControlType -ne [System.Windows.Automation.ControlType]::Button){continue};if($item.Current.Name -eq $name -or ($name -eq 'CONFIRM' -and $item.Current.Name -in @('Yes','Sim')) -or ($name -eq 'TERMS' -and $item.Current.Name.StartsWith('Entendo que'))){return $item}}
 }
 return $null
}
function Wait-Control($name,$kind) {
 for($i=0;$i -lt 100;$i++){
  $process.Refresh();if($process.HasExited){throw 'Application closed unexpectedly'}
  try{$control=Find-Control $name $kind;if($null -ne $control -and $control.Current.IsEnabled){return $control}}catch{# Controls are replaced during navigation; retry lookup only.
  };Start-Sleep -Milliseconds 200
 };throw "Control not available: $name"
}
try {
 while($null -ne ($line=[Console]::ReadLine())){
  try {
   $request=$line|ConvertFrom-Json;$result=@{ok=$true}
   switch($request.op){
    'wait' {$c=Wait-Control $request.name}
    'click' {$c=Wait-Control $request.name 'button';$handle=[IntPtr]$c.Current.NativeWindowHandle;if($handle -ne [IntPtr]::Zero){[void][NativeDriver]::PostMessage($handle,0xF5,[IntPtr]::Zero,[IntPtr]::Zero)}else{$c.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()}}
    'fill' {$c=Wait-Control $request.name 'edit';$c.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue([string]$request.value)}
    'read' {$c=Wait-Control $request.name 'edit';$result.value=$c.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.Value}
    'check' {$c=Wait-Control 'TERMS';$pattern=$c.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern);if($pattern.Current.ToggleState -ne [System.Windows.Automation.ToggleState]::On){$pattern.Toggle()}}
    'capture' {
     $process.Refresh();$rect=New-Object NativeDriver+RECT;[void][NativeDriver]::GetWindowRect($process.MainWindowHandle,[ref]$rect)
     $bitmap=New-Object Drawing.Bitmap(($rect.Right-$rect.Left),($rect.Bottom-$rect.Top));$g=[Drawing.Graphics]::FromImage($bitmap);$dc=$g.GetHdc()
     try{[void][NativeDriver]::PrintWindow($process.MainWindowHandle,$dc,2)}finally{$g.ReleaseHdc($dc);$g.Dispose()}
     try{$bitmap.Save([string]$request.path,[Drawing.Imaging.ImageFormat]::Png)}finally{$bitmap.Dispose()}
    }
    default {throw 'Unsupported driver operation'}
   }
   $result|ConvertTo-Json -Compress|ForEach-Object{[Console]::WriteLine($_)}
  }catch{[Console]::WriteLine((@{error=$_.Exception.Message}|ConvertTo-Json -Compress))}
 }
}finally{$process.Refresh();if(!$process.HasExited){Stop-Process -Id $process.Id}}
