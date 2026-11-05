namespace Sgcti.Core.Dtos;

public class ConteoEstadisticaDto
{
    public string Etiqueta { get; set; } = string.Empty;

    public int Total { get; set; }
}

public class ResumenEstadosTicketsDto
{
    public int Total { get; set; }

    public int Resueltos { get; set; }

    public int Abiertos { get; set; }

    public int EnProgreso { get; set; }
}

public class EquipoRecurrenteDto
{
    public string Equipo { get; set; } = string.Empty;

    public int Total { get; set; }

    public int Resueltos { get; set; }

    public int Abiertos { get; set; }

    public DateTime? UltimaIncidencia { get; set; }

    public List<string> Soluciones { get; set; } = [];
}

public class EstadisticasTicketsDto
{
    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public ResumenEstadosTicketsDto Resumen { get; set; } = new();

    public List<ConteoEstadisticaDto> PorDepartamento { get; set; } = [];

    public List<ConteoEstadisticaDto> PorCategoria { get; set; } = [];

    public List<EquipoRecurrenteDto> EquiposRecurrentes { get; set; } = [];
}
