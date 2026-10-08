using System.ComponentModel.DataAnnotations;

namespace Sgcti.Core.Dtos;

public class ActualizarObservacionDto
{
    [StringLength(500, ErrorMessage = "La observación no puede exceder 500 caracteres.")]
    public string? Observacion { get; set; }
}