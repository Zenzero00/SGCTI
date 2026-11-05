namespace Sgcti.Core.Dtos;

public class UsuarioDto
{
    public int Id { get; set; }

    public string NombreCompleto { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Rol { get; set; } = string.Empty;

    public bool Activo { get; set; }

    public string? Cargo { get; set; }

    public string? Departamento { get; set; }

    public string? PrioridadPersonalizada { get; set; }

    public string? TelegramUsuarioOId { get; set; }
}

public class DirectorioEmpleadoDto
{
    public int Id { get; set; }

    public string NombreCompleto { get; set; } = string.Empty;

    public string? Cargo { get; set; }

    public string? Departamento { get; set; }

    public string? PrioridadPersonalizada { get; set; }

    public string? Telefono { get; set; }
}

public class EmpleadoDto
{
    public int Id { get; set; }

    public string NombreCompleto { get; set; } = string.Empty;

    public bool Activo { get; set; }

    public string? Cargo { get; set; }

    public string? Departamento { get; set; }

    public string? PrioridadPersonalizada { get; set; }

    public string? Telefono { get; set; }

    public string? TelegramChatId { get; set; }

    public bool VinculadoTelegram { get; set; }

    public string? TelegramUsuarioOId { get; set; }
}