using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sgcti.Core.Entities;
using Sgcti.Infrastructure.Persistence;

namespace Sgcti.Api.Controllers;

public class LoginRequest
{
    public string Usuario { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;

    public DateTime Expira { get; set; }

    public string NombreCompleto { get; set; } = string.Empty;

    public string Rol { get; set; } = string.Empty;
}

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private const int DuracionHoras = 2;

    private readonly IConfiguration _configuration;

    private readonly SgctiDbContext _db;

    public AuthController(IConfiguration configuration, SgctiDbContext db)
    {
        _configuration = configuration;
        _db = db;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Usuario) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Usuario y contraseña son obligatorios.");
        }

        var usuario = await _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == request.Usuario, ct);

        if (usuario is null || usuario.PasswordHash != request.Password)
        {
            return Unauthorized("Usuario o contraseña incorrectos.");
        }

        if (!usuario.Activo)
        {
            return Unauthorized("El usuario está inactivo. Contacta al administrador.");
        }

        if (!EsRolConAccesoAlSistema(usuario.Rol))
        {
            return Unauthorized(
                "Este registro pertenece al directorio de empleados y no tiene acceso al sistema. " +
                "Solo las cuentas Admin, Ingeniero y Técnico pueden iniciar sesión.");
        }

        var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("No se configuró 'Jwt:Key'.");
        var issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("No se configuró 'Jwt:Issuer'.");

        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var rolNormalizado = NormalizarRolParaMostrar(usuario.Rol);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Username),
            new(ClaimTypes.Name, usuario.Username),
            new(ClaimTypes.Role, usuario.Rol),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Garantiza compatibilidad con todos los endpoints que validan [Authorize(Roles = "Admin,Tecnico")]
        if (rolNormalizado is "Ingeniero" or "Técnico" &&
            !string.Equals(usuario.Rol, Usuario.RolTecnico, StringComparison.Ordinal))
        {
            claims.Add(new Claim(ClaimTypes.Role, Usuario.RolTecnico));
        }

        if (rolNormalizado == "Técnico")
        {
            claims.Add(new Claim(ClaimTypes.Role, "Técnico"));
        }

        var expira = DateTime.UtcNow.AddHours(DuracionHoras);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: issuer,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expira,
            signingCredentials: credenciales);

        return Ok(new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expira = expira,
            NombreCompleto = usuario.NombreCompleto,
            Rol = rolNormalizado
        });
    }

    private static bool EsRolConAccesoAlSistema(string? rol)
    {
        if (string.IsNullOrWhiteSpace(rol)) return false;

        return rol.Trim() is
            Usuario.RolAdmin or
            Usuario.RolIngeniero or
            Usuario.RolTecnico or
            "Técnico";
    }

    private static string NormalizarRolParaMostrar(string rol)
    {
        var limpio = rol.Trim();
        if (limpio.Equals("Tecnico", StringComparison.OrdinalIgnoreCase) ||
            limpio.Equals("Técnico", StringComparison.OrdinalIgnoreCase))
        {
            return "Técnico";
        }

        if (limpio.Equals("Ingeniero", StringComparison.OrdinalIgnoreCase))
        {
            return "Ingeniero";
        }

        return Usuario.RolAdmin;
    }
}