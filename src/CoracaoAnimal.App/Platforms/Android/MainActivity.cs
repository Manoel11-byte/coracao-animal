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
#if NATIVE_TEST_ANDROID
 // Instrumentation-only key: dismiss the IME without sending Android Back.
 // This handler is absent from distributable APKs.
 public override bool OnKeyDown(Android.Views.Keycode keyCode,Android.Views.KeyEvent? e)
 {
  if(keyCode==Android.Views.Keycode.F12)
  {
   if(CurrentFocus is { } focus)
   {
    if(GetSystemService(Android.Content.Context.InputMethodService) is Android.Views.InputMethods.InputMethodManager keyboard)
     keyboard.HideSoftInputFromWindow(focus.WindowToken,Android.Views.InputMethods.HideSoftInputFlags.None);
    focus.ClearFocus();
   }
   return true;
  }
  return base.OnKeyDown(keyCode,e);
 }
#endif

}
