using Microsoft.EntityFrameworkCore;
using FocusFlowAPI.Models;
using FocusFlowAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DotNetEnv;


var builder = WebApplication.CreateBuilder(args);
DotNetEnv.Env.Load();
// Primero carga appsettings y variables de entorno
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Ahora sí puedes leerlas
var jwtKey      = builder.Configuration["Jwt:Key"];
var jwtIssuer   = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "authenticated";
var dbConn      = builder.Configuration.GetConnectionString("SupabaseDb");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

builder.Services.AddDbContext<UsuarioContext>(options =>
    options.UseNpgsql(dbConn, npgsqlOptions => npgsqlOptions.CommandTimeout(180))
);

if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("La clave JWT no está configurada en variables de entorno");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddScoped<TareaService>();
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
