namespace Sgcti.Core.Entities;

public class RegistroConsumoDiario
{
    public int Id { get; set; }

    public int ImpresoraId { get; set; }

    public DateTime Fecha { get; set; }

    public int Contador { get; set; }

    public int Consumo { get; set; }

    public string? Observacion { get; set; }

    public Impresora Impresora { get; set; } = null!;
}