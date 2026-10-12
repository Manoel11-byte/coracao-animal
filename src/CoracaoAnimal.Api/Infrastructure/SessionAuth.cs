using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace CoracaoAnimal.Api.Infrastructure;
public sealed class SessionAuth(IOptionsMonitor<AuthenticationSchemeOptions> options,ILoggerFactory logger,UrlEncoder encoder,AppDb db):AuthenticationHandler<AuthenticationSchemeOptions>(options,logger,encoder){
 public const string Cookie="ca_session";
 public static string Hash(string token)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
 protected override async Task<AuthenticateResult> HandleAuthenticateAsync(){
  var auth=Request.Headers.Authorization.ToString();var isBearer=auth.StartsWith("Bearer ",StringComparison.OrdinalIgnoreCase);
  var token=isBearer?auth[7..]:Request.Cookies[Cookie];if(string.IsNullOrWhiteSpace(token)||token.Length>256)return AuthenticateResult.NoResult();
  var hash=Hash(token);var s=await db.Sessoes.AsNoTracking().Include(x=>x.Usuario).SingleOrDefaultAsync(x=>x.Hash==hash);
  if(s is null||s.ExpiraEm<=DateTime.UtcNow||isBearer!=(s.Cliente=="app"))return AuthenticateResult.Fail("Sessão inválida.");
  Claim[] claims=[new(ClaimTypes.NameIdentifier,s.UsuarioId.ToString()),new(ClaimTypes.Name,s.Usuario.Nome),new(ClaimTypes.Role,s.Usuario.Papel),new("cliente",s.Cliente),new("sessao",s.Hash)];
  return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(claims,Scheme.Name)),Scheme.Name));
 }
}
public static class UserExtensions{public static int Id(this ClaimsPrincipal u)=>int.Parse(u.FindFirstValue(ClaimTypes.NameIdentifier)!);}
