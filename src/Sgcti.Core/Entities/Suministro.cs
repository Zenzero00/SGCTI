namespace Sgcti.Core.Entities;

public class Suministro
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;

    public int CantidadActual { get; set; }

    public int StockMinimo { get; set; }

    public string? Observacion { get; set; }
}
