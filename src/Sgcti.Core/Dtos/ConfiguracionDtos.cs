using System.ComponentModel.DataAnnotations;

namespace Sgcti.Core.Dtos;

public class DepartamentoDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Sigla { get; set; } = string.Empty;

    public string PrioridadPorDefecto { get; set; } = "Media";
}

public class CrearDepartamentoDto
{
    [Required(ErrorMessage = "El nombre del departamento es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La sigla es obligatoria.")]
    [StringLength(10, MinimumLength = 2, ErrorMessage = "La sigla debe tener entre 2 y 10 caracteres.")]
    public string Sigla { get; set; } = string.Empty;

    [Required(ErrorMessage = "La prioridad por defecto es obligatoria.")]
    public string PrioridadPorDefecto { get; set; } = "Media";
}

public class ActualizarDepartamentoDto : CrearDepartamentoDto
{
}

public class CategoriaTicketDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;
}

public class CrearCategoriaTicketDto
{
    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;
}

public class ActualizarCategoriaTicketDto : CrearCategoriaTicketDto
{
}
