using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Sgcti.Api.Hubs;
using Sgcti.Core.Entities;
using Sgcti.Core.Services;
using Sgcti.Infrastructure.Persistence;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Sgcti.Api.Services;

public class ServicioTelegramBot : BackgroundService
{
    private const string TokenPrueba = "";

    private readonly IServiceScopeFactory _scopeFactory;

    private readonly IHubContext<NotificacionesHub> _hubContext;

    private readonly ILogger<ServicioTelegramBot> _logger;

    public ServicioTelegramBot(
        IServiceScopeFactory scopeFactory,
        IHubContext<NotificacionesHub> hubContext,
        ILogger<ServicioTelegramBot> logger)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var botClient = new TelegramBotClient(TokenPrueba);

            _logger.LogInformation("Bot de Telegram iniciado. Escuchando mensajes...");

            await botClient.ReceiveAsync(
                updateHandler: OnMessage,
                errorHandler: OnError,
                receiverOptions: new ReceiverOptions
                {
                    AllowedUpdates = Array.Empty<UpdateType>()
                },
                cancellationToken: stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "El bot de Telegram no pudo iniciar. Reemplaza el token de prueba ('{Token}') por el token real de BotFather.",
                TokenPrueba);
        }
    }

    private async Task OnMessage(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        if (update.Message is not { From: { } remitente } message)
        {
            return;
        }

        var chatId = message.Chat.Id.ToString(CultureInfo.InvariantCulture);
        var nombrePerfil = ObtenerNombrePerfil(remitente);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SgctiDbContext>();
            var departamentos = await dbContext.Departamentos.AsNoTracking().OrderBy(d => d.Nombre).ToListAsync(ct);

            // 1. Cuando el usuario presiona el botón "📱 Verificar mi número de teléfono"
            if (message.Type == MessageType.Contact && message.Contact is { } contacto)
            {
                await VerificarPorNumeroTelefonicoAsync(
                    botClient,
                    dbContext,
                    message,
                    contacto.PhoneNumber,
                    chatId,
                    departamentos,
                    ct);
                return;
            }

            if (message.Type != MessageType.Text || string.IsNullOrWhiteSpace(message.Text))
            {
                return;
            }

            var texto = message.Text.Trim();

            // 2. Comando para que el Técnico registre/configure el teléfono directamente desde Telegram:
            // Formato: /registrar Nombre Apellido | 0424-1234567 | Departamento | Cargo (opcional)
            if (TryParseComandoRegistrar(texto, out var nombreCmd, out var telefonoCmd, out var deptoCmd, out var cargoCmd, out var errorFormato))
            {
                if (!string.IsNullOrWhiteSpace(errorFormato))
                {
                    await botClient.SendMessage(message.Chat.Id, errorFormato, cancellationToken: ct);
                    return;
                }

                await RegistrarEmpleadoDesdeBotAsync(
                    botClient,
                    dbContext,
                    message,
                    chatId,
                    nombreCmd,
                    telefonoCmd,
                    deptoCmd,
                    cargoCmd,
                    departamentos,
                    ct);
                return;
            }

            // 3. Buscar si este ChatId ya tiene un número de teléfono verificado y activo en el sistema
            var usuario = await BuscarEmpleadoVerificadoPorChatIdAsync(dbContext, chatId, ct);

            if (texto.Equals("/start", StringComparison.OrdinalIgnoreCase)
                || texto.Equals("/ayuda", StringComparison.OrdinalIgnoreCase))
            {
                await EnviarSolicitudVerificacionTelefonicaAsync(botClient, message.Chat.Id, usuario, ct);
                return;
            }

            // 4. Si el empleado no está verificado, pedir verificación o comando de técnico
            if (usuario is null)
            {
                await EnviarSolicitudVerificacionTelefonicaAsync(botClient, message.Chat.Id, null, ct);
                return;
            }

            // 5. Crear el ticket automáticamente con el Departamento, Prioridad y Categoría del empleado verificado
            var categorias = await dbContext.CategoriasTicket.AsNoTracking().ToListAsync(ct);
            var titulo = $"Reporte de {usuario.NombreCompleto} (Vía Telegram)";
            var clasificacion = ClasificadorPrioridadTicket.Clasificar(
                usuario.Departamento,
                departamentos,
                titulo,
                texto,
                usuario.NombreCompleto);

            var departamento = Limpiar(usuario.Departamento) ?? clasificacion?.Departamento ?? "TI";
            var departamentoConfigurado = departamentos.FirstOrDefault(d =>
                string.Equals(d.Nombre, departamento, StringComparison.OrdinalIgnoreCase));

            var prioridad = usuario.PrioridadPersonalizada
                ?? departamentoConfigurado?.PrioridadPorDefecto
                ?? clasificacion?.Prioridad
                ?? PrioridadTicket.Media;

            var categoria = ClasificadorPrioridadTicket.ClasificarCategoria(
                null,
                categorias,
                titulo,
                texto);

            var ticket = new Ticket
            {
                Titulo = titulo,
                Descripcion = texto,
                Estado = EstadoTicket.Abierto,
                Prioridad = prioridad,
                Solicitante = usuario.NombreCompleto,
                TelegramChatId = chatId,
                Departamento = departamento,
                Categoria = categoria,
                FechaCreacion = DateTime.UtcNow,
                SLAHoras = 24
            };

            dbContext.Tickets.Add(ticket);
            await dbContext.SaveChangesAsync(ct);

            var todosLosTickets = await dbContext.Tickets.OrderBy(t => t.Id).ToListAsync(ct);
            GeneradorNomenclaturaTicket.RecalcularConsecutivosPorDepartamento(todosLosTickets, departamentos);
            await dbContext.SaveChangesAsync(ct);

            await _hubContext.Clients.All.SendAsync(
                "RecibirNotificacion",
                "Nuevo Ticket (Telegram)",
                $"Nuevo ticket {ticket.CodigoNomenclatura} de {usuario.NombreCompleto}",
                ct);

            var confirmacion =
                $"Ticket {ticket.CodigoNomenclatura} registrado.\n" +
                $"Solicitante: {usuario.NombreCompleto}\n" +
                $"Departamento: {departamento}\n" +
                $"Categoría: {categoria}\n" +
                $"Prioridad: {prioridad}";

            await botClient.SendMessage(
                message.Chat.Id,
                confirmacion,
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando mensaje de Telegram para {Nombre}.", nombrePerfil);
        }
    }

    private static async Task RegistrarEmpleadoDesdeBotAsync(
        ITelegramBotClient botClient,
        SgctiDbContext dbContext,
        Message message,
        string chatId,
        string nombre,
        string telefono,
        string departamentoSolicitado,
        string? cargo,
        IReadOnlyCollection<Departamento> departamentos,
        CancellationToken ct)
    {
        var digitos = FormatoTelegramEmpleado.NormalizarDigitosTelefono(telefono);
        if (digitos is null)
        {
            await botClient.SendMessage(
                message.Chat.Id,
                "❌ El número de teléfono no es válido. Ejemplo: /registrar Carlos Pérez | 0424-3666182 | Facturación | Analista",
                cancellationToken: ct);
            return;
        }

        var telefonoFormateado = FormatoTelegramEmpleado.FormatearTelefono(telefono) ?? telefono;
        var clasificacion = ClasificadorPrioridadTicket.Clasificar(departamentoSolicitado, departamentos);
        var departamentoFinal = clasificacion?.Departamento ?? departamentoSolicitado.Trim();

        var empleados = await dbContext.Usuarios
            .Where(u => u.Rol == Usuario.RolEmpleado)
            .ToListAsync(ct);

        var empleado = empleados.FirstOrDefault(u =>
        {
            var (telGuardado, chatGuardado) = FormatoTelegramEmpleado.Desglosar(u.TelegramUsuarioOId);
            return FormatoTelegramEmpleado.NormalizarDigitosTelefono(telGuardado) == digitos
                || string.Equals(chatGuardado, chatId, StringComparison.OrdinalIgnoreCase);
        });

        if (empleado is null)
        {
            empleado = new Usuario
            {
                NombreCompleto = nombre.Trim(),
                Username = await GenerarUsernameEmpleadoAsync(dbContext, nombre, ct),
                PasswordHash = string.Empty,
                Rol = Usuario.RolEmpleado,
                Activo = true,
                Cargo = Limpiar(cargo),
                Departamento = departamentoFinal,
                TelegramUsuarioOId = FormatoTelegramEmpleado.Combinar(telefonoFormateado, chatId)
            };
            dbContext.Usuarios.Add(empleado);
        }
        else
        {
            empleado.NombreCompleto = nombre.Trim();
            empleado.Departamento = departamentoFinal;
            if (!string.IsNullOrWhiteSpace(cargo))
            {
                empleado.Cargo = cargo.Trim();
            }
            empleado.Activo = true;
            empleado.TelegramUsuarioOId = FormatoTelegramEmpleado.Combinar(telefonoFormateado, chatId);
        }

        await dbContext.SaveChangesAsync(ct);

        var deptoConfig = departamentos.FirstOrDefault(d =>
            string.Equals(d.Nombre, empleado.Departamento, StringComparison.OrdinalIgnoreCase));
        var prioridadEfectiva = empleado.PrioridadPersonalizada
            ?? deptoConfig?.PrioridadPorDefecto
            ?? PrioridadTicket.Media;

        var respuesta =
            $"✅ Empleado registrado y vinculado en SGCTI:\n\n" +
            $"👤 Nombre: {empleado.NombreCompleto}\n" +
            $"💼 Cargo: {empleado.Cargo ?? "Empleado"}\n" +
            $"📞 Teléfono: {telefonoFormateado}\n" +
            $"🏢 Departamento: {empleado.Departamento}\n" +
            $"⚡ Prioridad asignada: {prioridadEfectiva}\n\n" +
            "El dispositivo ya quedó configurado. Cualquier mensaje que se envíe en este chat creará el ticket automáticamente.";

        await botClient.SendMessage(
            message.Chat.Id,
            respuesta,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: ct);
    }

    private static async Task VerificarPorNumeroTelefonicoAsync(
        ITelegramBotClient botClient,
        SgctiDbContext dbContext,
        Message message,
        string telefonoContacto,
        string chatId,
        IReadOnlyCollection<Departamento> departamentos,
        CancellationToken ct)
    {
        var digitos = FormatoTelegramEmpleado.NormalizarDigitosTelefono(telefonoContacto);
        var telefonoFormateado = FormatoTelegramEmpleado.FormatearTelefono(telefonoContacto) ?? telefonoContacto;

        var empleados = await dbContext.Usuarios
            .Where(u => u.Activo && u.Rol == Usuario.RolEmpleado)
            .ToListAsync(ct);

        var empleado = empleados.FirstOrDefault(u =>
        {
            var (telGuardado, _) = FormatoTelegramEmpleado.Desglosar(u.TelegramUsuarioOId);
            return digitos is not null && FormatoTelegramEmpleado.NormalizarDigitosTelefono(telGuardado) == digitos;
        });

        if (empleado is null)
        {
            var mensajeNoRegistrado =
                $"❌ Número no registrado en el sistema.\n\n" +
                $"Tu número de Telegram es: {telefonoFormateado}\n\n" +
                "• Opción 1 (En la Web): Pídele al Técnico de TI que coloque el número " +
                $"{telefonoFormateado} en la pestaña 'Empleados (Telegram)' y vuelve a presionar el botón de abajo.\n\n" +
                "• Opción 2 (Configuración directa por el Técnico en este chat):\n" +
                $"/registrar Nombre Apellido | {telefonoFormateado} | Departamento | Cargo";

            await botClient.SendMessage(
                message.Chat.Id,
                mensajeNoRegistrado,
                replyMarkup: ConstruirBotonVerificarTelefono(),
                cancellationToken: ct);
            return;
        }

        empleado.TelegramUsuarioOId = FormatoTelegramEmpleado.Combinar(telefonoFormateado, chatId);
        await dbContext.SaveChangesAsync(ct);

        var deptoConfig = departamentos.FirstOrDefault(d =>
            string.Equals(d.Nombre, empleado.Departamento, StringComparison.OrdinalIgnoreCase));
        var prioridadEfectiva = empleado.PrioridadPersonalizada
            ?? deptoConfig?.PrioridadPorDefecto
            ?? PrioridadTicket.Media;

        var textoExito =
            $"✅ ¡Teléfono verificado y vinculado exitosamente!\n" +
            $"👤 Empleado: {empleado.NombreCompleto}\n" +
            $"💼 Cargo: {empleado.Cargo ?? "No especificado"}\n" +
            $"📞 Teléfono: {telefonoFormateado}\n" +
            $"🏢 Departamento: {empleado.Departamento ?? "TI"}\n" +
            $"⚡ Prioridad asignada: {prioridadEfectiva}\n\n" +
            "A partir de ahora, cualquier problema que escribas en este chat creará tu ticket automáticamente.";

        await botClient.SendMessage(
            message.Chat.Id,
            textoExito,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: ct);
    }

    private static async Task EnviarSolicitudVerificacionTelefonicaAsync(
        ITelegramBotClient botClient,
        long chatId,
        Usuario? usuarioActual,
        CancellationToken ct)
    {
        if (usuarioActual is not null)
        {
            var (tel, _) = FormatoTelegramEmpleado.Desglosar(usuarioActual.TelegramUsuarioOId);
            var yaVinculado =
                $"✅ Este teléfono ya está vinculado a:\n" +
                $"👤 {usuarioActual.NombreCompleto}\n" +
                $"📞 Teléfono: {tel}\n" +
                $"🏢 Departamento: {usuarioActual.Departamento ?? "TI"}\n\n" +
                "Escribe directamente tu incidencia en el chat para generar un ticket.\n" +
                "(Si eres el Técnico de TI y deseas reconfigurar este teléfono, usa: /registrar Nombre | Teléfono | Departamento | Cargo)";

            await botClient.SendMessage(
                chatId,
                yaVinculado,
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: ct);
            return;
        }

        var mensaje =
            "⚠️ Dispositivo no vinculado en SGCTI.\n\n" +
            "1️⃣ Si ya estás registrado en el sistema, presiona el botón '📱 Verificar mi número de teléfono' aquí abajo.\n\n" +
            "2️⃣ Si eres el Técnico de TI configurando este teléfono por primera vez, puedes registrarlo directamente escribiendo:\n" +
            "/registrar Nombre Apellido | 0424-1234567 | Departamento | Cargo";

        await botClient.SendMessage(
            chatId,
            mensaje,
            replyMarkup: ConstruirBotonVerificarTelefono(),
            cancellationToken: ct);
    }

    private static bool TryParseComandoRegistrar(
        string texto,
        out string nombre,
        out string telefono,
        out string departamento,
        out string? cargo,
        out string? errorFormato)
    {
        nombre = string.Empty;
        telefono = string.Empty;
        departamento = string.Empty;
        cargo = null;
        errorFormato = null;

        var contenido = texto.Trim();
        var indiceEspacio = contenido.IndexOfAny([' ', '\t', '\r', '\n']);
        var comando = indiceEspacio < 0 ? contenido : contenido[..indiceEspacio];

        if (!comando.Equals("/registrar", StringComparison.OrdinalIgnoreCase)
            && !comando.Equals("/configurar", StringComparison.OrdinalIgnoreCase)
            && !comando.Equals("/soy", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var cuerpo = indiceEspacio < 0 ? string.Empty : contenido[(indiceEspacio + 1)..].Trim();
        var partes = cuerpo.Split('|', StringSplitOptions.TrimEntries);

        if (partes.Length < 3 || string.IsNullOrWhiteSpace(partes[0]) || string.IsNullOrWhiteSpace(partes[1]) || string.IsNullOrWhiteSpace(partes[2]))
        {
            errorFormato =
                "ℹ️ Formato para registrar empleado desde Telegram:\n" +
                "/registrar Nombre Apellido | 0424-3666182 | Departamento | Cargo (opcional)\n\n" +
                "Ejemplo:\n" +
                "/registrar Ana Torres | 0424-3666182 | Operaciones | Supervisora";
            return true;
        }

        nombre = partes[0];
        telefono = partes[1];
        departamento = partes[2];
        cargo = partes.Length >= 4 ? partes[3] : null;
        return true;
    }

    private static async Task<string> GenerarUsernameEmpleadoAsync(
        SgctiDbContext dbContext,
        string nombre,
        CancellationToken ct)
    {
        var descompuesto = nombre.Normalize(NormalizationForm.FormD);
        var limpio = new string(descompuesto
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark && char.IsLetterOrDigit(c))
            .Take(25)
            .ToArray())
            .ToLowerInvariant();

        if (limpio.Length < 3)
        {
            limpio = "empleado";
        }

        var candidato = limpio;
        var sufijo = 1;

        while (await dbContext.Usuarios.AnyAsync(u => u.Username == candidato, ct))
        {
            sufijo++;
            candidato = $"{limpio}{sufijo}";
        }

        return candidato;
    }

    private static ReplyKeyboardMarkup ConstruirBotonVerificarTelefono()
    {
        return new ReplyKeyboardMarkup(
            [
                [KeyboardButton.WithRequestContact("📱 Verificar mi número de teléfono")]
            ])
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };
    }

    private static async Task<Usuario?> BuscarEmpleadoVerificadoPorChatIdAsync(
        SgctiDbContext dbContext,
        string chatId,
        CancellationToken ct)
    {
        var empleados = await dbContext.Usuarios
            .Where(u => u.Activo && u.Rol == Usuario.RolEmpleado && u.TelegramUsuarioOId != null)
            .ToListAsync(ct);

        return empleados.FirstOrDefault(u =>
        {
            var (telefonoGuardado, chatGuardado) = FormatoTelegramEmpleado.Desglosar(u.TelegramUsuarioOId);
            return !string.IsNullOrWhiteSpace(telefonoGuardado)
                && string.Equals(chatGuardado, chatId, StringComparison.OrdinalIgnoreCase);
        });
    }

    private static string ObtenerNombrePerfil(Telegram.Bot.Types.User remitente)
    {
        var nombre = string.Join(
            ' ',
            new[] { remitente.FirstName, remitente.LastName }
                .Where(valor => !string.IsNullOrWhiteSpace(valor)));

        return string.IsNullOrWhiteSpace(nombre)
            ? remitente.Username ?? "Empleado de Telegram"
            : nombre;
    }

    private static string? Limpiar(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private Task OnError(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
    {
        if (!ct.IsCancellationRequested)
        {
            _logger.LogError(exception, "Error en el receiver del bot de Telegram.");
        }

        return Task.CompletedTask;
    }
}