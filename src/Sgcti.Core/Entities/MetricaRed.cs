namespace Sgcti.Core.Entities;

public class MetricaRed
{
    public int Id { get; set; }

    public string DispositivoIP { get; set; } = "0.0.0.0";

    public double LatenciaMS { get; set; }

    public int PaquetesPerdidos { get; set; }

    public DateTime FechaHora { get; set; }
}