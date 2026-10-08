using System.ComponentModel.DataAnnotations;

namespace Sgcti.Core.Dtos;

public class CrearSuministroDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo es obligatorio.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El tipo debe tener entre 2 y 50 caracteres.")]
    public string Tipo { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "La cantidad actual no puede ser negativa.")]
    public int CantidadActual { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo.")]
    public int StockMinimo { get; set; }

    [StringLength(500, ErrorMessage = "La observación no puede exceder 500 caracteres.")]
    public string? Observacion { get; set; }
}
