using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CoracaoAnimal.Core;
namespace CoracaoAnimal.Client;
public interface IClientStorage { Task<string?> Read(string key); Task Write(string key,string? value); }
public sealed class ApiClient(IClientStorage storage,HttpClient? transport=null) {
 public const string ProductionUrl="https://coracao-pkzqbhzfiqlry.azurewebsites.net/";
 readonly HttpClient http=transport??new(new HttpClientHandler { AllowAutoRedirect=false }) { Timeout=TimeSpan.FromSeconds(60) };
 static readonly JsonSerializerOptions Json=new(JsonSerializerDefaults.Web);
 public Uri? BaseUri {get;private set;} public UsuarioDto? Usuario {get;private set;} string? token;
 public async Task Initialize(){var url=await storage.Read("url");BaseUri=ValidateUrl(ProductionUrl);if(!string.IsNullOrWhiteSpace(url)&&url!=BaseUri.AbsoluteUri)await Forget();await storage.Write("url",BaseUri.AbsoluteUri);token=await storage.Read("token");}
 public static Uri ValidateUrl(string value){if(!Uri.TryCreate(value.Trim().TrimEnd('/')+"/",UriKind.Absolute,out var u)||u.UserInfo.Length>0||u.Query.Length>0||u.Fragment.Length>0||u.AbsolutePath!="/"||(u.Scheme!="https"&&!(u.Scheme=="http"&&(u.IsLoopback||u.Host=="10.0.2.2"))))throw new InvalidOperationException("Informe a URL HTTPS do site. HTTP é permitido apenas no computador local ou emulador.");return u;}
 public async Task Configure(string url){var next=ValidateUrl(url);if(BaseUri!=next)await Forget();BaseUri=next;await storage.Write("url",next.AbsoluteUri);}
 public async Task<T?> Send<T>(HttpMethod method,string path,object? body=null){
  if(BaseUri==null)throw new InvalidOperationException("Aguarde a inicialização do aplicativo.");
  using var req=new HttpRequestMessage(method,new Uri(BaseUri,path.TrimStart('/')));req.Headers.Add("X-CSRF","1");if(token!=null)req.Headers.Authorization=new AuthenticationHeaderValue("Bearer",token);if(body!=null)req.Content=JsonContent.Create(body,options:Json);
  try{
   using var response=await http.SendAsync(req);var text=await response.Content.ReadAsStringAsync();
   if(!response.IsSuccessStatusCode){
    var message=response.StatusCode switch{
     HttpStatusCode.Unauthorized=>"Entre na sua conta para continuar.",
     HttpStatusCode.Forbidden=>"Acesso não permitido.",
     HttpStatusCode.TooManyRequests=>"Muitas tentativas. Aguarde um minuto e tente novamente.",
     HttpStatusCode.ServiceUnavailable or HttpStatusCode.BadGateway or HttpStatusCode.GatewayTimeout=>"O serviço está temporariamente indisponível. Tente novamente em instantes.",
     _=>"Não foi possível concluir a operação."
    };
    try{using var error=JsonDocument.Parse(text);if(error.RootElement.ValueKind==JsonValueKind.Object&&error.RootElement.TryGetProperty("mensagem",out var detail)&&detail.ValueKind==JsonValueKind.String)message=detail.GetString()??message;}catch(JsonException){}
    if(response.StatusCode==HttpStatusCode.Unauthorized){await Forget();if(!path.TrimStart('/').Equals("api/auth/login",StringComparison.OrdinalIgnoreCase))throw new UnauthorizedAccessException(message);}
    throw new InvalidOperationException(message);
   }
   if(string.IsNullOrWhiteSpace(text))return default;
   try{return JsonSerializer.Deserialize<T>(text,Json);}catch(JsonException){throw new InvalidOperationException("O servidor retornou dados incompatíveis. Verifique se o aplicativo está atualizado e tente novamente.");}
  }catch(HttpRequestException){throw new InvalidOperationException("Não foi possível acessar o servidor. Verifique sua conexão com a internet e tente novamente.");}
  catch(TaskCanceledException){throw new InvalidOperationException("O servidor demorou para responder. Consulte o histórico antes de repetir uma solicitação ou doação.");}
 }
 public Task<T?> Get<T>(string path)=>Send<T>(HttpMethod.Get,path);
 public Task<T?> Post<T>(string path,object? body=null)=>Send<T>(HttpMethod.Post,path,body);
 public Task<T?> Put<T>(string path,object body)=>Send<T>(HttpMethod.Put,path,body);
 public Task<object?> Delete(string path)=>Send<object>(HttpMethod.Delete,path);
 public async Task Login(string email,string senha){var s=await Post<SessaoDto>("api/auth/login",new Login(email,senha,"app"))??throw new InvalidOperationException("Resposta inválida.");token=s.Token??throw new InvalidOperationException("Sessão inválida.");Usuario=s.Usuario;await storage.Write("token",token);}
 public async Task<UsuarioDto?> Me(){Usuario=await Get<UsuarioDto>("api/me/");return Usuario;}
 public async Task Logout(){try{await Post<object>("api/auth/logout");}finally{await Forget();}}
 public async Task Forget(){token=null;Usuario=null;await storage.Write("token",null);}
}
