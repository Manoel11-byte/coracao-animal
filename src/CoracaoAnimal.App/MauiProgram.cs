using CoracaoAnimal.Client;
namespace CoracaoAnimal.App;
public static class MauiProgram { public static MauiApp CreateMauiApp()=>MauiApp.CreateBuilder().UseMauiApp<App>().Build(); }
public class App:Application {protected override Window CreateWindow(IActivationState? state)=>new(new MainPage());}
public sealed class Storage:IClientStorage {
 public async Task<string?> Read(string key){if(key!="token")return Preferences.Default.Get(key,(string?)null);try{return await SecureStorage.Default.GetAsync(key);}catch{SecureStorage.Default.Remove(key);return null;}}
 public async Task Write(string key,string? value){if(key=="token"){if(value==null)SecureStorage.Default.Remove(key);else await SecureStorage.Default.SetAsync(key,value);}else{if(value==null)Preferences.Default.Remove(key);else Preferences.Default.Set(key,value);}}
}
