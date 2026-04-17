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
    KeepAlive = 5,
    Timeout = 30,
    Pooling = true,
    ConnectionIdleLifetime = 60,
    ConnectionPruningInterval = 10
};
if (dbConnBuilder.ContainsKey("CommandTimeout"))
    dbConnBuilder.Remove("CommandTimeout");

// Aumentamos el timeout de comando para consultas que pueden tardar más de 5 segundos.
dbConnBuilder.CommandTimeout = 60;

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
        npgsqlOptions.CommandTimeout(60); // Timeout de comando a 60 segundos
        // Reintentos solo para errores transitorios de conexión, NO para timeouts de lectura
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(2),
            errorCodesToAdd: new[] { "57P01", "57P02", "57P03", "53300" }
        );
    })
);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine($"Auth failed: {ctx.Exception}");
                return Task.CompletedTask;
            },
            OnTokenValidated = ctx =>
            {
                Console.WriteLine("Token validado correctamente");
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
        await dbContext.Database.OpenConnectionAsync();
        await dbContext.Database.CloseConnectionAsync();
    }
    catch { }
    NpgsqlConnection.ClearAllPools();
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