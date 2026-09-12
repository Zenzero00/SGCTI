namespace Sgcti.Core.Entities;

public class HistorialConsumo
{
    public int Id { get; set; }

    public int ImpresoraId { get; set; }

    public int ConsumibleId { get; set; }

    public DateTime FechaCambio { get; set; }

    public int PaginasImpresas { get; set; }

    public decimal Costo { get; set; }

    public Impresora Impresora { get; set; } = null!;

    public Consumible Consumible { get; set; } = null!;
}