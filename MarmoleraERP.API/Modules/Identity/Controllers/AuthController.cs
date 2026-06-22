using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MarmoleraERP.API.Modules.Identity.DTOs;
using MarmoleraERP.API.Modules.Identity.Entities;

namespace MarmoleraERP.API.Modules.Identity.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<ApplicationUser>  userManager,
    SignInManager<ApplicationUser> signInManager,
    IConfiguration config) : ControllerBase
{
    // ════════════════════════════════════════════════════════════════════════
    //  POST /api/auth/login
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

        var roles = await userManager.GetRolesAsync(user);
        var token = GenerarToken(user, roles);

        return Ok(new LoginResponseDto(token, user.Id, user.Email!, user.Nombre, roles.ToList()));
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

    // ─── Helper: genera JWT ─────────────────────────────────────────────────────────
    private string GenerarToken(ApplicationUser user, IList<string> roles)
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
}
