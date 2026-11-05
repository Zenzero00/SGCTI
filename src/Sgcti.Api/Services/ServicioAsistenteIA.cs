using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Sgcti.Core.Dtos;
using UglyToad.PdfPig;

namespace Sgcti.Api.Services;

public class ServicioAsistenteIA : IServicioAsistenteIA
{
    private const string MensajeError = "La IA no pudo procesar la consulta.";

    private readonly HttpClient _httpClient;

    private readonly IConfiguration _configuracion;

    private const string InstruccionesSistema = @"Hola, soy el Asistente Inteligente TI integrado en SGCTI.
¿Para qué sirvo? Te ayudo a dar soporte TI más rápido consultando inventario, impresoras, manuales técnicos, suministros y guiando a los técnicos en la configuración del Bot de Telegram y la Mesa de Ayuda.
¿Cómo funciono? Uso el contexto del sistema (suministros/consumibles, impresoras, manuales PDF cargados y el manual operativo de SGCTI) para responder con datos reales.

Te ayudo a:
1) Verificar en tiempo real si aún quedan ciertos artículos, tóneres o repuestos en el inventario y cuáles están en nivel crítico.
2) Consultar el estado y niveles de las impresoras en red.
3) Leer e interpretar los manuales técnicos para resolver fallas o códigos de error (puedes subirlos en PDF con el botón 'Cargar Manual').
4) Explicar paso a paso a los técnicos de TI cómo configurar y vincular los teléfonos de los empleados en el Bot de Telegram (tanto desde la web como por comandos en el teléfono) y cómo operar la Mesa de Ayuda.

=== MANUAL OPERATIVO DEL BOT DE TELEGRAM Y MESA DE AYUDA (CONOCIMIENTO OBLIGATORIO) ===
Cuando un técnico o usuario pregunte cómo agregar, configurar o vincular a un empleado en Telegram, debes explicarle que existen DOS MÉTODOS oficiales:

MÉTODO 1: Configuración desde el Sistema Web (Recomendado para administración)
1. En el menú lateral izquierdo, entra a 'Usuarios / Configuración' y selecciona la pestaña 'Empleados (Telegram)'.
2. Haz clic en el botón 'Nuevo Empleado' (o en 'Editar' si el empleado ya está en la lista).
3. Completa sus datos:
   - Nombre Completo (ej. Carlos Pérez).
   - Cargo (ej. Analista de Facturación).
   - Departamento (ej. Facturación, Gerencia, Almacén, TI, Operaciones).
   - Prioridad Personalizada (opcional: si se deja vacío, el sistema le asigna automáticamente la prioridad de su departamento).
   - Número de Teléfono (ej. 0424-3666182).
4. Luego, en el teléfono del empleado, abre el Bot de Soporte TI en Telegram, escribe /start y presiona el botón inferior '📱 Verificar mi número de teléfono'.
5. El bot comprobará que el número de su línea coincida con el registrado en el sistema y cambiará su estado en la tabla a 'Vinculado ✓'.

MÉTODO 2: Configuración directa desde el teléfono por el Técnico de TI (Comando rápido en Telegram)
Si el técnico tiene el teléfono del nuevo empleado en la mano y quiere dejarlo listo sin ir a la computadora:
1. Abre el chat del Bot de Soporte TI en el Telegram del teléfono del empleado.
2. Escribe el comando directo con este formato exacto separado por barras verticales (|):
   /registrar Nombre Apellido | Teléfono | Departamento | Cargo
   Ejemplo real:
   /registrar Ana Torres | 0424-3666182 | Operaciones | Supervisora
   (También acepta los alias /configurar o /soy con el mismo formato).
3. Al enviar ese comando, el bot crea o actualiza automáticamente al empleado en la base de datos de SGCTI, le asigna su departamento, guarda su número de teléfono y deja el dispositivo 'Vinculado ✓' en un solo paso.
4. A partir de ese momento, cualquier mensaje que el empleado escriba en el chat (ej. 'La impresora no quiere imprimir' o 'Necesito un teclado y un cable USB') creará el ticket automáticamente en la Mesa de Ayuda.

