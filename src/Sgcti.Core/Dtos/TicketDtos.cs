using System.ComponentModel.DataAnnotations;

namespace Sgcti.Core.Dtos;

public class TicketDto
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public int Estado { get; set; }

    public int Prioridad { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaCierre { get; set; }

    public int SLAHoras { get; set; }

    public string? Solicitante { get; set; }

    public string? TelegramChatId { get; set; }

    public string? CodigoNomenclatura { get; set; }

    public string? Departamento { get; set; }

    public string? Categoria { get; set; }

    public string? EquipoAfectado { get; set; }

    public string? SolucionAplicada { get; set; }

    public string? SuministrosUtilizados { get; set; }

    public DateTime? FechaResolucion { get; set; }
}

public class CrearTicketDto
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "La descripción no puede exceder 2000 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    public int Estado { get; set; }

    public int Prioridad { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int SLAHoras { get; set; }

    [StringLength(150, ErrorMessage = "El solicitante no puede exceder 150 caracteres.")]
    public string? Solicitante { get; set; }

    [StringLength(100, ErrorMessage = "El ChatId de Telegram no puede exceder 100 caracteres.")]
    public string? TelegramChatId { get; set; }

    [StringLength(30, ErrorMessage = "El código de nomenclatura no puede exceder 30 caracteres.")]
    public string? CodigoNomenclatura { get; set; }

    [StringLength(100, ErrorMessage = "El departamento no puede exceder 100 caracteres.")]
    public string? Departamento { get; set; }

    [StringLength(100, ErrorMessage = "La categoría no puede exceder 100 caracteres.")]
    public string? Categoria { get; set; }

    [StringLength(150, ErrorMessage = "El equipo afectado no puede exceder 150 caracteres.")]
    public string? EquipoAfectado { get; set; }

    [StringLength(2000, ErrorMessage = "La solución aplicada no puede exceder 2000 caracteres.")]
    public string? SolucionAplicada { get; set; }

    [StringLength(1000, ErrorMessage = "Los suministros utilizados no pueden exceder 1000 caracteres.")]
    public string? SuministrosUtilizados { get; set; }

    public DateTime? FechaResolucion { get; set; }
}

public class ActualizarTicketDto
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "La descripción no puede exceder 2000 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    public int Estado { get; set; }

    public int Prioridad { get; set; }

    public DateTime? FechaCierre { get; set; }

    public int SLAHoras { get; set; }

    [StringLength(150, ErrorMessage = "El solicitante no puede exceder 150 caracteres.")]
    public string? Solicitante { get; set; }

    [StringLength(100, ErrorMessage = "El ChatId de Telegram no puede exceder 100 caracteres.")]
    public string? TelegramChatId { get; set; }

    [StringLength(30, ErrorMessage = "El código de nomenclatura no puede exceder 30 caracteres.")]
    public string? CodigoNomenclatura { get; set; }

    [StringLength(100, ErrorMessage = "El departamento no puede exceder 100 caracteres.")]
    public string? Departamento { get; set; }

    [StringLength(100, ErrorMessage = "La categoría no puede exceder 100 caracteres.")]
    public string? Categoria { get; set; }

    [StringLength(150, ErrorMessage = "El equipo afectado no puede exceder 150 caracteres.")]
    public string? EquipoAfectado { get; set; }

    [StringLength(2000, ErrorMessage = "La solución aplicada no puede exceder 2000 caracteres.")]
    public string? SolucionAplicada { get; set; }

    [StringLength(1000, ErrorMessage = "Los suministros utilizados no pueden exceder 1000 caracteres.")]
    public string? SuministrosUtilizados { get; set; }

    public DateTime? FechaResolucion { get; set; }
}
