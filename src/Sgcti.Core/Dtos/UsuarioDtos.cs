using System.ComponentModel.DataAnnotations;

namespace Sgcti.Core.Dtos;

public class CrearUsuarioDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 150 caracteres.")]
    public string NombreCompleto { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "El usuario no puede exceder 50 caracteres.")]
    public string? Username { get; set; }

    [StringLength(200, ErrorMessage = "La contraseña no puede exceder 200 caracteres.")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "El rol es obligatorio.")]
    [RegularExpression("^(Admin|Ingeniero|Tecnico|Técnico)$", ErrorMessage = "El rol debe ser 'Admin', 'Ingeniero' o 'Técnico'.")]
    public string Rol { get; set; } = UsuarioRol.RolTecnico;

    [StringLength(100, ErrorMessage = "El cargo no puede exceder 100 caracteres.")]
    public string? Cargo { get; set; }

    [StringLength(100, ErrorMessage = "El departamento no puede exceder 100 caracteres.")]
    public string? Departamento { get; set; }

    [RegularExpression("^(Baja|Media|Alta|Critica|Crítica)?$", ErrorMessage = "La prioridad debe ser Baja, Media, Alta o Crítica.")]
    public string? PrioridadPersonalizada { get; set; }

    [StringLength(100, ErrorMessage = "El teléfono no puede exceder 100 caracteres.")]
    public string? TelegramUsuarioOId { get; set; }
}

public class ActualizarUsuarioDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 150 caracteres.")]
    public string NombreCompleto { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "El usuario no puede exceder 50 caracteres.")]
    public string? Username { get; set; }

    [StringLength(200, ErrorMessage = "La contraseña no puede exceder 200 caracteres.")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "El rol es obligatorio.")]
    [RegularExpression("^(Admin|Ingeniero|Tecnico|Técnico)$", ErrorMessage = "El rol debe ser 'Admin', 'Ingeniero' o 'Técnico'.")]
    public string Rol { get; set; } = UsuarioRol.RolTecnico;

    public bool Activo { get; set; } = true;

    [StringLength(100, ErrorMessage = "El cargo no puede exceder 100 caracteres.")]
    public string? Cargo { get; set; }

    [StringLength(100, ErrorMessage = "El departamento no puede exceder 100 caracteres.")]
    public string? Departamento { get; set; }

    [RegularExpression("^(Baja|Media|Alta|Critica|Crítica)?$", ErrorMessage = "La prioridad debe ser Baja, Media, Alta o Crítica.")]
    public string? PrioridadPersonalizada { get; set; }

    [StringLength(100, ErrorMessage = "El teléfono no puede exceder 100 caracteres.")]
    public string? TelegramUsuarioOId { get; set; }
}

public class CrearEmpleadoDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 150 caracteres.")]
    public string NombreCompleto { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "El cargo no puede exceder 100 caracteres.")]
    public string? Cargo { get; set; }

    [StringLength(100, ErrorMessage = "El departamento no puede exceder 100 caracteres.")]
    public string? Departamento { get; set; }

    [RegularExpression("^(Baja|Media|Alta|Critica|Crítica)?$", ErrorMessage = "La prioridad debe ser Baja, Media, Alta o Crítica.")]
    public string? PrioridadPersonalizada { get; set; }

    [StringLength(40, ErrorMessage = "El número de teléfono no puede exceder 40 caracteres.")]
    public string? Telefono { get; set; }

    [StringLength(100, ErrorMessage = "El dato de Telegram no puede exceder 100 caracteres.")]
    public string? TelegramUsuarioOId { get; set; }
}

public class ActualizarEmpleadoDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 150 caracteres.")]
    public string NombreCompleto { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    [StringLength(100, ErrorMessage = "El cargo no puede exceder 100 caracteres.")]
    public string? Cargo { get; set; }

    [StringLength(100, ErrorMessage = "El departamento no puede exceder 100 caracteres.")]
    public string? Departamento { get; set; }

    [RegularExpression("^(Baja|Media|Alta|Critica|Crítica)?$", ErrorMessage = "La prioridad debe ser Baja, Media, Alta o Crítica.")]
    public string? PrioridadPersonalizada { get; set; }

    [StringLength(40, ErrorMessage = "El número de teléfono no puede exceder 40 caracteres.")]
    public string? Telefono { get; set; }

    [StringLength(100, ErrorMessage = "El dato de Telegram no puede exceder 100 caracteres.")]
    public string? TelegramUsuarioOId { get; set; }
}

public static class UsuarioRol
{
    public const string RolAdmin = "Admin";
    public const string RolIngeniero = "Ingeniero";
    public const string RolTecnico = "Tecnico";
    public const string RolEmpleado = "Empleado";
}