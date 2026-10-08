using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using UglyToad.PdfPig;

namespace Sgcti.Api.Services;

public class ServicioAsistenteIA : IServicioAsistenteIA
{
    private const string MensajeError = "La IA no pudo procesar la consulta.";

    private readonly HttpClient _httpClient;

    private readonly IConfiguration _configuracion;

    public ServicioAsistenteIA(HttpClient httpClient, IConfiguration configuracion)
    {
        _httpClient = httpClient;
        _configuracion = configuracion;
    }

    public async Task<string> ExtraerTextoPdfAsync(string rutaRelativa)
    {
        var rutaAbsoluta = Path.Combine(Directory.GetCurrentDirectory(), rutaRelativa);

        if (!File.Exists(rutaAbsoluta))
        {
            throw new FileNotFoundException($"No se encontró el manual en la ruta {rutaRelativa}.");
        }

        return await Task.Run(() => ExtraerTexto(rutaAbsoluta));
    }

    public async Task<string> ConsultarChatbotAsync(string textoManual, string pregunta)
    {
        var apiKey = _configuracion["GeminiApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("ERROR GEMINI: falta GeminiApiKey en appsettings.json");
            return MensajeError;
        }

        var manual = textoManual ?? string.Empty;

        var promptCompletado =
            "Eres un asistente técnico para el SGCTI del CENDI. Responde ESTRICTAMENTE basándote en este manual. " +
            $"Si no sabes, dilo. Manual: {manual}. Pregunta: {pregunta}";

        try
        {
            var cuerpo = JsonSerializer.Serialize(new
            {
                contents = new[] { new { parts = new[] { new { text = promptCompletado } } } },
            });

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

            using var response = await _httpClient.PostAsync(
                url, new StringContent(cuerpo, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"ERROR GEMINI: {errorBody}");
                return errorBody;
            }

            var json = await response.Content.ReadAsStringAsync();

            var datos = JsonSerializer.Deserialize<RespuestaGemini>(
                json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var texto = datos?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

            return string.IsNullOrWhiteSpace(texto) ? MensajeError : texto;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR GEMINI: {ex.Message}");
            return MensajeError;
        }
    }

    private static string ExtraerTexto(string rutaAbsoluta)
    {
        var texto = new StringBuilder();

        using var documento = PdfDocument.Open(rutaAbsoluta);

        foreach (var pagina in documento.GetPages())
        {
            texto.AppendLine(pagina.Text);
        }

        return texto.ToString();
    }

    private sealed class RespuestaGemini
    {
        public CandidatoGemini[]? Candidates { get; set; }
    }

    private sealed class CandidatoGemini
    {
        public ContenidoGemini? Content { get; set; }
    }

    private sealed class ContenidoGemini
    {
        public ParteGemini[]? Parts { get; set; }
    }

    private sealed class ParteGemini
    {
        public string? Text { get; set; }
    }
}
