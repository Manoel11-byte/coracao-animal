using System.Threading.RateLimiting;
using CoracaoAnimal.Api;
using CoracaoAnimal.Api.Infrastructure;
using CoracaoAnimal.Api.Application;
using CoracaoAnimal.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
var builder=WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDb>(o=>{var c=builder.Configuration.GetConnectionString("CoracaoAnimal");if(builder.Configuration["Database:Provider"]=="Sqlite"){if(!builder.Environment.IsDevelopment()&&!builder.Environment.IsEnvironment("Testing"))throw new InvalidOperationException("SQLite somente para desenvolvimento e teste.");o.UseSqlite(c);}else o.UseSqlServer(c);});
builder.Services.AddAuthentication("Session").AddScheme<AuthenticationSchemeOptions,SessionAuth>("Session",null);
builder.Services.AddAuthorization(o=>o.AddPolicy("AdminWeb",p=>p.RequireAuthenticatedUser().RequireRole("admin").RequireClaim("cliente","web")));
builder.Services.AddScoped<IPasswordHasher<Usuario>,PasswordHasher<Usuario>>();builder.Services.Configure<PasswordHasherOptions>(o=>o.IterationCount=210000);
builder.Services.AddScoped<Contas>();builder.Services.AddScoped<Adocoes>();builder.Services.AddScoped<Fotos>();builder.Services.AddProblemDetails();
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o=>o.MultipartBodyLengthLimit=6*1024*1024);builder.WebHost.ConfigureKestrel(o=>o.Limits.MaxRequestBodySize=6*1024*1024);
builder.Services.AddRateLimiter(o=>{o.RejectionStatusCode=429;o.AddPolicy("auth",c=>RateLimitPartition.GetFixedWindowLimiter(c.Connection.RemoteIpAddress?.ToString()??"unknown",_=>new FixedWindowRateLimiterOptions{PermitLimit=30,Window=TimeSpan.FromMinutes(1),QueueLimit=0}));});
var app=builder.Build();
app.UseExceptionHandler(error=>error.Run(async c=>{var ex=c.Features.Get<IExceptionHandlerFeature>()?.Error;var status=ex is RegraException r?r.Status:ex is DbUpdateException?409:ex is BadHttpRequestException?400:500;c.Response.StatusCode=status;await c.Response.WriteAsJsonAsync(new{mensagem=ex is RegraException regra?regra.Message:status==409?"Conflito de dados. Atualize e tente novamente.":status==400?"Dados inválidos.":"Erro no serviço. Tente novamente.",traceId=c.TraceIdentifier});}));
if(!app.Environment.IsDevelopment()&&!app.Environment.IsEnvironment("Testing")){app.UseHsts();app.UseHttpsRedirection();}
app.Use(async(c,next)=>{c.Response.Headers["X-Content-Type-Options"]="nosniff";c.Response.Headers["Referrer-Policy"]="same-origin";c.Response.Headers["X-Frame-Options"]="DENY";c.Response.Headers["Content-Security-Policy"]="default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; font-src 'self' https://fonts.gstatic.com; img-src 'self' data:; connect-src 'self'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";
 if(c.Request.Path.StartsWithSegments("/api")&&!c.Request.Path.StartsWithSegments("/api/fotos"))c.Response.Headers.CacheControl="no-store";
 if(c.Request.Path.StartsWithSegments("/api")&&c.Request.Method is not ("GET" or "HEAD" or "OPTIONS")){var origin=c.Request.Headers.Origin.ToString();if((c.Request.Headers["X-CSRF"]!="1"&&!c.Request.Headers.Authorization.ToString().StartsWith("Bearer ",StringComparison.OrdinalIgnoreCase))||(origin.Length>0&&origin!=$"{c.Request.Scheme}://{c.Request.Host}")){c.Response.StatusCode=403;await c.Response.WriteAsJsonAsync(new{mensagem="Origem ou solicitação não permitida."});return;}}await next();});
app.UseDefaultFiles();app.UseStaticFiles();app.UseRouting();app.UseRateLimiter();app.UseAuthentication();app.UseAuthorization();
app.MapGet("/health/live",()=>Results.Ok(new{status="ok"}));app.MapGet("/health/ready",async(AppDb db)=>await db.Database.CanConnectAsync()?Results.Ok(new{status="ok"}):Results.StatusCode(503));app.MapApi();
app.MapFallback(async c=>{if(c.Request.Path.StartsWithSegments("/api")){c.Response.StatusCode=404;return;}c.Response.ContentType="text/html";await c.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath,"index.html"));});
await DatabaseSetup.Initialize(app.Services,app.Configuration);app.Run();
public partial class Program{}
