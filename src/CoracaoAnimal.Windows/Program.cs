using System.Security.Cryptography;
using System.Text;
using CoracaoAnimal.Client;
namespace CoracaoAnimal.Windows;
static class Program {
 [STAThread] static void Main(){
  ApplicationConfiguration.Initialize();
  Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
  Application.ThreadException+=(_,e)=>ShowFatal(e.Exception);
  AppDomain.CurrentDomain.UnhandledException+=(_,e)=>ShowFatal(e.ExceptionObject as Exception??new Exception("Falha inesperada."));
  try{Application.Run(new MainForm());}catch(Exception ex){ShowFatal(ex);}
 }
 static void ShowFatal(Exception ex){
  try{
   var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"CoracaoAnimal");
   Directory.CreateDirectory(folder);
   var log=Path.Combine(folder,"erro-inicializacao.txt");
   File.WriteAllText(log,$"{DateTime.Now:O}{Environment.NewLine}{ex}");
   MessageBox.Show($"O Coração Animal encontrou um erro ao iniciar.\n\n{ex.Message}\n\nUm relatório foi salvo em:\n{log}","Coração Animal",MessageBoxButtons.OK,MessageBoxIcon.Error);
  }catch{MessageBox.Show(ex.Message,"Coração Animal",MessageBoxButtons.OK,MessageBoxIcon.Error);}
 }
}
sealed class Storage:IClientStorage {
 readonly string folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"CoracaoAnimal");
 public async Task<string?> Read(string key){var path=Path.Combine(folder,key);if(!File.Exists(path))return null;try{var b=await File.ReadAllBytesAsync(path);return Encoding.UTF8.GetString(key=="token"?ProtectedData.Unprotect(b,null,DataProtectionScope.CurrentUser):b);}catch(CryptographicException){File.Delete(path);return null;}}
 public async Task Write(string key,string? value){Directory.CreateDirectory(folder);var path=Path.Combine(folder,key);if(value==null){File.Delete(path);return;}var b=Encoding.UTF8.GetBytes(value);await File.WriteAllBytesAsync(path,key=="token"?ProtectedData.Protect(b,null,DataProtectionScope.CurrentUser):b);}
}
