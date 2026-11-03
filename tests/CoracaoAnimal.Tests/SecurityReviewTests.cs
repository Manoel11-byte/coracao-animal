using System.Net;
using System.Net.Http.Json;
using CoracaoAnimal.Api.Infrastructure;
using CoracaoAnimal.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
namespace CoracaoAnimal.Tests;

public class SecurityReviewTests
{
 [Fact]
 public async Task WebAndNativeTokensCannotCrossAuthenticationChannels()
 {
  using var factory=new Factory();using var web=factory.Client();
  var response=await web.PostAsJsonAsync("/api/auth/login",new Login("admin@example.test","Test-only-Password-987!","web"));
  response.EnsureSuccessStatusCode();
  var session=await response.Content.ReadFromJsonAsync<SessaoDto>();Assert.Null(session!.Token);
  var cookie=response.Headers.GetValues("Set-Cookie").Single(x=>x.StartsWith(SessionAuth.Cookie+"="));
  Assert.Contains("httponly",cookie.ToLowerInvariant());Assert.Contains("secure",cookie.ToLowerInvariant());Assert.Contains("samesite=strict",cookie.ToLowerInvariant());
  using var attacker=factory.Client();
  attacker.DefaultRequestHeaders.Authorization=new("Bearer",cookie.Split(';')[0].Split('=',2)[1]);
  Assert.Equal(HttpStatusCode.Unauthorized,(await attacker.GetAsync("/api/me/")).StatusCode);
  using var native=factory.Client();
  var login=await native.PostAsJsonAsync("/api/auth/login",new Login("admin@example.test","Test-only-Password-987!","app"));
  login.EnsureSuccessStatusCode();var token=(await login.Content.ReadFromJsonAsync<SessaoDto>())!.Token;
  using var cookieOnly=factory.Client();cookieOnly.DefaultRequestHeaders.Add("Cookie",SessionAuth.Cookie+"="+token);
  Assert.Equal(HttpStatusCode.Unauthorized,(await cookieOnly.GetAsync("/api/me/")).StatusCode);
 }
 [Fact]
 public async Task ExpiredNativeSessionIsRejectedAndAnonymousUsersCannotReadAdministrativeLists()
 {
  using var factory=new Factory();using var native=factory.Client();
  var login=await native.PostAsJsonAsync("/api/auth/login",new Login("admin@example.test","Test-only-Password-987!","app"));
  login.EnsureSuccessStatusCode();var session=(await login.Content.ReadFromJsonAsync<SessaoDto>())!;
  await using(var scope=factory.Services.CreateAsyncScope())
  {
   var db=scope.ServiceProvider.GetRequiredService<AppDb>();
   await db.Sessoes.Where(x=>x.Hash==SessionAuth.Hash(session.Token!)).ExecuteUpdateAsync(x=>x.SetProperty(s=>s.ExpiraEm,DateTime.UtcNow.AddMinutes(-1)));
  }
  native.DefaultRequestHeaders.Authorization=new("Bearer",session.Token);
  Assert.Equal(HttpStatusCode.Unauthorized,(await native.GetAsync("/api/me/")).StatusCode);
  using var anonymous=factory.Client();
  foreach(var resource in new[]{"adotantes","adocoes","doacoes","voluntarios"})
   Assert.Equal(HttpStatusCode.Unauthorized,(await anonymous.GetAsync("/api/admin/"+resource)).StatusCode);
 }
}
