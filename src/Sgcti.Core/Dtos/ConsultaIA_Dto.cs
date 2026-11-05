namespace Sgcti.Core.Dtos;

public class ConsultaIA_Dto
{
    public string Pregunta { get; set; } = string.Empty;

    public List<MensajeHistorialDto>? Historial { get; set; }
}

public class MensajeHistorialDto
{
    public string Autor { get; set; } = string.Empty;

    public string Texto { get; set; } = string.Empty;
}
