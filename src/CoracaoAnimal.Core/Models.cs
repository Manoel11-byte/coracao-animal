namespace CoracaoAnimal.Core;
public sealed class Usuario {
 public int Id {get;set;} public string Nome {get;set;}=""; public string Email {get;set;}="";
 public string SenhaHash {get;set;}=""; public string Papel {get;set;}="usuario";
 public string? Cpf {get;set;} public string Telefone {get;set;}=""; public string Endereco {get;set;}="";
 public int Tentativas {get;set;} public DateTime? BloqueadoAte {get;set;}
 public DateTime CriadoEm {get;set;}=DateTime.UtcNow;
}
public sealed class Sessao {
 public string Hash {get;set;}=""; public int UsuarioId {get;set;} public Usuario Usuario {get;set;}=null!;
 public string Cliente {get;set;}="web"; public DateTime ExpiraEm {get;set;}
}
public sealed class Adotante {
 public int Id {get;set;} public int UsuarioId {get;set;} public Usuario Usuario {get;set;}=null!;
 public string EspeciePreferida {get;set;}="cao"; public string Espaco {get;set;}="medio"; public string Experiencia {get;set;}="alguma";
}
public sealed class Animal {
 public int Id {get;set;} public string Nome {get;set;}=""; public string Especie {get;set;}="cao";
 public string Raca {get;set;}="SRD"; public int Idade {get;set;} public string Porte {get;set;}="medio";
 public string Status {get;set;}="disponivel"; public string Descricao {get;set;}=""; public string? FotoUrl {get;set;}
 public bool Arquivado {get;set;} public DateTime CriadoEm {get;set;}=DateTime.UtcNow;
}
public sealed class Adocao {
 public int Id {get;set;} public int AnimalId {get;set;} public Animal Animal {get;set;}=null!;
 public int AdotanteId {get;set;} public Adotante Adotante {get;set;}=null!;
 public string Status {get;set;}="pendente"; public bool Ativa {get;set;}=true; public string Observacoes {get;set;}="";
 public DateTime CriadoEm {get;set;}=DateTime.UtcNow; public DateTime AtualizadoEm {get;set;}=DateTime.UtcNow;
}
public sealed class Doacao {
 public int Id {get;set;} public int AdotanteId {get;set;} public Adotante Adotante {get;set;}=null!;
 public decimal Valor {get;set;} public string FormaPagamento {get;set;}="pix"; public string Status {get;set;}="pendente";
 public string Descricao {get;set;}=""; public bool Anonima {get;set;} public DateTime CriadoEm {get;set;}=DateTime.UtcNow;
}
public sealed class Voluntario {
 public int Id {get;set;} public int UsuarioId {get;set;} public Usuario Usuario {get;set;}=null!;
 public string Area {get;set;}=""; public string Disponibilidade {get;set;}=""; public string Status {get;set;}="pendente";
 public DateTime CriadoEm {get;set;}=DateTime.UtcNow;
}
public sealed class Auditoria {
 public long Id {get;set;} public int? UsuarioId {get;set;} public string Acao {get;set;}="";
 public string Entidade {get;set;}=""; public int RegistroId {get;set;} public DateTime CriadoEm {get;set;}=DateTime.UtcNow;
}
