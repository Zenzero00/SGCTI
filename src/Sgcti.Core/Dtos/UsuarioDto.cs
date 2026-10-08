namespace Sgcti.Core.Dtos;

public class UsuarioDto
{
    public int Id { get; set; }

    public string NombreCompleto { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Rol { get; set; } = string.Empty;

    public bool Activo { get; set; }
}