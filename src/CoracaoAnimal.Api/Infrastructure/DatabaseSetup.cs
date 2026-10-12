using CoracaoAnimal.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace CoracaoAnimal.Api.Infrastructure;
public static class DatabaseSetup {
 public static async Task Initialize(IServiceProvider services,IConfiguration config){await using var scope=services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<AppDb>();
  if(config.GetValue<bool>("Database:Initialize")){
   if(db.Database.IsSqlite())await db.Database.EnsureCreatedAsync();
   else{var conn=config["Database:AdminConnection"];await using var setup=new AppDb(new DbContextOptionsBuilder<AppDb>().UseSqlServer(string.IsNullOrEmpty(conn)?config.GetConnectionString("CoracaoAnimal"):conn).Options);await setup.Database.OpenConnectionAsync();try{
    await setup.Database.ExecuteSqlRawAsync("DECLARE @r int; EXEC @r=sp_getapplock @Resource=N'CoracaoAnimalSchema',@LockMode='Exclusive',@LockOwner='Session',@LockTimeout=60000; IF @r<0 THROW 51000,'Falha no bloqueio de inicialização.',1;");
    var sql=await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory,"database","schema.sql"));foreach(var batch in System.Text.RegularExpressions.Regex.Split(sql,@"^\s*GO\s*$",System.Text.RegularExpressions.RegexOptions.Multiline|System.Text.RegularExpressions.RegexOptions.IgnoreCase))if(!string.IsNullOrWhiteSpace(batch))await setup.Database.ExecuteSqlRawAsync(batch);
    if(!string.IsNullOrEmpty(conn)){var cs=new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(config.GetConnectionString("CoracaoAnimal"));Regras.Exigir(cs.UserID=="ca_runtime","Usuário runtime deve ser ca_runtime.");await setup.Database.ExecuteSqlRawAsync("IF NOT EXISTS(SELECT 1 FROM sys.database_principals WHERE name=N'ca_runtime') BEGIN DECLARE @cmd nvarchar(max)=N'CREATE USER [ca_runtime] WITH PASSWORD = '+QUOTENAME(@pwd,CHAR(39)); EXEC sp_executesql @cmd; END; ALTER ROLE db_datareader ADD MEMBER [ca_runtime]; ALTER ROLE db_datawriter ADD MEMBER [ca_runtime]; GRANT EXECUTE ON dbo.usp_ResumoMensal TO [ca_runtime]; GRANT EXECUTE ON dbo.usp_LimparSessoesExpiradas TO [ca_runtime];",new Microsoft.Data.SqlClient.SqlParameter("@pwd",cs.Password));}
   }finally{await setup.Database.CloseConnectionAsync();}}
  }
  var email=config["Bootstrap:Email"]?.Trim().ToLowerInvariant();var password=config["Bootstrap:Password"];
  if(!string.IsNullOrEmpty(email)&&!string.IsNullOrEmpty(password)&&!await db.Usuarios.AnyAsync(x=>x.Papel=="admin")){Regras.Validar(new Cadastro("Administrador",email,password));Regras.Exigir(!await db.Usuarios.AnyAsync(x=>x.Email==email),"E-mail de inicialização já está cadastrado.");var u=new Usuario{Nome="Administrador",Email=email,Papel="admin"};u.SenhaHash=scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>().HashPassword(u,password);db.Adotantes.Add(new Adotante{Usuario=u});await db.SaveChangesAsync();}
 }
}
