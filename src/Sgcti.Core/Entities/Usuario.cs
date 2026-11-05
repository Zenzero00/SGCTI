namespace Sgcti.Core.Entities;

public class Usuario
{
    public int Id { get; set; }

    public string NombreCompleto { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Rol { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public string? Cargo { get; set; }

    public string? Departamento { get; set; }

    public PrioridadTicket? PrioridadPersonalizada { get; set; }

    public string? TelegramUsuarioOId { get; set; }

    public const string RolAdmin = "Admin";
    public const string RolIngeniero = "Ingeniero";
    public const string RolTecnico = "Tecnico";
    public const string RolEmpleado = "Empleado";
}