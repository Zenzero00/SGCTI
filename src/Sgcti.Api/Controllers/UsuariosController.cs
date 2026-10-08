using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sgcti.Core.Dtos;
using Sgcti.Core.Entities;
using Sgcti.Infrastructure.Persistence;

namespace Sgcti.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly SgctiDbContext _db;

    public UsuariosController(SgctiDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [Authorize(Roles = Usuario.RolAdmin)]
    public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetAll(CancellationToken ct)
    {
        var usuarios = await _db.Usuarios
            .AsNoTracking()
            .OrderBy(u => u.Rol)
            .ThenBy(u => u.NombreCompleto)
            .Select(u => new UsuarioDto
            {
                Id = u.Id,
                NombreCompleto = u.NombreCompleto,
                Username = u.Username,
                Rol = u.Rol,
                Activo = u.Activo
            })
            .ToListAsync(ct);

        return Ok(usuarios);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = Usuario.RolAdmin)]
    public async Task<ActionResult<UsuarioDto>> GetById(int id, CancellationToken ct)
    {
        var usuario = await _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);

        if (usuario is null)
        {
            return NotFound();
        }

        return Ok(ADto(usuario));
    }

    [HttpPost]
    [Authorize(Roles = Usuario.RolAdmin)]
    public async Task<ActionResult<UsuarioDto>> Create(CrearUsuarioDto dto, CancellationToken ct)
    {
        if (await ExisteUsernameAsync(dto.Username.Trim(), null, ct))
        {
            return BadRequest("Ya existe un usuario con ese nombre de usuario.");
        }

        var entidad = new Usuario
        {
            NombreCompleto = dto.NombreCompleto.Trim(),
            Username = dto.Username.Trim(),
            PasswordHash = dto.Password,
            Rol = dto.Rol,
            Activo = true
        };

        _db.Usuarios.Add(entidad);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = entidad.Id }, ADto(entidad));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Usuario.RolAdmin)]
    public async Task<IActionResult> Update(int id, ActualizarUsuarioDto dto, CancellationToken ct)
    {
        var entidad = await _db.Usuarios.FindAsync(new object[] { id }, ct);

        if (entidad is null)
        {
            return NotFound();
        }

        if (await ExisteUsernameAsync(dto.Username.Trim(), id, ct))
        {
            return BadRequest("Ya existe un usuario con ese nombre de usuario.");
        }

        if (EsAdministradorPrincipal(entidad) && !dto.Activo)
        {
            return BadRequest("No se puede desactivar al administrador principal del sistema.");
        }

        entidad.NombreCompleto = dto.NombreCompleto.Trim();
        entidad.Username = dto.Username.Trim();
        entidad.Rol = dto.Rol;
        entidad.Activo = dto.Activo;

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            entidad.PasswordHash = dto.Password;
        }

        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Usuario.RolAdmin)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entidad = await _db.Usuarios.FindAsync(new object[] { id }, ct);

        if (entidad is null)
        {
            return NotFound();
        }

        if (EsAdministradorPrincipal(entidad))
        {
            return BadRequest("No se puede eliminar al administrador principal del sistema.");
        }

        _db.Usuarios.Remove(entidad);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    private static bool EsAdministradorPrincipal(Usuario usuario)
        => usuario.Username.Equals("admin", StringComparison.OrdinalIgnoreCase)
            || (usuario.Id == 1 && usuario.Rol == Usuario.RolAdmin);

    private async Task<bool> ExisteUsernameAsync(
        string username, int? excluirId, CancellationToken ct)
    {
        return await _db.Usuarios.AnyAsync(
            u => u.Username == username && u.Id != excluirId, ct);
    }

    private static UsuarioDto ADto(Usuario u) => new()
    {
        Id = u.Id,
        NombreCompleto = u.NombreCompleto,
        Username = u.Username,
        Rol = u.Rol,
        Activo = u.Activo
    };
}