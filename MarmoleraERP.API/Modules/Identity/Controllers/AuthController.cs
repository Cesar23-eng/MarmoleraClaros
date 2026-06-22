using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MarmoleraERP.API.Modules.Identity.DTOs;
using MarmoleraERP.API.Modules.Identity.Entities;

namespace MarmoleraERP.API.Modules.Identity.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<ApplicationUser>   userManager,
    SignInManager<ApplicationUser> signInManager,
    IConfiguration config) : ControllerBase
{
    // ════════════════════════════════════════════════════════════════════════
    //  POST /api/auth/login
    //  Devuelve accessToken (JWT, corta vida) + refreshToken (opaco, larga vida)
    // ════════════════════════════════════════════════════════════════════════
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user is null || !user.Activo)
            return Unauthorized(new { mensaje = "Credenciales inválidas o usuario inactivo." });

        var result = await signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
            return Unauthorized(new { mensaje = "Credenciales inválidas." });

        var roles        = await userManager.GetRolesAsync(user);
        var accessToken  = GenerarAccessToken(user, roles);
        var refreshToken = await GuardarRefreshTokenAsync(user);

        return Ok(new LoginResponseDto(accessToken, refreshToken, user.Id, user.Email!, user.Nombre, roles.ToList()));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  POST /api/auth/refresh
    //  Valida el refresh token y devuelve un nuevo par de tokens (rotación)
    // ════════════════════════════════════════════════════════════════════════
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto dto)
    {
        // Buscar usuario por el refresh token recibido
        var user = userManager.Users.SingleOrDefault(u => u.RefreshToken == dto.RefreshToken);

        if (user is null || !user.Activo)
            return Unauthorized(new { mensaje = "Refresh token inválido." });

        if (user.RefreshTokenExpiry is null || user.RefreshTokenExpiry < DateTime.UtcNow)
            return Unauthorized(new { mensaje = "Refresh token expirado. Vuelve a iniciar sesión." });

        // Token válido → rotar: generar un par nuevo y anular el anterior
        var roles           = await userManager.GetRolesAsync(user);
        var nuevoAccess     = GenerarAccessToken(user, roles);
        var nuevoRefresh    = await GuardarRefreshTokenAsync(user);

        return Ok(new LoginResponseDto(nuevoAccess, nuevoRefresh, user.Id, user.Email!, user.Nombre, roles.ToList()));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  POST /api/auth/logout
    //  Revoca el refresh token del usuario autenticado
    // ════════════════════════════════════════════════════════════════════════
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user   = await userManager.FindByIdAsync(userId!);

        if (user is not null)
        {
            user.RefreshToken       = null;
            user.RefreshTokenExpiry = null;
            await userManager.UpdateAsync(user);
        }

        return NoContent();
    }

    // ════════════════════════════════════════════════════════════════════════
    //  POST /api/auth/register  (solo Admin en producción, abierto en dev)
    // ════════════════════════════════════════════════════════════════════════
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (await userManager.FindByEmailAsync(dto.Email) is not null)
            return BadRequest(new { mensaje = "Ya existe un usuario con ese email." });

        var user = new ApplicationUser
        {
            UserName      = dto.Email,
            Email         = dto.Email,
            Nombre        = dto.Nombre,
            Activo        = true,
            FechaCreacion = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(new { errores = result.Errors.Select(e => e.Description) });

        if (!string.IsNullOrWhiteSpace(dto.Rol))
            await userManager.AddToRoleAsync(user, dto.Rol);

        return StatusCode(201, new { mensaje = "Usuario creado correctamente.", userId = user.Id });
    }

    // ─── HELPERS ──────────────────────────────────────────────────────────────────

    /// <summary>Genera un JWT de corta vida con todos los claims del usuario.</summary>
    private string GenerarAccessToken(ApplicationUser user, IList<string> roles)
    {
        var jwtSettings = config.GetSection("JwtSettings");
        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        var creds       = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email,          user.Email!),
            new("nombre",                  user.Nombre),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer:             jwtSettings["Issuer"],
            audience:           jwtSettings["Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiresInMinutes"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Genera un refresh token criptográficamente seguro, lo persiste en el usuario
    /// con su fecha de expiración y devuelve el valor en texto plano.
    /// </summary>
    private async Task<string> GuardarRefreshTokenAsync(ApplicationUser user)
    {
        var jwtSettings     = config.GetSection("JwtSettings");
        var diasExpiracion  = int.Parse(jwtSettings["RefreshTokenDays"] ?? "30");

        // 32 bytes aleatorios → 43 chars Base64Url, sin padding
        var tokenBytes   = RandomNumberGenerator.GetBytes(32);
        var refreshToken = Convert.ToBase64String(tokenBytes)
                              .Replace("+", "-").Replace("/", "_").Replace("=", "");

        user.RefreshToken       = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(diasExpiracion);
        await userManager.UpdateAsync(user);

        return refreshToken;
    }
}
