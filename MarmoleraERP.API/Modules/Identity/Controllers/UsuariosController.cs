using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarmoleraERP.API.Modules.Identity.DTOs;
using MarmoleraERP.API.Modules.Identity.Entities;

namespace MarmoleraERP.API.Modules.Identity.Controllers;

/// <summary>
/// CRUD de usuarios. Solo accesible por Admin.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsuariosController(
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/usuarios
    // ════════════════════════════════════════════════════════════════════════
    [HttpGet]
    [ProducesResponseType(typeof(List<UsuarioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar()
    {
        var usuarios = await userManager.Users
            .OrderBy(u => u.Nombre)
            .ToListAsync();

        var result = new List<UsuarioDto>();
        foreach (var u in usuarios)
        {
            var roles = await userManager.GetRolesAsync(u);
            result.Add(new UsuarioDto(u.Id, u.Nombre, u.Email!, u.Activo, u.FechaCreacion, roles.ToList()));
        }

        return Ok(result);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/usuarios/{id}
    // ════════════════════════════════════════════════════════════════════════
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerPorId(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        var roles = await userManager.GetRolesAsync(user);
        return Ok(new UsuarioDto(user.Id, user.Nombre, user.Email!, user.Activo, user.FechaCreacion, roles.ToList()));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  POST /api/usuarios
    // ════════════════════════════════════════════════════════════════════════
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Crear([FromBody] CrearUsuarioDto dto)
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

        var roles = await userManager.GetRolesAsync(user);
        return StatusCode(201, new UsuarioDto(user.Id, user.Nombre, user.Email!, user.Activo, user.FechaCreacion, roles.ToList()));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  PUT /api/usuarios/{id}
    // ════════════════════════════════════════════════════════════════════════
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Editar(string id, [FromBody] EditarUsuarioDto dto)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        user.Nombre   = dto.Nombre;
        user.Email    = dto.Email;
        user.UserName = dto.Email;
        user.Activo   = dto.Activo;

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return BadRequest(new { errores = updateResult.Errors.Select(e => e.Description) });

        // Cambiar rol si se especificó uno diferente
        var rolesActuales = await userManager.GetRolesAsync(user);
        if (!string.IsNullOrWhiteSpace(dto.Rol) && !rolesActuales.Contains(dto.Rol))
        {
            await userManager.RemoveFromRolesAsync(user, rolesActuales);
            await userManager.AddToRoleAsync(user, dto.Rol);
        }

        var rolesNuevos = await userManager.GetRolesAsync(user);
        return Ok(new UsuarioDto(user.Id, user.Nombre, user.Email!, user.Activo, user.FechaCreacion, rolesNuevos.ToList()));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  PUT /api/usuarios/{id}/password
    // ════════════════════════════════════════════════════════════════════════
    [HttpPut("{id}/password")]
    public async Task<IActionResult> CambiarPassword(string id, [FromBody] CambiarPasswordDto dto)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        // El Admin puede resetear la contraseña directamente sin conocer la anterior
        var token  = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, dto.NuevaPassword);

        if (!result.Succeeded)
            return BadRequest(new { errores = result.Errors.Select(e => e.Description) });

        return Ok(new { mensaje = "Contraseña actualizada correctamente." });
    }

    // ════════════════════════════════════════════════════════════════════════
    //  DELETE /api/usuarios/{id}  (desactivar, no eliminar físicamente)
    // ════════════════════════════════════════════════════════════════════════
    [HttpDelete("{id}")]
    public async Task<IActionResult> Desactivar(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        // Soft delete: desactiva en vez de eliminar para preservar historial
        user.Activo = false;
        await userManager.UpdateAsync(user);

        return Ok(new { mensaje = $"Usuario '{user.Nombre}' desactivado." });
    }
}
