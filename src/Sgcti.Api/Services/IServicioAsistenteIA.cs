using Sgcti.Core.Dtos;

namespace Sgcti.Api.Services;

public interface IServicioAsistenteIA
{
    Task<string> ExtraerTextoPdfAsync(string rutaRelativa);

    Task<string> ConsultarChatbotAsync(string textoManual, string pregunta);

    Task<string> ConsultarChatbotConContextoAsync(string pregunta, string contextoAdicional);

    Task<string> ConsultarChatbotConContextoAsync(string pregunta, string contextoAdicional, List<MensajeHistorialDto>? historial);
}
