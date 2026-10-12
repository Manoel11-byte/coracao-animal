using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CoracaoAnimal.Core;
namespace CoracaoAnimal.Client;
public interface IClientStorage { Task<string?> Read(string key); Task Write(string key,string? value); }
public sealed class ApiClient(IClientStorage storage) {
 public const string ProductionUrl="https://coracaoanimal-api-fqegfpe7avcda5cy.chilecentral-01.azurewebsites.net/";
 readonly HttpClient http=new(new HttpClientHandler { AllowAutoRedirect=false }) { Timeout=TimeSpan.FromSeconds(30) };
 static readonly JsonSerializerOptions Json=new(JsonSerializerDefaults.Web);
 public Uri? BaseUri {get;private set;} public UsuarioDto? Usuario {get;private set;} string? token;
 public async Task Initialize(){var url=await storage.Read("url");BaseUri=ValidateUrl(string.IsNullOrWhiteSpace(url)?ProductionUrl:url);token=await storage.Read("token");}
 public static Uri ValidateUrl(string value){if(!Uri.TryCreate(value.Trim().TrimEnd('/')+"/",UriKind.Absolute,out var u)||u.UserInfo.Length>0||u.Query.Length>0||u.Fragment.Length>0||u.AbsolutePath!="/"||(u.Scheme!="https"&&!(u.Scheme=="http"&&(u.IsLoopback||u.Host=="10.0.2.2"))))throw new InvalidOperationException("Informe a URL HTTPS do site. HTTP é permitido apenas no computador local ou emulador.");return u;}
 public async Task Configure(string url){var next=ValidateUrl(url);if(BaseUri!=next)await Forget();BaseUri=next;await storage.Write("url",next.AbsoluteUri);}
 public async Task<T?> Send<T>(HttpMethod method,string path,object? body=null){BaseUri??=ValidateUrl(ProductionUrl);using var req=new HttpRequestMessage(method,new Uri(BaseUri,path.TrimStart('/')));req.Headers.Add("X-CSRF","1");if(token!=null)req.Headers.Authorization=new AuthenticationHeaderValue("Bearer",token);if(body!=null)req.Content=JsonContent.Create(body,options:Json);HttpResponseMessage response;try{response=await http.SendAsync(req);}catch(HttpRequestException){throw new InvalidOperationException("Não foi possível conectar ao servidor. Verifique sua internet e tente novamente.");}catch(TaskCanceledException){throw new InvalidOperationException("O servidor demorou para responder. Atualize antes de repetir uma operação.");}using(response){var text=await response.Content.ReadAsStringAsync();if(!response.IsSuccessStatusCode){if(response.StatusCode==HttpStatusCode.Unauthorized)await Forget();string message=response.StatusCode==HttpStatusCode.Unauthorized?"Entre novamente para continuar.":response.StatusCode==HttpStatusCode.Forbidden?"Acesso não permitido.":"Não foi possível concluir a operação.";try{message=JsonDocument.Parse(text).RootElement.GetProperty("mensagem").GetString()??message;}catch(JsonException){}catch(KeyNotFoundException){}throw new InvalidOperationException(message);}return string.IsNullOrWhiteSpace(text)?default:JsonSerializer.Deserialize<T>(text,Json);}}
 public Task<T?> Get<T>(string path)=>Send<T>(HttpMethod.Get,path);
 public Task<T?> Post<T>(string path,object? body=null)=>Send<T>(HttpMethod.Post,path,body);
 public Task<T?> Put<T>(string path,object body)=>Send<T>(HttpMethod.Put,path,body);
 public Task<object?> Delete(string path)=>Send<object>(HttpMethod.Delete,path);
 public async Task Login(string email,string senha){var s=await Post<SessaoDto>("api/auth/login",new Login(email,senha,"app"))??throw new InvalidOperationException("Resposta inválida.");token=s.Token??throw new InvalidOperationException("Sessão inválida.");Usuario=s.Usuario;await storage.Write("token",token);}
 public async Task<UsuarioDto?> Me(){Usuario=await Get<UsuarioDto>("api/me/");return Usuario;}
 public async Task Logout(){try{await Post<object>("api/auth/logout");}finally{await Forget();}}
 public async Task Forget(){token=null;Usuario=null;await storage.Write("token",null);}
}
