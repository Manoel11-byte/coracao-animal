using System.Data;
using CoracaoAnimal.Core;
using CoracaoAnimal.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
namespace CoracaoAnimal.Api.Application;
public sealed class Adocoes(AppDb db){
 public static AdocaoDto Dto(Adocao a,bool admin=false)=>new(a.Id,a.AnimalId,a.Animal.Nome,a.Animal.FotoUrl,a.Status,a.Observacoes,a.CriadoEm,a.AtualizadoEm,admin?a.Adotante.Usuario.Nome:null);
 public async Task<AdocaoDto> Criar(int id,AdocaoInput i){Regras.Validar(i);Regras.Exigir(i.AceitaTermos,"Aceite o compromisso de adoção responsável.");await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);var a=await db.Adotantes.Include(x=>x.Usuario).SingleAsync(x=>x.UsuarioId==id);
  Regras.Exigir(a.Usuario.Cpf is not null&&!string.IsNullOrWhiteSpace(a.Usuario.Telefone)&&!string.IsNullOrWhiteSpace(a.Usuario.Endereco),"Complete CPF, telefone e endereço no perfil.");
  var animal=await db.Animais.SingleOrDefaultAsync(x=>x.Id==i.AnimalId&&!x.Arquivado)??throw new RegraException("Animal não encontrado.",404);Regras.Exigir(animal.Status=="disponivel"&&!await db.Adocoes.AnyAsync(x=>x.AnimalId==animal.Id&&x.Ativa),"Animal não está disponível.",409);
  var pedido=new Adocao{Animal=animal,Adotante=a,Observacoes=Regras.Limpar(i.Observacoes)};animal.Status="em_processo";db.Adocoes.Add(pedido);await db.SaveChangesAsync();await tx.CommitAsync();return Dto(pedido);
 }
 public async Task Alterar(int id,string status,int user,bool admin){await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);var a=await db.Adocoes.Include(x=>x.Animal).Include(x=>x.Adotante).SingleOrDefaultAsync(x=>x.Id==id)??throw new RegraException("Adoção não encontrada.",404);Regras.Exigir(admin||a.Adotante.UsuarioId==user,"Adoção não encontrada.",404);Regras.Exigir(admin||status=="cancelada","Ação não permitida.",403);Regras.Exigir(Regras.Transicao(a.Status,status),"Alteração de status não permitida.",409);a.Status=status;a.Ativa=status is "pendente" or "aprovada";a.AtualizadoEm=DateTime.UtcNow;a.Animal.Status=Regras.StatusAnimal(status);db.Auditorias.Add(new Auditoria{UsuarioId=user,Acao="adoção: "+status,Entidade="Adocoes",RegistroId=id});await db.SaveChangesAsync();await tx.CommitAsync();}
}
