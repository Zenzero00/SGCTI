using System.ComponentModel.DataAnnotations;

namespace Sgcti.Core.Dtos;

public class CrearBitacoraDto
{
    [Required(ErrorMessage = "El analista es obligatorio.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El analista debe tener entre 2 y 50 caracteres.")]
    public string AnalistaId { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción de la actividad es obligatoria.")]
    [StringLength(2000, MinimumLength = 2, ErrorMessage = "La descripción debe tener entre 2 y 2000 caracteres.")]
    public string DescripcionActividad { get; set; } = string.Empty;

    [StringLength(150, ErrorMessage = "El equipo intervenido no puede exceder 150 caracteres.")]
    public string? EquipoIntervenido { get; set; }

    [Required(ErrorMessage = "La etiqueta es obligatoria.")]
    [StringLength(30, ErrorMessage = "La etiqueta no puede exceder 30 caracteres.")]
    public string Etiqueta { get; set; } = "Rutina";
}