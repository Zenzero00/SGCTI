using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sgcti.Core.Dtos;
using Sgcti.Core.Entities;
using Sgcti.Core.Services;
using Sgcti.Infrastructure.Persistence;

namespace Sgcti.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController : ControllerBase
{
    private const string RolesGestion =
        Usuario.RolAdmin + "," + Usuario.RolIngeniero + "," + Usuario.RolTecnico + ",Técnico";

    private static readonly Dictionary<string, string> TelefonosDemoPorNombre = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Ana Torres"] = "0414-4092817",
        ["Carlos Pérez"] = "0414-3289104",
        ["José Martínez"] = "0412-8841209",
        ["Laura Gómez"] = "0424-4910238",
        ["Luis Ramírez"] = "0424-3117845",
        ["María Rodríguez"] = "0416-6421908"
    };

    private readonly SgctiDbContext _db;

    public UsuariosController(SgctiDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [Authorize(Roles = RolesGestion)]
    public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetAll(CancellationToken ct)
    {
        var usuarios = await _db.Usuarios
            .AsNoTracking()
            .Where(u => u.Rol != Usuario.RolEmpleado)
            .OrderBy(u => u.Rol)
            .ThenBy(u => u.NombreCompleto)
            .ToListAsync(ct);

        return Ok(usuarios.Select(ADto));
    }

    [HttpGet("directorio")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<DirectorioEmpleadoDto>>> GetDirectorio(CancellationToken ct)
    {
        await NormalizarEmpleadosExistentesAsync(ct);

        var empleados = await _db.Usuarios
            .AsNoTracking()
            .Where(u => u.Activo && u.Rol == Usuario.RolEmpleado)
            .OrderBy(u => u.NombreCompleto)
            .ToListAsync(ct);

        return Ok(empleados.Select(u =>
        {
            var (telefono, _) = FormatoTelegramEmpleado.Desglosar(u.TelegramUsuarioOId);

            return new DirectorioEmpleadoDto
            {
                Id = u.Id,
                NombreCompleto = u.NombreCompleto,
                Cargo = u.Cargo,
                Departamento = u.Departamento,
                PrioridadPersonalizada = FormatearPrioridadVisual(u.PrioridadPersonalizada),
                Telefono = telefono
            };
        }));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = RolesGestion)]
    public async Task<ActionResult<UsuarioDto>> GetById(int id, CancellationToken ct)
    {
        var usuario = await _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id && u.Rol != Usuario.RolEmpleado, ct);

        if (usuario is null)
        {
            return NotFound();
        }

        return Ok(ADto(usuario));
    }

    [HttpPost]
    [Authorize(Roles = RolesGestion)]
    public async Task<ActionResult<UsuarioDto>> Create(CrearUsuarioDto dto, CancellationToken ct)
    {
        if (!RolSistemaValido(dto.Rol, out var rol))
        {
            return BadRequest("El rol debe ser 'Admin', 'Ingeniero' o 'Técnico'. Los empleados se registran en el directorio de Telegram.");
        }

        if (rol == Usuario.RolAdmin && !EsAdminActual())
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Solo un administrador puede crear cuentas con rol Admin.");
        }

        var username = dto.Username?.Trim() ?? string.Empty;
        var password = dto.Password ?? string.Empty;

        if (username.Length < 3 || username.Length > 50)
        {
            return BadRequest("El usuario es obligatorio y debe tener entre 3 y 50 caracteres.");
        }

        if (password.Length < 4)
        {
            return BadRequest("La contraseña es obligatoria y debe tener al menos 4 caracteres.");
        }

        if (await ExisteUsernameAsync(username, null, ct))
        {
            return BadRequest("Ya existe un usuario con ese nombre de usuario.");
        }

        var entidad = new Usuario
        {
            NombreCompleto = dto.NombreCompleto.Trim(),
            Username = username,
            PasswordHash = password,
            Rol = rol,
            Activo = true
        };

        _db.Usuarios.Add(entidad);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = entidad.Id }, ADto(entidad));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = RolesGestion)]
    public async Task<IActionResult> Update(int id, ActualizarUsuarioDto dto, CancellationToken ct)
    {
        var entidad = await _db.Usuarios.FindAsync(new object[] { id }, ct);

        if (entidad is null || entidad.Rol == Usuario.RolEmpleado)
        {
            return NotFound();
        }

        if (!RolSistemaValido(dto.Rol, out var rol))
        {
            return BadRequest("El rol debe ser 'Admin', 'Ingeniero' o 'Técnico'. Los empleados se registran en el directorio de Telegram.");
        }

        if ((entidad.Rol == Usuario.RolAdmin || rol == Usuario.RolAdmin || EsAdministradorPrincipal(entidad)) && !EsAdminActual())
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Los ingenieros y técnicos no tienen permiso para editar las credenciales ni el estado de un administrador.");
        }

        if (EsAdministradorPrincipal(entidad) && !dto.Activo)
        {
            return BadRequest("No se puede desactivar al administrador principal del sistema.");
        }

        var username = dto.Username?.Trim() ?? string.Empty;

        if (username.Length < 3 || username.Length > 50)
        {
            return BadRequest("El usuario es obligatorio y debe tener entre 3 y 50 caracteres.");
        }

        if (await ExisteUsernameAsync(username, id, ct))
        {
            return BadRequest("Ya existe un usuario con ese nombre de usuario.");
        }

        var password = dto.Password ?? string.Empty;

        if (password.Length > 0 && password.Length < 4)
        {
            return BadRequest("La contraseña debe tener al menos 4 caracteres.");
        }

        if (password.Length == 0 && string.IsNullOrEmpty(entidad.PasswordHash))
        {
            return BadRequest("Los usuarios Admin, Ingeniero y Técnico requieren una contraseña para iniciar sesión.");
        }

        entidad.NombreCompleto = dto.NombreCompleto.Trim();
        entidad.Username = username;
        entidad.Rol = rol;
        entidad.Activo = dto.Activo;

        entidad.Cargo = null;
        entidad.Departamento = null;
        entidad.PrioridadPersonalizada = null;
        entidad.TelegramUsuarioOId = null;

        if (password.Length > 0)
        {
            entidad.PasswordHash = password;
        }

        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = RolesGestion)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entidad = await _db.Usuarios.FindAsync(new object[] { id }, ct);

        if (entidad is null || entidad.Rol == Usuario.RolEmpleado)
        {
            return NotFound();
        }

        if ((entidad.Rol == Usuario.RolAdmin || EsAdministradorPrincipal(entidad)) && !EsAdminActual())
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Los ingenieros y técnicos no tienen permiso para eliminar a un administrador.");
        }

        if (EsAdministradorPrincipal(entidad))
        {
            return BadRequest("No se puede eliminar al administrador principal del sistema.");
        }

        _db.Usuarios.Remove(entidad);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpGet("empleados")]
    [Authorize(Roles = RolesGestion)]
    public async Task<ActionResult<IEnumerable<EmpleadoDto>>> GetEmpleados(CancellationToken ct)
    {
        await NormalizarEmpleadosExistentesAsync(ct);

        var empleados = await _db.Usuarios
            .AsNoTracking()
            .Where(u => u.Rol == Usuario.RolEmpleado)
            .OrderBy(u => u.NombreCompleto)
            .ToListAsync(ct);

        return Ok(empleados.Select(AEmpleadoDto));
    }

    [HttpGet("empleados/{id:int}")]
    [Authorize(Roles = RolesGestion)]
    public async Task<ActionResult<EmpleadoDto>> GetEmpleado(int id, CancellationToken ct)
    {
        var empleado = await _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id && u.Rol == Usuario.RolEmpleado, ct);

        if (empleado is null)
        {
            return NotFound();
        }

        return Ok(AEmpleadoDto(empleado));
    }

    [HttpPost("empleados")]
    [Authorize(Roles = RolesGestion)]
    public async Task<ActionResult<EmpleadoDto>> CreateEmpleado(CrearEmpleadoDto dto, CancellationToken ct)
    {
        if (!TryParsePrioridad(dto.PrioridadPersonalizada, out var prioridad, out var errorPrioridad))
        {
            return BadRequest(errorPrioridad);
        }

        var telefonoEntrada = Limpiar(dto.Telefono) ?? Limpiar(dto.TelegramUsuarioOId);
        var digitos = FormatoTelegramEmpleado.NormalizarDigitosTelefono(telefonoEntrada);

        if (!string.IsNullOrWhiteSpace(telefonoEntrada) && digitos is null)
        {
            return BadRequest("Ingresa un número de teléfono válido (ej. 0412-1234567).");
        }

        if (await ExisteTelefonoEmpleadoAsync(digitos, null, ct))
        {
            return BadRequest("Ese número de teléfono ya está registrado con otro empleado.");
        }

        var entidad = new Usuario
        {
            NombreCompleto = dto.NombreCompleto.Trim(),
            Username = await GenerarUsernameEmpleadoAsync(dto.NombreCompleto, ct),
            PasswordHash = string.Empty,
            Rol = Usuario.RolEmpleado,
            Activo = true,
            Cargo = Limpiar(dto.Cargo),
            Departamento = Limpiar(dto.Departamento),
            PrioridadPersonalizada = prioridad,
            TelegramUsuarioOId = FormatoTelegramEmpleado.Combinar(telefonoEntrada, null)
        };

        _db.Usuarios.Add(entidad);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetEmpleado), new { id = entidad.Id }, AEmpleadoDto(entidad));
    }

    [HttpPut("empleados/{id:int}")]
    [Authorize(Roles = RolesGestion)]
    public async Task<IActionResult> UpdateEmpleado(int id, ActualizarEmpleadoDto dto, CancellationToken ct)
    {
        var entidad = await _db.Usuarios.FindAsync(new object[] { id }, ct);

        if (entidad is null || entidad.Rol != Usuario.RolEmpleado)
        {
            return NotFound();
        }

        if (!TryParsePrioridad(dto.PrioridadPersonalizada, out var prioridad, out var errorPrioridad))
        {
            return BadRequest(errorPrioridad);
        }

        var telefonoEntrada = Limpiar(dto.Telefono) ?? Limpiar(dto.TelegramUsuarioOId);
        var digitosNuevos = FormatoTelegramEmpleado.NormalizarDigitosTelefono(telefonoEntrada);

        if (!string.IsNullOrWhiteSpace(telefonoEntrada) && digitosNuevos is null)
        {
            return BadRequest("Ingresa un número de teléfono válido (ej. 0412-1234567).");
        }

        if (await ExisteTelefonoEmpleadoAsync(digitosNuevos, id, ct))
        {
            return BadRequest("Ese número de teléfono ya está registrado con otro empleado.");
        }

        var (telefonoAnterior, chatIdActual) = FormatoTelegramEmpleado.Desglosar(entidad.TelegramUsuarioOId);
        var digitosAnteriores = FormatoTelegramEmpleado.NormalizarDigitosTelefono(telefonoAnterior);

        var chatIdConservar = digitosNuevos is not null && digitosAnteriores == digitosNuevos
            ? chatIdActual
            : null;

        entidad.NombreCompleto = dto.NombreCompleto.Trim();
        entidad.Activo = dto.Activo;
        entidad.Cargo = Limpiar(dto.Cargo);
        entidad.Departamento = Limpiar(dto.Departamento);
        entidad.PrioridadPersonalizada = prioridad;
        entidad.TelegramUsuarioOId = FormatoTelegramEmpleado.Combinar(telefonoEntrada, chatIdConservar);
        entidad.PasswordHash = string.Empty;

        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpDelete("empleados/{id:int}")]
    [Authorize(Roles = RolesGestion)]
    public async Task<IActionResult> DeleteEmpleado(int id, CancellationToken ct)
    {
        var entidad = await _db.Usuarios.FindAsync(new object[] { id }, ct);

        if (entidad is null || entidad.Rol != Usuario.RolEmpleado)
        {
            return NotFound();
        }

        _db.Usuarios.Remove(entidad);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task NormalizarEmpleadosExistentesAsync(CancellationToken ct)
    {
        var empleados = await _db.Usuarios
            .Where(u => u.Rol == Usuario.RolEmpleado)
            .OrderBy(u => u.Id)
            .ToListAsync(ct);

        var huboCambios = false;

        foreach (var emp in empleados)
        {
            if (emp.NombreCompleto.StartsWith("Usuario Telegram ", StringComparison.OrdinalIgnoreCase))
            {
                _db.Usuarios.Remove(emp);
                huboCambios = true;
                continue;
            }

            var (telefono, chatId) = FormatoTelegramEmpleado.Desglosar(emp.TelegramUsuarioOId);

            if (telefono == "0412-4158210")
            {
                emp.TelegramUsuarioOId = null;
                huboCambios = true;
                continue;
            }

            if (string.IsNullOrWhiteSpace(telefono) && TelefonosDemoPorNombre.TryGetValue(emp.NombreCompleto.Trim(), out var telefonoDemo))
            {
                emp.TelegramUsuarioOId = FormatoTelegramEmpleado.Combinar(telefonoDemo, null);
                huboCambios = true;
            }
            else if (string.IsNullOrWhiteSpace(telefono) && !string.IsNullOrWhiteSpace(chatId))
            {
                emp.TelegramUsuarioOId = null;
                huboCambios = true;
            }
        }

        if (huboCambios)
        {
            await _db.SaveChangesAsync(ct);
        }
    }

    private bool EsAdminActual()
        => User.IsInRole(Usuario.RolAdmin);

    private static bool EsAdministradorPrincipal(Usuario usuario)
        => usuario.Username.Equals("admin", StringComparison.OrdinalIgnoreCase)
            || (usuario.Id == 1 && usuario.Rol == Usuario.RolAdmin);

    private async Task<bool> ExisteUsernameAsync(
        string username, int? excluirId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return false;
        }

        return await _db.Usuarios.AnyAsync(
            u => u.Username.ToLower() == username.ToLower()
                && (!excluirId.HasValue || u.Id != excluirId.Value),
            ct);
    }

    private async Task<bool> ExisteTelefonoEmpleadoAsync(
        string? digitosTelefono, int? excluirId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(digitosTelefono))
        {
            return false;
        }

        var empleados = await _db.Usuarios
            .AsNoTracking()
            .Where(u => u.Rol == Usuario.RolEmpleado && (!excluirId.HasValue || u.Id != excluirId.Value))
            .Select(u => u.TelegramUsuarioOId)
            .ToListAsync(ct);

        return empleados.Any(valor =>
        {
            var (tel, _) = FormatoTelegramEmpleado.Desglosar(valor);
            return FormatoTelegramEmpleado.NormalizarDigitosTelefono(tel) == digitosTelefono;
        });
    }

    private async Task<string> GenerarUsernameEmpleadoAsync(string nombre, CancellationToken ct)
    {
        var baseUsuario = new string(
            QuitarAcentos(nombre)
                .ToLowerInvariant()
                .Where(char.IsLetterOrDigit)
                .Take(30)
                .ToArray());

        if (baseUsuario.Length < 3)
        {
            baseUsuario = "empleado";
        }

        var candidato = baseUsuario;
        var sufijo = 1;

        while (await _db.Usuarios.AnyAsync(u => u.Username == candidato, ct))
        {
            sufijo++;
            candidato = $"{baseUsuario}{sufijo}";
        }

        return candidato;
    }

    private static bool RolSistemaValido(string rol, out string rolNormalizado)
    {
        var limpio = QuitarAcentos(rol.Trim()).ToLowerInvariant();

        switch (limpio)
        {
            case "admin":
                rolNormalizado = Usuario.RolAdmin;
                return true;
            case "ingeniero":
                rolNormalizado = Usuario.RolIngeniero;
                return true;
            case "tecnico":
                rolNormalizado = Usuario.RolTecnico;
                return true;
            default:
                rolNormalizado = string.Empty;
                return false;
        }
    }

    private static string FormatearRolVisual(string rol)
    {
        var limpio = QuitarAcentos(rol.Trim()).ToLowerInvariant();
        return limpio switch
        {
            "admin" => "Admin",
            "ingeniero" => "Ingeniero",
            "tecnico" => "Técnico",
            _ => rol
        };
    }

    private static string? FormatearPrioridadVisual(PrioridadTicket? prioridad)
    {
        return prioridad switch
        {
            PrioridadTicket.Critica => "Crítica",
            PrioridadTicket.Alta => "Alta",
            PrioridadTicket.Media => "Media",
            PrioridadTicket.Baja => "Baja",
            _ => null
        };
    }

    private static bool TryParsePrioridad(
        string? valor, out PrioridadTicket? prioridad, out string error)
    {
        error = string.Empty;
        prioridad = null;

        if (string.IsNullOrWhiteSpace(valor))
        {
            return true;
        }

        var parseada = ClasificadorPrioridadTicket.PrioridadDesdeTexto(valor);

        if (parseada is null)
        {
            error = "La prioridad debe ser Baja, Media, Alta o Crítica.";
            return false;
        }

        prioridad = parseada;
        return true;
    }

    private static string? Limpiar(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static string QuitarAcentos(string valor)
    {
        var descompuesto = valor.Normalize(NormalizationForm.FormD);
        var resultado = new StringBuilder(descompuesto.Length);

        foreach (var caracter in descompuesto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caracter) != UnicodeCategory.NonSpacingMark)
            {
                resultado.Append(caracter);
            }
        }

        return resultado.ToString();
    }

    private static UsuarioDto ADto(Usuario u) => new()
    {
        Id = u.Id,
        NombreCompleto = u.NombreCompleto,
        Username = u.Username,
        Rol = FormatearRolVisual(u.Rol),
        Activo = u.Activo
    };

    private static EmpleadoDto AEmpleadoDto(Usuario u)
    {
        var (telefono, chatId) = FormatoTelegramEmpleado.Desglosar(u.TelegramUsuarioOId);

        return new EmpleadoDto
        {
            Id = u.Id,
            NombreCompleto = u.NombreCompleto,
            Activo = u.Activo,
            Cargo = u.Cargo,
            Departamento = u.Departamento,
            PrioridadPersonalizada = FormatearPrioridadVisual(u.PrioridadPersonalizada),
            Telefono = telefono,
            TelegramChatId = chatId,
            VinculadoTelegram = !string.IsNullOrWhiteSpace(telefono) && !string.IsNullOrWhiteSpace(chatId),
            TelegramUsuarioOId = telefono
        };
    }
}