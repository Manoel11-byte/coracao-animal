using System.Net;
using System.Net.Http.Json;
using CoracaoAnimal.Client;
using CoracaoAnimal.Core;
using Xunit;
namespace CoracaoAnimal.Tests;

public class ThreeClientTests
{
 sealed class MemoryStorage:IClientStorage {
  readonly Dictionary<string,string?> values=new();
  public Task<string?> Read(string key)=>Task.FromResult(values.GetValueOrDefault(key));
  public Task Write(string key,string? value){values[key]=value;return Task.CompletedTask;}
 }
 [Fact]
 public async Task WebWindowsAndAndroidShareDataAndPasswordRevokesAllSessions()
 {
  using var factory=new Factory();using var web=factory.Client();
  var email=Guid.NewGuid()+"@example.test";const string password="Three-Clients-987654!";
  (await web.PostAsJsonAsync("/api/auth/cadastro",new Cadastro("QA Three Clients",email,password))).EnsureSuccessStatusCode();
  (await web.PostAsJsonAsync("/api/auth/login",new Login(email,password,"web"))).EnsureSuccessStatusCode();
  using var winHttp=factory.Client();using var androidHttp=factory.Client();
  var windows=new ApiClient(new MemoryStorage(),winHttp);var android=new ApiClient(new MemoryStorage(),androidHttp);
  await windows.Initialize();await android.Initialize();await windows.Login(email,password);await android.Login(email,password);
  var user=await windows.Me();Assert.Equal(user!.Id,(await android.Me())!.Id);Assert.Equal(user.Id,(await web.GetFromJsonAsync<UsuarioDto>("/api/me/"))!.Id);
  await windows.Put<UsuarioDto>("api/me/",new PerfilInput("From Windows",null,"11999999999","QA address","cao","medio","alguma"));
  Assert.Equal("From Windows",(await android.Me())!.Nome);Assert.Equal("From Windows",(await web.GetFromJsonAsync<UsuarioDto>("/api/me/"))!.Nome);
  var donation=await android.Post<DoacaoDto>("api/me/doacoes",new DoacaoInput(25.50m,"pix","QA from Android",true));
  Assert.Contains((await windows.Get<List<DoacaoDto>>("api/me/doacoes"))!,x=>x.Id==donation!.Id);
  Assert.Contains((await web.GetFromJsonAsync<List<DoacaoDto>>("/api/me/doacoes"))!,x=>x.Id==donation!.Id);
  (await web.PostAsync($"/api/me/doacoes/{donation!.Id}/cancelar",null)).EnsureSuccessStatusCode();
  Assert.Equal("cancelada",(await windows.Get<List<DoacaoDto>>("api/me/doacoes"))!.Single().Status);
  Assert.Equal("cancelada",(await android.Get<List<DoacaoDto>>("api/me/doacoes"))!.Single().Status);
  await windows.Put<VoluntarioDto>("api/me/voluntariado",new VoluntarioInput("From Windows","QA schedule"));
  Assert.Equal("From Windows",(await android.Get<VoluntarioDto>("api/me/voluntariado"))!.Area);
  (await web.DeleteAsync("/api/me/voluntariado")).EnsureSuccessStatusCode();
  Assert.Equal("inativo",(await android.Get<VoluntarioDto>("api/me/voluntariado"))!.Status);
  await android.Put<object>("api/me/senha",new SenhaInput(password,"Changed-Three-Clients-987!"));
  await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>windows.Me());
  Assert.Equal(HttpStatusCode.Unauthorized,(await web.GetAsync("/api/me/")).StatusCode);
  await windows.Login(email,"Changed-Three-Clients-987!");Assert.Equal(user.Id,(await windows.Me())!.Id);
  await windows.Logout();await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>windows.Me());
 }
}
