namespace Sgcti.Core.Entities;

public class Ticket
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public EstadoTicket Estado { get; set; } = EstadoTicket.Abierto;

    public PrioridadTicket Prioridad { get; set; }

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

public enum PrioridadTicket
{
    Baja,
    Media,
    Alta,
    Critica
}

public enum EstadoTicket
{
    Abierto,
    EnProgreso,
    Resuelto
}