FUNCIONAMIENTO DE CÓDIGOS, CATEGORÍAS Y ALERTAS EN LA MESA DE AYUDA:
- Código de Nomenclatura por Departamento: Cada departamento tiene su propio contador independiente (ej. GER-001, GER-002 para Gerencia; TI-001, TI-002 para TI; FAC-001 para Facturación). Las letras indican el departamento de origen y el número indica el consecutivo histórico de solicitudes de ese departamento (si supera 999 continúa en 1000, 1001, etc. sin romperse).
- Detección Automática de Categoría: El bot analiza el texto del reporte en Telegram y asigna la categoría automáticamente ('Impresoras', 'Periféricos', 'Red', 'Software' o 'Hardware'). Si el técnico desea cambiarla manualmente, puede abrir el botón 'Detalles' del ticket en la Mesa de Ayuda.
- Sistema de Alertas Sonoras y Silenciamiento: La Mesa de Ayuda emite recordatorios sonoros automáticos para los tickets abiertos según su prioridad:
  * Prioridad Crítica: cada 1 minuto.
  * Prioridad Alta: cada 3 minutos.
  * Prioridad Media: cada 10 minutos.
  * Prioridad Baja: cada 30 minutos.
- ¿Cómo silenciar las alertas si el técnico ya está trabajando en el caso?:
  1) Por ticket: haciendo clic en el botón '🔔 Alerta / 🔕 Silenciado' en la fila del ticket, o entrando en 'Detalles' y cambiando el estado a 'En Progreso'.
  2) Global: haciendo clic en el botón superior '🔔 Sonido Activo / 🔕 Alertas Silenciadas' en la cabecera de la Mesa de Ayuda.
- Eliminar Tickets: Si un empleado envía mensajes repetidos o por error desde Telegram, el técnico puede borrarlos con el botón rojo 'Eliminar' en la fila del ticket.

Si me pides un manual técnico o drivers de una impresora (por ejemplo, HP Smart Tank 210-220), DEBO proporcionarte directamente el enlace oficial de soporte del fabricante (ej. HP, Epson, Canon, Brother, Xerox). Una vez que te dé el enlace, te explicaré que debes descargar el archivo PDF desde ese link y luego subirlo aquí mismo usando el botón 'Cargar Manual' para que pueda leerlo e interpretarlo.

