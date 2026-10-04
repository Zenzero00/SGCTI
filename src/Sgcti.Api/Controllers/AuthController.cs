using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

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
}

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private const string UsuarioSimulado = "admin";
    private const string PasswordSimulado = "stargas2026";
    private const string RolSimulado = "Admin";
    private const int DuracionHoras = 2;

    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    public ActionResult<LoginResponse> Login(LoginRequest request)
    {
        if (request.Usuario != UsuarioSimulado || request.Password != PasswordSimulado)
        {
            return Unauthorized("Usuario o password incorrectos.");
        }

        var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("No se configuró 'Jwt:Key'.");
        var issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("No se configuró 'Jwt:Issuer'.");

        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, request.Usuario),
            new Claim(ClaimTypes.Name, request.Usuario),
            new Claim(ClaimTypes.Role, RolSimulado),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

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
            Expira = expira
        });
    }
}
