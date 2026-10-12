using CoracaoAnimal.Core;
using Microsoft.EntityFrameworkCore;
namespace CoracaoAnimal.Api.Infrastructure;
public sealed class AppDb(DbContextOptions<AppDb> options):DbContext(options){
 public DbSet<Usuario> Usuarios=>Set<Usuario>();public DbSet<Sessao> Sessoes=>Set<Sessao>();
 public DbSet<Adotante> Adotantes=>Set<Adotante>();public DbSet<Animal> Animais=>Set<Animal>();
 public DbSet<Adocao> Adocoes=>Set<Adocao>();public DbSet<Doacao> Doacoes=>Set<Doacao>();
 public DbSet<Voluntario> Voluntarios=>Set<Voluntario>();public DbSet<Auditoria> Auditorias=>Set<Auditoria>();
 protected override void OnModelCreating(ModelBuilder b){
  b.Entity<Usuario>().Property(x=>x.Email).HasMaxLength(254);b.Entity<Usuario>().HasIndex(x=>x.Email).IsUnique();
  b.Entity<Usuario>().Property(x=>x.Cpf).HasMaxLength(11);b.Entity<Usuario>().HasIndex(x=>x.Cpf).IsUnique().HasFilter("[Cpf] IS NOT NULL");
  b.Entity<Sessao>().HasKey(x=>x.Hash);b.Entity<Sessao>().Property(x=>x.Hash).HasMaxLength(64);b.Entity<Sessao>().HasIndex(x=>x.ExpiraEm);
  b.Entity<Adotante>().HasIndex(x=>x.UsuarioId).IsUnique();b.Entity<Voluntario>().HasIndex(x=>x.UsuarioId).IsUnique();
  b.Entity<Adocao>().HasIndex(x=>x.AnimalId).IsUnique().HasFilter("[Ativa] = 1");
  b.Entity<Adocao>().HasIndex(x=>new{x.AdotanteId,x.CriadoEm});b.Entity<Doacao>().HasIndex(x=>new{x.AdotanteId,x.CriadoEm});
  b.Entity<Doacao>().Property(x=>x.Valor).HasPrecision(10,2);
  b.Entity<Animal>().Property(x=>x.Status).HasMaxLength(20);b.Entity<Animal>().Property(x=>x.Especie).HasMaxLength(10);b.Entity<Animal>().Property(x=>x.Porte).HasMaxLength(10);
  b.Entity<Animal>().HasIndex(x=>new{x.Arquivado,x.Status,x.Especie,x.Porte});
  b.Entity<Adocao>().ToTable("Adocoes",t=>t.UseSqlOutputClause(false));
  foreach(var fk in b.Model.GetEntityTypes().SelectMany(e=>e.GetForeignKeys()))fk.DeleteBehavior=DeleteBehavior.Restrict;
 }
}