Siempre respondo en primera persona, en español, claro, directo y práctico.";

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
        return await ConsultarChatbotConContextoAsync(
            pregunta,
            string.IsNullOrWhiteSpace(textoManual) ? string.Empty : $"MANUALES PDF:\n{textoManual}",
            null);
    }

    public Task<string> ConsultarChatbotConContextoAsync(string pregunta, string contextoAdicional)
    {
        return ConsultarChatbotConContextoAsync(pregunta, contextoAdicional, null);
    }

    public async Task<string> ConsultarChatbotConContextoAsync(
        string pregunta,
        string contextoAdicional,
        List<MensajeHistorialDto>? historial)
    {
        if (EsSaludoOPreguntaFunciones(pregunta))
        {
            return ObtenerRespuestaSaludoFunciones();
        }

        if (IntentarRespuestaGuiaOperativaLocal(pregunta, out var respuestaGuiaLocal))
        {
            return respuestaGuiaLocal;
        }

        var apiKey = _configuracion["GeminiApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return ObtenerRespuestaRespaldo(pregunta, contextoAdicional);
        }

        var contexto = contextoAdicional ?? string.Empty;

        var contenido = new List<object>
        {
            new
            {
                role = "user",
                parts = new[]
                {
                    new { text = InstruccionesSistema + "\n\n=== CONTEXTO DEL SISTEMA SGCTI ===\n" + contexto }
                }
            }
        };

        if (historial != null)
        {
            foreach (var h in historial)
            {
                if (string.IsNullOrWhiteSpace(h.Texto)) continue;
                var rol = string.Equals(h.Autor, "usuario", StringComparison.OrdinalIgnoreCase) ? "user" : "model";
                contenido.Add(new { role = rol, parts = new[] { new { text = h.Texto } } });
            }
        }

        contenido.Add(new
        {
            role = "user",
            parts = new[]
            {
                new
                {
                    text = "=== CONSULTA DEL USUARIO ===\n" + pregunta +
                           "\n\nResponde de forma clara y estructurada en español usando las instrucciones operativas de SGCTI y el contexto del sistema."
                }
            }
        });

        try
        {
            var cuerpo = JsonSerializer.Serialize(new
            {
                contents = contenido
            });

            var url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key=" + apiKey;

            using var response = await _httpClient.PostAsync(
                url, new StringContent(cuerpo, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                return ObtenerRespuestaRespaldo(pregunta, contextoAdicional);
            }

            var json = await response.Content.ReadAsStringAsync();

            var datos = JsonSerializer.Deserialize<RespuestaGemini>(
                json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var texto = datos?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

            if (string.IsNullOrWhiteSpace(texto))
            {
                return ObtenerRespuestaRespaldo(pregunta, contextoAdicional);
            }

            return texto;
        }
        catch (Exception)
        {
            return ObtenerRespuestaRespaldo(pregunta, contextoAdicional);
        }
    }

    private static bool EsSaludoOPreguntaFunciones(string pregunta)
    {
        if (string.IsNullOrWhiteSpace(pregunta)) return false;
        var p = pregunta.Trim().ToLowerInvariant();
        if (p == "hola" || p == "ayuda" || p.Contains("que puedes hacer") || p.Contains("qué puedes hacer")) return true;
        if ((p.StartsWith("hola") || p.StartsWith("buenas")) && (p.Contains("que") || p.Contains("puedes") || p.Contains("funcion"))) return true;
        if (p.Contains("que puedes") || p.Contains("qué puedes")) return true;
        return false;
    }

    private static bool IntentarRespuestaGuiaOperativaLocal(string pregunta, out string respuesta)
    {
        respuesta = string.Empty;
        if (string.IsNullOrWhiteSpace(pregunta))
        {
            return false;
        }

        var p = pregunta.Trim().ToLowerInvariant();

        var preguntaTelegram = p.Contains("telegram")
            || p.Contains("/registrar")
            || p.Contains("/configurar")
            || p.Contains("vincular")
            || (p.Contains("empleado") && (p.Contains("telefono") || p.Contains("teléfono") || p.Contains("bot") || p.Contains("agregar") || p.Contains("registrar")))
            || (p.Contains("bot") && (p.Contains("como") || p.Contains("cómo") || p.Contains("comando") || p.Contains("usar") || p.Contains("configurar")));

        if (preguntaTelegram)
        {
            respuesta =
                "📱 **Guía para Técnicos TI: Cómo configurar y vincular empleados en el Bot de Telegram**\n\n" +
                "Existen **2 maneras** de registrar y vincular el teléfono de un empleado para que pueda reportar incidencias por Telegram:\n\n" +
                "---\n" +
                "**Opción 1: Directamente desde el teléfono del empleado (Comando rápido para el Técnico)**\n" +
                "Si tienes el teléfono del empleado en la mano, abre el chat con el Bot de Soporte TI y envía este comando separando los datos con `|`:\n\n" +
                "`/registrar Nombre Apellido | Teléfono | Departamento | Cargo`\n\n" +
                "• **Ejemplo práctico:**\n" +
                "`/registrar Ana Torres | 0424-3666182 | Operaciones | Supervisora`\n\n" +
                "*(El bot creará al empleado en la base de datos con su teléfono, departamento y prioridad, y dejará el dispositivo **Vinculado ✓** de inmediato).* \n\n" +
                "---\n" +
                "**Opción 2: Desde el Sistema Web SGCTI (Panel de Configuración)**\n" +
                "1. Ve al menú lateral **Usuarios / Configuración** y entra en la pestaña **Empleados (Telegram)**.\n" +
                "2. Haz clic en **Nuevo Empleado** (o en **Editar** si ya existe).\n" +
                "3. Ingresa su **Nombre Completo**, **Cargo**, **Departamento**, **Prioridad** (opcional) y su **Número de Teléfono** (ej. `0424-3666182`).\n" +
                "4. En el teléfono del empleado, abre el bot de Telegram, escribe `/start` y toca el botón inferior **\"📱 Verificar mi número de teléfono\"**.\n" +
                "5. Si el número del teléfono coincide con el registrado en la web, quedará **Vinculado ✓** y cualquier mensaje que envíe creará su ticket automáticamente con su departamento, prioridad y categoría.";
            return true;
        }

        var preguntaAlertasOCodigos = (p.Contains("alerta") || p.Contains("sonido") || p.Contains("silenciar") || p.Contains("nomenclatura") || p.Contains("codigo") || p.Contains("código"))
            && (p.Contains("ticket") || p.Contains("mesa de ayuda") || p.Contains("prioridad") || p.Contains("departamento") || p.Contains("como") || p.Contains("cómo") || p.Contains("para que") || p.Contains("para qué"));

        if (preguntaAlertasOCodigos)
        {
            respuesta =
                "🎫 **Funcionamiento de Códigos, Categorías y Alertas en la Mesa de Ayuda**\n\n" +
                "1) **Código de Nomenclatura por Departamento (`GER-001`, `TI-002`, `FAC-001`):**\n" +
                "   • Las siglas iniciales identifican el departamento que solicita el soporte y los dígitos llevan el **conteo histórico propio de ese departamento**.\n" +
                "   • Si un departamento supera las 999 solicitudes, el código continúa automáticamente en `1000`, `1001`, etc., sin romperse.\n\n" +
                "2) **Detección Automática de Categoría y Prioridad:**\n" +
                "   • Cuando entra un reporte por Telegram, el sistema analiza las palabras clave y le asigna la categoría (*Hardware*, *Software*, *Impresoras*, *Periféricos* o *Red*) y la prioridad del empleado o de su departamento.\n" +
                "   • Puedes modificar el Departamento, Categoría, Estado o Prioridad en cualquier momento desde el botón **Detalles**.\n\n" +
                "3) **Alertas Sonoras según Prioridad y cómo Silenciarlas:**\n" +
                "   • Los tickets abiertos emiten recordatorios sonoros según su urgencia: **Crítica** (cada 1 min), **Alta** (cada 3 min), **Media** (cada 10 min) y **Baja** (cada 30 min).\n" +
                "   • **Para silenciar un ticket mientras trabajas en él:** presiona el botón **🔔 Alerta / 🔕 Silenciado** en la fila del ticket, o entra a **Detalles** y cámbialo a estado **En Progreso**.\n" +
                "   • **Para silenciar todo el módulo:** usa el botón **🔔 Sonido Activo / 🔕 Alertas Silenciadas** arriba a la derecha en la Mesa de Ayuda.";
            return true;
        }

        return false;
    }

    private static string ObtenerRespuestaSaludoFunciones()
    {
        return "Hola, soy el Asistente Inteligente TI integrado en la plataforma SGCTI.\n\n" +
               "¿Para qué sirvo?\n" +
               "Te ayudo a gestionar el soporte técnico más rápido consultando inventario, impresoras, manuales técnicos y guiándote en el uso del sistema.\n\n" +
               "Puedo ayudarte a:\n" +
               "1) Verificar en tiempo real el stock de suministros, tóneres o repuestos y cuáles están en nivel crítico.\n" +
               "2) Consultar el estado, IP y niveles de tóner de las impresoras en red.\n" +
               "3) Leer e interpretar manuales técnicos en PDF (puedes subirlos con el botón 'Cargar Manual').\n" +
               "4) Explicarte paso a paso cómo vincular teléfonos de empleados en el Bot de Telegram (desde la web o con el comando `/registrar` en el teléfono) y cómo usar las alertas y códigos de la Mesa de Ayuda.";
    }

    private static string ObtenerRespuestaRespaldo(string pregunta, string? contextoAdicional)
    {
        if (EsSaludoOPreguntaFunciones(pregunta))
        {
            return ObtenerRespuestaSaludoFunciones();
        }

        if (IntentarRespuestaGuiaOperativaLocal(pregunta, out var guiaLocal))
        {
            return guiaLocal;
        }

        if (string.IsNullOrWhiteSpace(contextoAdicional))
        {
            return "Puedes consultarme sobre:\n" +
                   "• Cómo vincular un empleado o teléfono en el Bot de Telegram (`/registrar`).\n" +
                   "• Estado de suministros e inventario en nivel crítico.\n" +
                   "• Estado y niveles de las impresoras en red.\n" +
                   "• Lectura de manuales técnicos en PDF con el botón 'Cargar Manual'.";
        }

        var ctx = contextoAdicional!;
        if (ctx.Length > 1800)
        {
            ctx = ctx[..1800] + "\n...";
        }

        return "Aquí tienes la información actual registrada en el sistema SGCTI para tu consulta:\n\n" + ctx;
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