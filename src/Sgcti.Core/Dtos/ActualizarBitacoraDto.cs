using System.ComponentModel.DataAnnotations;

namespace Sgcti.Core.Dtos;

public class ActualizarBitacoraDto
{
    [Required(ErrorMessage = "El estado es obligatorio.")]
    [RegularExpression("^(Pendiente|Completado)$", ErrorMessage = "El estado debe ser 'Pendiente' o 'Completado'.")]
    public string Estado { get; set; } = string.Empty;

    public DateTime? HoraFin { get; set; }

    [StringLength(2000, ErrorMessage = "La descripción no puede exceder 2000 caracteres.")]
    public string? DescripcionActividad { get; set; }
}