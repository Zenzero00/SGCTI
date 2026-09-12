namespace Sgcti.Core.Entities;

public class Ticket
{
    public int Id { get; set; }

    public string UsuarioSolicitante { get; set; } = string.Empty;

    public string Departamento { get; set; } = string.Empty;

    public string Categoria { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public PrioridadTicket Prioridad { get; set; }

    public EstadoTicket Estado { get; set; } = EstadoTicket.Abierto;

    public DateTime FechaApertura { get; set; }

    public DateTime? FechaCierre { get; set; }
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
    EnProceso,
    EnEsperaDeRepuesto,
    Resuelto,
    Cerrado
}