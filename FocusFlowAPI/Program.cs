using Microsoft.EntityFrameworkCore;
using FocusFlowAPI.Models;
using FocusFlowAPI.Security;
using FocusFlowAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using DotNetEnv;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
DotNetEnv.Env.Load();

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "authenticated";
var dbConn = builder.Configuration.GetConnectionString("SupabaseDb");

var dbConnBuilder = new NpgsqlConnectionStringBuilder(dbConn)
{
    Multiplexing = false,
    KeepAlive = 10,
    Timeout = 30,
    Pooling = true,
    ConnectionIdleLifetime = 60,
    ConnectionPruningInterval = 10
};
if (dbConnBuilder.ContainsKey("CommandTimeout"))
    dbConnBuilder.Remove("CommandTimeout");

var connectionString = dbConnBuilder.ConnectionString;

var jwksCache = new SupabaseJwksCache($"{jwtIssuer}/.well-known/jwks.json");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddSingleton(jwksCache);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

builder.Services.AddDbContext<UsuarioContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.CommandTimeout(15);
        // Sin reintentos — el Transaction Pooler de Supabase ya maneja reconexiones
    })
    .LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name },
           LogLevel.Information)
);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
{
    var logger = ctx.HttpContext.RequestServices
        .GetRequiredService<ILogger<Program>>();
    logger.LogWarning(ctx.Exception, "Auth failed");
    return Task.CompletedTask;
},
            OnTokenValidated = ctx =>
            {
                return Task.CompletedTask;
            }
        };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) => jwksCache.GetSigningKeys(kid),
            NameClaimType = "sub",
            RoleClaimType = "role"
        };
    });

builder.Services.AddScoped<TareaService>();
builder.Services.AddScoped<PerfilUsuarioService>();
builder.Services.AddScoped<RecordatorioService>();
builder.Services.AddScoped<SesionEnfoqueService>();
builder.Services.AddScoped<RegistroEmocionalService>();
builder.Services.AddScoped<TransaccionService>();
builder.Services.AddScoped<CuestionarioService>();
builder.Services.AddScoped<PlanPersonalizadoService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UsuarioContext>();
    try
    {
        await dbContext.Database.CanConnectAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "No se pudo conectar a la DB al iniciar");
    }
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FocusFlowAPI v1");
    c.RoutePrefix = string.Empty;
});

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
await app.RunAsync();