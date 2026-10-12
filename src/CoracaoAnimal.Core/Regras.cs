using System.ComponentModel.DataAnnotations;
namespace CoracaoAnimal.Core;
public sealed class RegraException(string mensagem,int status=400):Exception(mensagem){public int Status {get;}=status;}
public static class Regras {
 public static void Exigir(bool condicao,string mensagem,int status=400){if(!condicao)throw new RegraException(mensagem,status);}
 public static void Validar(object valor){var erros=new List<ValidationResult>();if(!Validator.TryValidateObject(valor,new ValidationContext(valor),erros,true))throw new RegraException(string.Join(" ",erros.Select(x=>x.ErrorMessage)));}
 public static void Opcao(string valor,params string[] opcoes)=>Exigir(opcoes.Contains(valor),"Opção inválida.");
 public static string Limpar(string? valor)=>valor?.Trim()??"";
 public static string? Cpf(string? valor){if(string.IsNullOrWhiteSpace(valor))return null;var d=new string(valor.Where(char.IsAsciiDigit).ToArray());Exigir(d.Length==11&&d.Distinct().Count()>1,"CPF inválido.");for(int n=9;n<=10;n++){var r=Enumerable.Range(0,n).Sum(i=>(d[i]-'0')*(n+1-i))*10%11;if(r==10)r=0;Exigir(r==d[n]-'0',"CPF inválido.");}return d;}
 public static bool Transicao(string atual,string novo)=>(atual,novo) switch{("pendente","aprovada" or "cancelada")=>true,("aprovada","concluida" or "cancelada")=>true,_=>false};
 public static string StatusAnimal(string status)=>status switch{"pendente" or "aprovada"=>"em_processo","concluida"=>"adotado","cancelada"=>"disponivel",_=>throw new RegraException("Status inválido.")};
 public static void Animal(AnimalInput a){Validar(a);Exigir(!string.IsNullOrWhiteSpace(a.Nome),"Informe o nome.");Opcao(a.Especie,"cao","gato");Opcao(a.Porte,"pequeno","medio","grande");Opcao(a.Status,"disponivel","em_tratamento","em_processo","adotado");}
 public static double Afinidade(Animal a,string especie,string porte,string experiencia){double[] Vetor(string e,string p,int idade)=>[e=="cao"?1:0,e=="gato"?1:0,p=="pequeno"?1:0,p=="medio"?1:0,p=="grande"?1:0,Math.Clamp(idade,0,15)/15d];var x=Vetor(a.Especie,a.Porte,a.Idade);var y=Vetor(especie,porte,experiencia switch{"nenhuma"=>2,"muita"=>7,_=>4});return Math.Round(100*x.Zip(y,(i,j)=>i*j).Sum()/Math.Sqrt(x.Sum(i=>i*i)*y.Sum(i=>i*i)),1);}
}
