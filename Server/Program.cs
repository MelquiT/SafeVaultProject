using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Cargar configuración desde appsettings.json
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings.GetValue<string>("Key");
var issuer = jwtSettings.GetValue<string>("Issuer");
var audience = jwtSettings.GetValue<string>("Audience");

// Validar que la clave exista para evitar errores silenciosos
if (string.IsNullOrEmpty(secretKey))
    throw new Exception("JWT Key no está configurada en appsettings.json");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),

            // MEJORA: Eliminar el tiempo de gracia de 5 minutos. 
            // Si el token expira a las 10:00:00, a las 10:00:01 ya no sirve.
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

//builder.Services.AddAuthorizationBuilder();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder
            .WithOrigins("https://localhost:7011", "http://localhost:5038") // <--- AÑADE TUS ORÍGENES BLazor AQUÍ
            .AllowAnyMethod() // Permite GET, POST, OPTIONS, etc.
            .AllowAnyHeader() // Permite encabezados personalizados (incluyendo el 'Authorization' para el JWT)
            .AllowCredentials()); // Es necesario para cookies, pero es buena práctica incluirlo.
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("CorsPolicy");

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
