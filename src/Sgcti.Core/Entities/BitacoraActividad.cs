namespace Sgcti.Core.Entities;

public class BitacoraActividad
{
    public int Id { get; set; }

    public string AnalistaId { get; set; } = string.Empty;

    public string DescripcionActividad { get; set; } = string.Empty;

    public string? EquipoIntervenido { get; set; }

    public DateTime HoraInicio { get; set; }

    public DateTime? HoraFin { get; set; }

    public string Etiqueta { get; set; } = EtiquetaRutina;

    public string Estado { get; set; } = EstadoPendiente;

    public const string EtiquetaUrgente = "Urgente";
    public const string EtiquetaRutina = "Rutina";
    public const string EtiquetaPendienteDeCompra = "Pendiente de Compra";

    public const string EstadoPendiente = "Pendiente";
    public const string EstadoCompletado = "Completado";
}