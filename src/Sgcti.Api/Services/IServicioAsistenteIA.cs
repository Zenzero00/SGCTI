namespace Sgcti.Api.Services;

public interface IServicioAsistenteIA
{
    Task<string> ExtraerTextoPdfAsync(string rutaRelativa);

    Task<string> ConsultarChatbotAsync(string textoManual, string pregunta);
}
