namespace Sgcti.Core.Entities;

public class BitacoraActividad
{
    public int Id { get; set; }

    public int AnalistaId { get; set; }

    public string DescripcionActividad { get; set; } = string.Empty;

    public string EquipoIntervenido { get; set; } = string.Empty;

    public DateTime HoraInicio { get; set; }

    public DateTime HoraFin { get; set; }

    public string Etiqueta { get; set; } = EtiquetaRutina;

    public EstadoActividad Estado { get; set; } = EstadoActividad.Pendiente;

    public const string EtiquetaUrgente = "Urgente";
    public const string EtiquetaRutina = "Rutina";
    public const string EtiquetaPendienteDeCompra = "Pendiente de Compra";
}

public enum EstadoActividad
{
    Pendiente,
    Completado
}