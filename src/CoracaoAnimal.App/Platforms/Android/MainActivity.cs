using Android.App;
using Android.Content.PM;
using Android.OS;
namespace CoracaoAnimal.App;

[Activity(Theme="@style/Maui.SplashTheme",MainLauncher=true,ConfigurationChanges=ConfigChanges.ScreenSize|ConfigChanges.Orientation|ConfigChanges.UiMode|ConfigChanges.ScreenLayout|ConfigChanges.SmallestScreenSize|ConfigChanges.Density)]
public class MainActivity:MauiAppCompatActivity
{
 protected override void OnCreate(Bundle? savedInstanceState)
 {
  base.OnCreate(savedInstanceState);
  if(Window is { } window)
  {
   // The app uses a light canvas even when the device theme is dark. Keep
   // system icons legible on the status and navigation bar backgrounds.
   var controller=AndroidX.Core.View.WindowCompat.GetInsetsController(window,window.DecorView);
   controller.AppearanceLightStatusBars=true;
   controller.AppearanceLightNavigationBars=true;
  }
 }
}
