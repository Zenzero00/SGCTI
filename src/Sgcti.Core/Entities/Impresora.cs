namespace Sgcti.Core.Entities;

public class Impresora
{
    public int Id { get; set; }

    public string Ip { get; set; } = "0.0.0.0";

    public string Modelo { get; set; } = string.Empty;

    public string Fabricante { get; set; } = string.Empty;

    public string Departamento { get; set; } = DepartamentoPorDefecto;

    public DateTime FechaInstalacion { get; set; }

    public EstadoPing EstadoPing { get; set; } = EstadoPing.Desconectado;

    public int LatenciaMs { get; set; }

    public string ComunidadSnmp { get; set; } = ComunidadSnmpPorDefecto;

    public string? ComputadorasVinculadas { get; set; }

    public string? RutaManual { get; set; }

    public int ContadorTotalPaginas { get; set; }

    public int ContadorMonocromo { get; set; }

    public int ContadorColor { get; set; }

    public int NivelTonnerNegro { get; set; }

    public int? NivelTonnerCian { get; set; }

    public int? NivelTonnerMagenta { get; set; }

    public int? NivelTonnerAmarillo { get; set; }

    public double PromedioPaginasDiario { get; set; }

    public double PromedioTonnerDiario { get; set; }

    public DateTime? UltimoMantenimiento { get; set; }

    public int PaginasDesdeUltimoMantenimiento { get; set; }

    public int DiasEstimadosAgotamientoTonner { get; set; }

    public DateTime? FechaEstimadaAgotamientoTonner { get; set; }

    public int DiasEstimadosMantenimiento { get; set; }

    public DateTime? FechaEstimadaMantenimiento { get; set; }

    public int? ConsumibleTonerId { get; set; }

    public Consumible? ConsumibleToner { get; set; }

    public bool AlertaStockCritico { get; set; }

    public ICollection<HistorialConsumo> HistorialConsumo { get; set; } = new List<HistorialConsumo>();

    public ICollection<RegistroConsumoDiario> RegistrosConsumoDiario { get; set; } = new List<RegistroConsumoDiario>();

    public ICollection<MantenimientoImpresora> MantenimientosImpresora { get; set; } = new List<MantenimientoImpresora>();

    public const string DepartamentoPorDefecto = "Facturación";

    public const string ComunidadSnmpPorDefecto = "public";
}

public enum EstadoPing
{
    EnLinea,
    Inestable,
    Desconectado
}