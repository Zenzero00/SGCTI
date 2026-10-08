using System.ComponentModel.DataAnnotations;

namespace Sgcti.Core.Dtos;

public class RegistrarMovimientoDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El suministro es obligatorio.")]
    public int SuministroId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La impresora debe ser un id válido.")]
    public int? ImpresoraId { get; set; }

    [Required(ErrorMessage = "El tipo de movimiento es obligatorio.")]
    [RegularExpression("^(Entrada|Salida)$", ErrorMessage = "El tipo de movimiento debe ser 'Entrada' o 'Salida'.")]
    public string TipoMovimiento { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    public int Cantidad { get; set; }

    [StringLength(500, ErrorMessage = "La observación no puede exceder 500 caracteres.")]
    public string? Observacion { get; set; }
}
