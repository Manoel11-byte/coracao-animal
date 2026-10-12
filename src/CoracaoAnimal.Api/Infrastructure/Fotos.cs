using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CoracaoAnimal.Core;
namespace CoracaoAnimal.Api.Infrastructure;
public sealed class Fotos(IConfiguration config,IWebHostEnvironment env){
 public async Task<string> Salvar(IFormFile file,CancellationToken ct){Regras.Exigir(file.Length>0&&file.Length<=5*1024*1024,"A foto deve ter até 5 MB.");using var data=new MemoryStream();await file.CopyToAsync(data,ct);var b=data.ToArray();string? ext=null;
  if(b.Length>=3&&b[0]==255&&b[1]==216&&b[2]==255)ext="jpg";
  else if(b.Length>=8&&b.Take(8).SequenceEqual(new byte[]{137,80,78,71,13,10,26,10}))ext="png";
  else if(b.Length>=12&&System.Text.Encoding.ASCII.GetString(b,0,4)=="RIFF"&&System.Text.Encoding.ASCII.GetString(b,8,4)=="WEBP")ext="webp";
  Regras.Exigir(ext is not null,"Envie uma imagem JPG, PNG ou WEBP válida.");var name=Guid.NewGuid().ToString("N")+"."+ext;var connection=config["Storage:ConnectionString"];
  if(!string.IsNullOrEmpty(connection)){var c=new BlobContainerClient(connection,config["Storage:Container"]??"fotos");await c.CreateIfNotExistsAsync(PublicAccessType.None,cancellationToken:ct);data.Position=0;await c.GetBlobClient(name).UploadAsync(data,new BlobUploadOptions{HttpHeaders=new BlobHttpHeaders{ContentType=Mime(name)}},ct);}
  else{Regras.Exigir(env.IsDevelopment()||env.IsEnvironment("Testing"),"Armazenamento não configurado.",503);Directory.CreateDirectory(Local);await File.WriteAllBytesAsync(Path.Combine(Local,name),b,ct);}return "/api/fotos/"+name;
 }
 string Local=>Path.Combine(env.ContentRootPath,"App_Data","fotos");
 static string Mime(string n)=>Path.GetExtension(n) switch{".jpg"=>"image/jpeg",".png"=>"image/png",_=>"image/webp"};
 public async Task<IResult> Ler(string name,CancellationToken ct){if(!System.Text.RegularExpressions.Regex.IsMatch(name,@"^[a-f0-9]{32}\.(jpg|png|webp)$"))return Results.NotFound();var conn=config["Storage:ConnectionString"];if(!string.IsNullOrEmpty(conn)){var b=new BlobContainerClient(conn,config["Storage:Container"]??"fotos").GetBlobClient(name);if(!(await b.ExistsAsync(ct)).Value)return Results.NotFound();return Results.Stream((await b.DownloadStreamingAsync(cancellationToken:ct)).Value.Content,Mime(name));}var p=Path.Combine(Local,name);return File.Exists(p)?Results.File(p,Mime(name)):Results.NotFound();}
}
