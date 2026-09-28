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