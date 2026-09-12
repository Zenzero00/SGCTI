namespace Sgcti.Core.Entities;

public class Consumible
{
    public int Id { get; set; }

    public string Tipo { get; set; } = string.Empty;

    public string ModeloCompatible { get; set; } = string.Empty;

    public int StockActual { get; set; }

    public int StockMinimo { get; set; }

    public int CantidadActual { get; set; }

    public int DiasAntelacionPedido { get; set; }

    public decimal CostoUnitario { get; set; }

    public ICollection<HistorialConsumo> HistorialConsumo { get; set; } = new List<HistorialConsumo>();
}