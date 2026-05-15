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
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _config;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration config)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _config = config;
    }

    /// <summary>
    /// Autentica al usuario y devuelve un JWT con los claims del rol.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized(new { message = "Credenciales incorrectas." });

        var userRoles = await _userManager.GetRolesAsync(user);
        var rol = userRoles.FirstOrDefault() ?? "Sin Rol";

        var token = GenerateJwt(user, userRoles);

        return Ok(new AuthResponseDto(
            Token: new JwtSecurityTokenHandler().WriteToken(token),
            Email: user.Email!,
            NombreCompleto: user.NombreCompleto,
            Rol: rol,
            Expiracion: token.ValidTo
        ));
    }

    /// <summary>
    /// Crea un nuevo usuario y le asigna un rol.
    /// Si el rol aún no existe en la BD, lo crea automáticamente.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        // 1. Verificar/crear el rol
        if (!await _roleManager.RoleExistsAsync(dto.Role))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(dto.Role));
            if (!roleResult.Succeeded)
                return BadRequest(new { errors = roleResult.Errors.Select(e => e.Description) });
        }

        // 2. Crear el usuario
        var user = new ApplicationUser
        {
            UserName       = dto.Email,
            Email          = dto.Email,
            NombreCompleto = dto.NombreCompleto
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
            return BadRequest(new { errors = createResult.Errors.Select(e => e.Description) });

        // 3. Asignar el rol
        var roleAssignResult = await _userManager.AddToRoleAsync(user, dto.Role);
        if (!roleAssignResult.Succeeded)
            return BadRequest(new { errors = roleAssignResult.Errors.Select(e => e.Description) });

        return Ok(new { message = $"Usuario '{dto.Email}' creado exitosamente con rol '{dto.Role}'." });
    }

    // ─── Helper privado ───────────────────────────────────────────────────────
    private JwtSecurityToken GenerateJwt(ApplicationUser user, IList<string> userRoles)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new(ClaimTypes.Name,               user.NombreCompleto),
        };

        // Inyectar cada rol como un claim independiente
        foreach (var userRole in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole));
        }

        return new JwtSecurityToken(
            issuer:   jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims:   claims,
            expires:  DateTime.UtcNow.AddHours(int.Parse(jwtSettings["ExpiresInHours"] ?? "8")),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );
    }
}
