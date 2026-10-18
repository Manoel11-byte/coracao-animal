using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CoracaoAnimal.Client;
using CoracaoAnimal.Core;
using Xunit;
namespace CoracaoAnimal.Tests;
public class ClientReviewTests {
 sealed class MemoryStorage:IClientStorage { public Dictionary<string,string?> Values=new();public Task<string?> Read(string key)=>Task.FromResult(Values.GetValueOrDefault(key));public Task Write(string key,string? value){Values[key]=value;return Task.CompletedTask;} }
 sealed class ResponseHandler(HttpStatusCode status,string body):HttpMessageHandler {protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)=>Task.FromResult(new HttpResponseMessage(status){Content=new StringContent(body,Encoding.UTF8,"application/json")});}
 [Theory][InlineData("25,50",25.50)][InlineData("25.50",25.50)][InlineData("1000",1000)][InlineData("1.234,56",1234.56)][InlineData(" 10,5 ",10.5)]
 public void DonationKeyboardSeparatorsPreserveTheAmount(string text,double amount)=>Assert.Equal((decimal)amount,DonationAmount.Parse(text));
 [Theory][InlineData("1.000")][InlineData("25.500")][InlineData("NaN")][InlineData("1,23.45")][InlineData("0")][InlineData("1000000.01")]
 public void DonationRejectsAmbiguousOrOutOfRangeAmounts(string text)=>Assert.Throws<InvalidOperationException>(()=>DonationAmount.Parse(text));
 [Fact]public async Task InvalidServerContractHasFriendlyMessage(){using var http=new HttpClient(new ResponseHandler(HttpStatusCode.OK,"[]"));var client=new ApiClient(new MemoryStorage(),http);await client.Initialize();var ex=await Assert.ThrowsAsync<InvalidOperationException>(()=>client.Get<Pagina<AnimalDto>>("api/animais"));Assert.Contains("incompatíveis",ex.Message);Assert.DoesNotContain("Json",ex.Message);}
 [Theory][InlineData("[]")][InlineData("<html>Unavailable</html>")][InlineData("{\"mensagem\":42}")]
 public async Task UnexpectedErrorBodyDoesNotCrashErrorHandling(string body){using var http=new HttpClient(new ResponseHandler(HttpStatusCode.ServiceUnavailable,body));var client=new ApiClient(new MemoryStorage(),http);await client.Initialize();var ex=await Assert.ThrowsAsync<InvalidOperationException>(()=>client.Get<object>("api/animais"));Assert.Contains("temporariamente",ex.Message);}
 [Fact]public async Task UnauthorizedClearsSessionAndRequestsLogin(){var storage=new MemoryStorage();storage.Values["token"]="expired";using var http=new HttpClient(new ResponseHandler(HttpStatusCode.Unauthorized,""));var client=new ApiClient(storage,http);await client.Initialize();await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>client.Me());Assert.Null(storage.Values["token"]);}
 [Fact]public async Task NativeClientAndWebShareRecords(){using var factory=new Factory();using var http=factory.Client();var storage=new MemoryStorage();var native=new ApiClient(storage,http);await native.Initialize();var email=Guid.NewGuid()+"@example.test";await native.Post<object>("api/auth/cadastro",new Cadastro("Pessoa de revisão",email,"Review-Test-Password-123!"));await native.Login(email,"Review-Test-Password-123!");using var web=factory.Client();(await web.PostAsJsonAsync("/api/auth/login",new Login(email,"Review-Test-Password-123!","web"))).EnsureSuccessStatusCode();var donation=await native.Post<DoacaoDto>("api/me/doacoes",new DoacaoInput(DonationAmount.Parse("25.50"),"pix","Teste de integração do cliente",false));var history=await web.GetFromJsonAsync<List<DoacaoDto>>("/api/me/doacoes");Assert.Contains(history!,d=>d.Id==donation!.Id&&d.Valor==25.50m);await native.Put<VoluntarioDto>("api/me/voluntariado",new VoluntarioInput("Passeios","Sábados"));Assert.Equal("Passeios",(await web.GetFromJsonAsync<VoluntarioDto>("/api/me/voluntariado"))!.Area);await native.Logout();await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>native.Me());}
 [Fact]public async Task SummaryReturnsBothTwelveMonthSeries(){using var factory=new Factory();using var http=factory.Client();var stats=await http.GetFromJsonAsync<JsonElement>("/api/resumo");Assert.Equal(DateTime.UtcNow.Year,stats.GetProperty("ano").GetInt32());Assert.Equal(12,stats.GetProperty("adocoesPorMes").GetArrayLength());Assert.Equal(12,stats.GetProperty("doacoesPorMes").GetArrayLength());}
}
