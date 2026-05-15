using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MarmoleraERP.API.Data;
using MarmoleraERP.API.Modules.Identity.Entities;

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════════════════════
//  1. BASE DE DATOS — Entity Framework Core + MySQL 8.0 (Pomelo)
// ═══════════════════════════════════════════════════════════════════════════
var mysqlVersion = new MySqlServerVersion(new Version(8, 0, 21));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        mysqlVersion,
        mysqlOptions => mysqlOptions
            .EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null)
            .MigrationsHistoryTable("__EFMigrationsHistory")));

// ═══════════════════════════════════════════════════════════════════════════
//  2. ASP.NET CORE IDENTITY
// ═══════════════════════════════════════════════════════════════════════════
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit           = true;
        options.Password.RequireLowercase       = true;
        options.Password.RequireUppercase       = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength         = 8;
        options.User.RequireUniqueEmail         = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ═══════════════════════════════════════════════════════════════════════════
//  3. AUTENTICACIÓN JWT
// ═══════════════════════════════════════════════════════════════════════════
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey   = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("JwtSettings:SecretKey no configurado.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtSettings["Issuer"],
            ValidAudience            = jwtSettings["Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew                = TimeSpan.Zero  // Sin margen extra de expiración
        };
    });

// ═══════════════════════════════════════════════════════════════════════════
//  4. AUTORIZACIÓN (politica por defecto: usuario autenticado)
// ═══════════════════════════════════════════════════════════════════════════
builder.Services.AddAuthorization();

// ═══════════════════════════════════════════════════════════════════════════
//  5. CONTROLLERS + JSON (enums como strings en la API)
// ═══════════════════════════════════════════════════════════════════════════
builder.Services
    .AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));

// ═══════════════════════════════════════════════════════════════════════════
//  6. SWAGGER con soporte JWT Bearer
// ═══════════════════════════════════════════════════════════════════════════
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Marmolera Claros ERP API",
        Version     = "v1",
        Description = "API del sistema ERP interno para gestión de pedidos, catálogo y producción."
    });

    // Esquema de seguridad JWT para Swagger UI
    // Tipo ApiKey: el usuario escribe el valor completo del header, ej: "Bearer eyJhbGci..."
    var jwtScheme = new OpenApiSecurityScheme
    {
        Name        = "Authorization",
        Type        = SecuritySchemeType.ApiKey,
        In          = ParameterLocation.Header,
        Description = "Ingresa el token con el prefijo Bearer. Ejemplo: **Bearer eyJhbGci...**"
    };
    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ═══════════════════════════════════════════════════════════════════════════
//  7. CORS (ajustar origenes en producción)
// ═══════════════════════════════════════════════════════════════════════════
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ═══════════════════════════════════════════════════════════════════════════
//  BUILD
// ═══════════════════════════════════════════════════════════════════════════
var app = builder.Build();

// ─── Middleware Pipeline ──────────────────────────────────────────────────
// Swagger siempre activo para facilitar el desarrollo local.
// IMPORTANTE: En producción real, volver a condicionar con IsDevelopment()
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Marmolera Claros ERP v1");
    c.RoutePrefix = "swagger"; // Acceso en http://localhost:5183/swagger
});

//app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication(); // ← Siempre antes de UseAuthorization
app.UseAuthorization();

app.MapControllers();

// ─── Aplicar migraciones automáticamente al iniciar ─────────────────────
try
{
    using var scope = app.Services.CreateScope();
    var dbCtx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbCtx.Database.MigrateAsync();
    Console.WriteLine("✅ Migraciones aplicadas correctamente.");
}
catch (Exception ex)
{
    // El error se imprime en consola y la app sigue corriendo.
    // Swagger seguirá visible para depuración aunque la BD falle.
    Console.WriteLine($"❌ Error al aplicar migraciones: {ex.Message}");
}

app.Run();
