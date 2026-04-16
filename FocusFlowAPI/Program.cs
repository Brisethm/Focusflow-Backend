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

// Primero carga appsettings y variables de entorno
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Variables de entorno
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "authenticated";
var dbConn = builder.Configuration.GetConnectionString("SupabaseDb");
var dbConnBuilder = new NpgsqlConnectionStringBuilder(dbConn)
{
    Multiplexing = false
};
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
    options.UseNpgsql(dbConnBuilder.ConnectionString, npgsqlOptions => npgsqlOptions.CommandTimeout(180))
);

// Configuración de autenticación con Supabase (ES256 + JWKS)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
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
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            {
                return jwksCache.GetSigningKeys(kid);
            },
            // Esto asegura que ASP.NET use directamente el claim "sub" y "role"
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

var app = builder.Build();

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
