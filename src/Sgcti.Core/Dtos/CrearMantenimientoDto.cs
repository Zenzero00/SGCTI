using System.ComponentModel.DataAnnotations;

namespace Sgcti.Core.Dtos;

public class CrearMantenimientoDto
{
    [Required(ErrorMessage = "El tipo de mantenimiento es obligatorio.")]
    [RegularExpression("^(Preventivo|Correctivo)$", ErrorMessage = "El tipo debe ser 'Preventivo' o 'Correctivo'.")]
    public string Tipo { get; set; } = MantenimientoImpresoraDto.TipoPreventivo;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(2000, MinimumLength = 5, ErrorMessage = "La descripción debe tener entre 5 y 2000 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El técnico es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El técnico debe tener entre 2 y 100 caracteres.")]
    public string RealizadoPor { get; set; } = string.Empty;
}

public static class MantenimientoImpresoraDto
{
    public const string TipoPreventivo = "Preventivo";
    public const string TipoCorrectivo = "Correctivo";
}