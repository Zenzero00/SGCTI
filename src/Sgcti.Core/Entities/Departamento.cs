namespace Sgcti.Core.Entities;

public class Departamento
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Sigla { get; set; } = string.Empty;

    public PrioridadTicket PrioridadPorDefecto { get; set; } = PrioridadTicket.Media;
}
