namespace Sgcti.Core.Dtos;

public class BitacoraDto
{
    public int Id { get; set; }

    public string AnalistaId { get; set; } = string.Empty;

    public string DescripcionActividad { get; set; } = string.Empty;

    public string? EquipoIntervenido { get; set; }

    public DateTime HoraInicio { get; set; }

    public DateTime? HoraFin { get; set; }

    public string Etiqueta { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;
}