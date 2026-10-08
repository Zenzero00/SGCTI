using System.ComponentModel.DataAnnotations;

namespace Sgcti.Core.Dtos;

public class CrearUsuarioDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 150 caracteres.")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(200, MinimumLength = 4, ErrorMessage = "La contraseña debe tener al menos 4 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio.")]
    [RegularExpression("^(Admin|Tecnico)$", ErrorMessage = "El rol debe ser 'Admin' o 'Tecnico'.")]
    public string Rol { get; set; } = UsuarioRol.RolTecnico;
}

public class ActualizarUsuarioDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 150 caracteres.")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres.")]
    public string Username { get; set; } = string.Empty;

    [StringLength(200, MinimumLength = 4, ErrorMessage = "La contraseña debe tener al menos 4 caracteres.")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "El rol es obligatorio.")]
    [RegularExpression("^(Admin|Tecnico)$", ErrorMessage = "El rol debe ser 'Admin' o 'Tecnico'.")]
    public string Rol { get; set; } = UsuarioRol.RolTecnico;

    public bool Activo { get; set; } = true;
}

public static class UsuarioRol
{
    public const string RolAdmin = "Admin";
    public const string RolTecnico = "Tecnico";
}