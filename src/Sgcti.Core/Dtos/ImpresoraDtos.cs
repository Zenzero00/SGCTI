using System.ComponentModel.DataAnnotations;

namespace Sgcti.Core.Dtos;

public class CrearImpresoraDto
{
    [Required(ErrorMessage = "El modelo es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El modelo debe tener entre 2 y 100 caracteres.")]
    public string Modelo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La IP es obligatoria.")]
    [StringLength(45, MinimumLength = 1, ErrorMessage = "La IP no puede exceder 45 caracteres.")]
    public string Ip { get; set; } = string.Empty;

    [Required(ErrorMessage = "El departamento es obligatorio.")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "El departamento debe tener entre 2 y 60 caracteres.")]
    public string Departamento { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "La comunidad SNMP no puede exceder 100 caracteres.")]
    public string ComunidadSnmp { get; set; } = "public";
}

public class ActualizarImpresoraDto
{
    [Required(ErrorMessage = "El modelo es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El modelo debe tener entre 2 y 100 caracteres.")]
    public string Modelo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La IP es obligatoria.")]
    [StringLength(45, MinimumLength = 1, ErrorMessage = "La IP no puede exceder 45 caracteres.")]
    public string Ip { get; set; } = string.Empty;

    [Required(ErrorMessage = "El departamento es obligatorio.")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "El departamento debe tener entre 2 y 60 caracteres.")]
    public string Departamento { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "La comunidad SNMP no puede exceder 100 caracteres.")]
    public string ComunidadSnmp { get; set; } = "public";
}