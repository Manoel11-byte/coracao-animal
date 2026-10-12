using System.Security.Cryptography;
using CoracaoAnimal.Core;
using CoracaoAnimal.Api.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace CoracaoAnimal.Api.Application;
public sealed class Contas(AppDb db,IPasswordHasher<Usuario> hasher){
 public static UsuarioDto Dto(Adotante a)=>new(a.Usuario.Id,a.Usuario.Nome,a.Usuario.Email,a.Usuario.Papel,a.Usuario.Cpf,a.Usuario.Telefone,a.Usuario.Endereco,a.EspeciePreferida,a.Espaco,a.Experiencia);
 public async Task<UsuarioDto> Perfil(int id)=>Dto(await db.Adotantes.AsNoTracking().Include(x=>x.Usuario).SingleAsync(x=>x.UsuarioId==id));
 public async Task Cadastrar(Cadastro i){Regras.Validar(i);Regras.Exigir(!string.IsNullOrWhiteSpace(i.Nome),"Informe seu nome.");var email=i.Email.Trim().ToLowerInvariant();Regras.Exigir(!await db.Usuarios.AnyAsync(x=>x.Email==email),"E-mail já cadastrado. Tente entrar na conta.",409);var u=new Usuario{Nome=i.Nome.Trim(),Email=email};u.SenhaHash=hasher.HashPassword(u,i.Senha);db.Adotantes.Add(new Adotante{Usuario=u});await db.SaveChangesAsync();}
 public async Task<SessaoDto> Entrar(Login i){Regras.Validar(i);Regras.Opcao(i.Cliente,"web","app");var email=i.Email.Trim().ToLowerInvariant();var u=await db.Usuarios.SingleOrDefaultAsync(x=>x.Email==email);
  if(u is null){hasher.HashPassword(new Usuario(),i.Senha);throw new RegraException("E-mail ou senha inválidos.",401);}
  Regras.Exigir(u.BloqueadoAte is null||u.BloqueadoAte<=DateTime.UtcNow,"Aguarde 15 minutos para tentar novamente.",429);
  var check=hasher.VerifyHashedPassword(u,u.SenhaHash,i.Senha);if(check==PasswordVerificationResult.Failed){u.Tentativas++;if(u.Tentativas>=5){u.BloqueadoAte=DateTime.UtcNow.AddMinutes(15);u.Tentativas=0;}await db.SaveChangesAsync();throw new RegraException("E-mail ou senha inválidos.",401);}
  u.Tentativas=0;u.BloqueadoAte=null;if(check==PasswordVerificationResult.SuccessRehashNeeded)u.SenhaHash=hasher.HashPassword(u,i.Senha);
  var token=Convert.ToHexString(RandomNumberGenerator.GetBytes(32));var end=DateTime.UtcNow.AddHours(i.Cliente=="web"?8:168);
  db.Sessoes.Add(new Sessao{UsuarioId=u.Id,Hash=SessionAuth.Hash(token),Cliente=i.Cliente,ExpiraEm=end});await db.Sessoes.Where(x=>x.ExpiraEm<DateTime.UtcNow).ExecuteDeleteAsync();await db.SaveChangesAsync();return new(await Perfil(u.Id),token,end);
 }
 public async Task<UsuarioDto> Atualizar(int id,PerfilInput i){Regras.Validar(i);Regras.Opcao(i.EspeciePreferida,"cao","gato");Regras.Opcao(i.Espaco,"pequeno","medio","grande");Regras.Opcao(i.Experiencia,"nenhuma","alguma","muita");Regras.Exigir(!string.IsNullOrWhiteSpace(i.Nome),"Informe o nome.");var cpf=Regras.Cpf(i.Cpf);Regras.Exigir(cpf is null||!await db.Usuarios.AnyAsync(x=>x.Cpf==cpf&&x.Id!=id),"CPF já cadastrado.",409);var a=await db.Adotantes.Include(x=>x.Usuario).SingleAsync(x=>x.UsuarioId==id);a.Usuario.Nome=i.Nome.Trim();a.Usuario.Cpf=cpf;a.Usuario.Telefone=Regras.Limpar(i.Telefone);a.Usuario.Endereco=Regras.Limpar(i.Endereco);a.EspeciePreferida=i.EspeciePreferida;a.Espaco=i.Espaco;a.Experiencia=i.Experiencia;await db.SaveChangesAsync();return Dto(a);}
 public async Task Senha(int id,SenhaInput i){Regras.Validar(i);var u=await db.Usuarios.SingleAsync(x=>x.Id==id);Regras.Exigir(hasher.VerifyHashedPassword(u,u.SenhaHash,i.Atual)!=PasswordVerificationResult.Failed,"Senha atual inválida.");await using var tx=await db.Database.BeginTransactionAsync();u.SenhaHash=hasher.HashPassword(u,i.Nova);await db.Sessoes.Where(x=>x.UsuarioId==id).ExecuteDeleteAsync();await db.SaveChangesAsync();await tx.CommitAsync();}
}
