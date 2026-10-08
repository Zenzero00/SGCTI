namespace Sgcti.Core.Entities;

public class MantenimientoImpresora
{
    public int Id { get; set; }

    public int ImpresoraId { get; set; }

    public DateTime Fecha { get; set; }

    public string Tipo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string RealizadoPor { get; set; } = string.Empty;

    public Impresora Impresora { get; set; } = null!;

    public const string TipoPreventivo = "Preventivo";
    public const string TipoCorrectivo = "Correctivo";
}