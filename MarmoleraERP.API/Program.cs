using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MarmoleraERP.API.Data;
using MarmoleraERP.API.Modules.Identity.Entities;
using MarmoleraERP.API.Modules.Notificaciones.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── Base de datos (MySQL / Pomelo) ─────────────────────────────────────────
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseMySql(connStr, ServerVersion.AutoDetect(connStr)));

// ─── Identity ──────────────────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit           = false;
    options.Password.RequiredLength         = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase       = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ─── JWT ─────────────────────────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(options =>
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
        IssuerSigningKey         = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ─── Notificaciones (Scoped: una instancia por request HTTP) ───────────────────
builder.Services.AddScoped<INotificacionService, NotificacionService>();

// ─── Controllers + Swagger ──────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MarmoleraERP API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Ingresa el token JWT. Ejemplo: eyJhbGci..."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ─── CORS (restringir en producción) ─────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// ─── Aplicar migraciones + Seed de usuarios ──────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db          = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    db.Database.Migrate();

    // Roles definidos en el sistema
    string[] roles = ["Admin", "Ventas", "Produccion", "Contabilidad", "Tablet"];
    foreach (var rol in roles)
        if (!await roleManager.RoleExistsAsync(rol))
            await roleManager.CreateAsync(new IdentityRole(rol));

    // ┌──────────────────────────────────────────────────────────────────────────┐
    // │  Tabla de usuarios semilla                                               │
    // │  Formato: (email, password, nombre, roles[])                             │
    // │  Un usuario puede tener MÚLTIPLES roles (ej: Juliana → Ventas + Tablet)  │
    // └──────────────────────────────────────────────────────────────────────────┘
    var seedUsers = new (string Email, string Password, string Nombre, string[] Roles)[]
    {
        // ── Gerencia / Admin ──────────────────────────────────────────────────
        ("julio@marmolera.com",    "julio123",   "Julio",   ["Admin"]),
        ("cesar@marmolera.com",    "cesar123",   "Cesar",   ["Admin"]),

        // ── Ventas ────────────────────────────────────────────────────────────
        // Juliana y Ana y Mari también tienen acceso a Tablet (multi-rol)
        ("juliana@marmolera.com",  "juliana123", "Juliana", ["Ventas", "Tablet"]),
        ("ana@marmolera.com",      "ana123",     "Ana",     ["Ventas", "Tablet"]),
        ("mari@marmolera.com",     "mari123",    "Mari",    ["Ventas", "Tablet"]),

        // ── Fábrica / Producción ──────────────────────────────────────────────
        ("javier@marmolera.com",   "javier123",  "Javier",  ["Produccion"]),
        ("marco@marmolera.com",    "marco123",   "Marco",   ["Produccion"]),
        // Julio ya está creado arriba como Admin, no se duplica

        // ── Finanzas / Contabilidad ───────────────────────────────────────────
        ("sheila@marmolera.com",   "sheila123",  "Sheila",  ["Contabilidad"]),
    };

    foreach (var (email, password, nombre, userRoles) in seedUsers)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
            continue; // ya existe, no tocar

        var user = new ApplicationUser
        {
            UserName      = email,
            Email         = email,
            Nombre        = nombre,
            Activo        = true,
            FechaCreacion = DateTime.UtcNow,
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
            foreach (var rol in userRoles)
                await userManager.AddToRoleAsync(user, rol);
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
