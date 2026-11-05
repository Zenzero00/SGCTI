using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Sgcti.Api.Hubs;
using Sgcti.Core.Abstractions;
using Sgcti.Core.Dtos;
using Sgcti.Core.Entities;
using Sgcti.Core.Services;
using Sgcti.Infrastructure.Persistence;

namespace Sgcti.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly ITicketRepositorio _ticketRepositorio;

    private readonly IHubContext<NotificacionesHub> _hubContext;

    private readonly SgctiDbContext _db;

    public TicketsController(
        ITicketRepositorio ticketRepositorio,
        IHubContext<NotificacionesHub> hubContext,
        SgctiDbContext db)
    {
        _ticketRepositorio = ticketRepositorio;
        _hubContext = hubContext;
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetAll(CancellationToken ct)
    {
        await CompletarCamposTicketsExistentesAsync(ct);
        var tickets = await _ticketRepositorio.ObtenerTodosAsync(ct);
        return Ok(tickets.Select(ADto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TicketDto>> GetById(int id, CancellationToken ct)
    {
        var ticket = await _ticketRepositorio.ObtenerPorIdAsync(id, ct);
        if (ticket is null)
        {
            return NotFound();
        }

        return Ok(ADto(ticket));
    }

    [HttpPost]
    public async Task<ActionResult<TicketDto>> Create(CrearTicketDto dto, CancellationToken ct)
    {
        var departamentos = await _db.Departamentos.AsNoTracking().ToListAsync(ct);
        var categorias = await _db.CategoriasTicket.AsNoTracking().ToListAsync(ct);
        var solicitante = Limpiar(dto.Solicitante);

        var empleado = string.IsNullOrWhiteSpace(solicitante)
            ? null
            : await _db.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    u => u.NombreCompleto.ToLower() == solicitante.ToLower(),
                    ct);

        var departamentoSolicitado = Limpiar(dto.Departamento)
            ?? Limpiar(empleado?.Departamento);

        var clasificacion = ClasificadorPrioridadTicket.Clasificar(
            departamentoSolicitado,
            departamentos,
            dto.Titulo,
            dto.Descripcion);

        var prioridad = empleado?.PrioridadPersonalizada
            ?? clasificacion?.Prioridad
            ?? (PrioridadTicket)dto.Prioridad;

        var departamento = !string.IsNullOrWhiteSpace(dto.Departamento)
            ? dto.Departamento.Trim()
            : clasificacion?.Departamento ?? Limpiar(empleado?.Departamento);

        var categoria = ClasificadorPrioridadTicket.ClasificarCategoria(
            dto.Categoria,
            categorias,
            dto.Titulo,
            dto.Descripcion);

        var ticket = new Ticket
        {
            Titulo = dto.Titulo.Trim(),
            Descripcion = dto.Descripcion?.Trim() ?? string.Empty,
            Estado = (EstadoTicket)dto.Estado,
            Prioridad = prioridad,
            FechaCreacion = dto.FechaCreacion == default ? DateTime.UtcNow : dto.FechaCreacion,
            SLAHoras = dto.SLAHoras,
            Solicitante = solicitante,
            TelegramChatId = Limpiar(dto.TelegramChatId),
            Departamento = departamento,
            Categoria = categoria,
            EquipoAfectado = dto.EquipoAfectado?.Trim(),
            SolucionAplicada = dto.SolucionAplicada?.Trim(),
            SuministrosUtilizados = dto.SuministrosUtilizados?.Trim(),
            FechaResolucion = dto.FechaResolucion
        };

        var creado = await _ticketRepositorio.AgregarAsync(ticket, ct);

        var todosLosTickets = await _db.Tickets.OrderBy(t => t.Id).ToListAsync(ct);
        GeneradorNomenclaturaTicket.RecalcularConsecutivosPorDepartamento(todosLosTickets, departamentos);
        await _db.SaveChangesAsync(ct);

        await _hubContext.Clients.All.SendAsync(
            "RecibirNotificacion",
            "Nuevo Ticket",
            $"Se ha creado el ticket {creado.CodigoNomenclatura}: {creado.Titulo}",
            ct);

        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, ADto(creado));
    }

    [HttpPost("{id:int}/alerta-recordatorio")]
    public async Task<IActionResult> EnviarAlertaRecordatorio(
        int id,
        [FromBody] RecordatorioTicketDto? dto,
        CancellationToken ct)
    {
        var ticket = await _ticketRepositorio.ObtenerPorIdAsync(id, ct);
        if (ticket is null || ticket.Estado == EstadoTicket.Resuelto)
        {
            return NotFound();
        }

        var codigo = string.IsNullOrWhiteSpace(ticket.CodigoNomenclatura)
            ? $"#{ticket.Id}"
            : ticket.CodigoNomenclatura;
        var solicitante = string.IsNullOrWhiteSpace(ticket.Solicitante)
            ? "Empleado"
            : ticket.Solicitante;
        var depto = string.IsNullOrWhiteSpace(ticket.Departamento)
            ? "Sin departamento"
            : ticket.Departamento;
        var minutos = dto?.MinutosEspera is > 0
            ? dto.MinutosEspera
            : Math.Max(1, (int)Math.Round((DateTime.UtcNow - ticket.FechaCreacion).TotalMinutes));

        await _hubContext.Clients.All.SendAsync(
            "RecibirNotificacion",
            $"⏰ Alerta Prioridad {ticket.Prioridad} ({codigo})",
            $"El ticket {codigo} de {solicitante} ({depto}) lleva {minutos} min sin resolver.",
            ct);

        return NoContent();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ActualizarTicketDto dto, CancellationToken ct)
    {
        var ticketExistente = await _ticketRepositorio.ObtenerPorIdAsync(id, ct);
        if (ticketExistente is null)
        {
            return NotFound();
        }

        var nuevoEstado = (EstadoTicket)dto.Estado;
        var departamentos = await _db.Departamentos.AsNoTracking().ToListAsync(ct);
        var categorias = await _db.CategoriasTicket.AsNoTracking().ToListAsync(ct);

        ticketExistente.Titulo = dto.Titulo.Trim();
        ticketExistente.Descripcion = dto.Descripcion?.Trim() ?? string.Empty;
        ticketExistente.Estado = nuevoEstado;
        ticketExistente.Prioridad = (PrioridadTicket)dto.Prioridad;
        ticketExistente.SLAHoras = dto.SLAHoras;
        ticketExistente.FechaCierre = dto.FechaCierre;
        ticketExistente.Solicitante = Limpiar(dto.Solicitante);
        ticketExistente.TelegramChatId = Limpiar(dto.TelegramChatId) ?? ticketExistente.TelegramChatId;
        ticketExistente.Departamento = Limpiar(dto.Departamento);
        ticketExistente.Categoria = ClasificadorPrioridadTicket.ClasificarCategoria(
            dto.Categoria,
            categorias,
            dto.Titulo,
            dto.Descripcion);
        ticketExistente.EquipoAfectado = dto.EquipoAfectado?.Trim();
        ticketExistente.SolucionAplicada = dto.SolucionAplicada?.Trim();
        ticketExistente.SuministrosUtilizados = dto.SuministrosUtilizados?.Trim();
        ticketExistente.FechaResolucion = dto.FechaResolucion ?? ticketExistente.FechaResolucion;

        if (nuevoEstado == EstadoTicket.Resuelto)
        {
            ticketExistente.FechaResolucion ??= DateTime.UtcNow;
            ticketExistente.FechaCierre ??= DateTime.UtcNow;
        }

        await _ticketRepositorio.ActualizarAsync(ticketExistente, ct);

        var todosLosTickets = await _db.Tickets.OrderBy(t => t.Id).ToListAsync(ct);
        if (GeneradorNomenclaturaTicket.RecalcularConsecutivosPorDepartamento(todosLosTickets, departamentos))
        {
            await _db.SaveChangesAsync(ct);
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var existente = await _ticketRepositorio.ObtenerPorIdAsync(id, ct);
        if (existente is null)
        {
            return NotFound();
        }

        await _ticketRepositorio.EliminarAsync(id, ct);
        return NoContent();
    }

    [HttpGet("estadisticas")]
    public async Task<ActionResult<EstadisticasTicketsDto>> Estadisticas(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken ct)
    {
        if (desde.HasValue && hasta.HasValue && desde.Value.Date > hasta.Value.Date)
        {
            return BadRequest("La fecha de inicio no puede ser posterior a la fecha de fin.");
        }

        await CompletarCamposTicketsExistentesAsync(ct);
        var tickets = await ObtenerTicketsFiltradosAsync(desde, hasta, ct);
        var resueltos = tickets.Count(t => t.Estado == EstadoTicket.Resuelto);

        return Ok(new EstadisticasTicketsDto
        {
            FechaInicio = desde?.Date,
            FechaFin = hasta?.Date,
            Resumen = new ResumenEstadosTicketsDto
            {
                Total = tickets.Count,
                Resueltos = resueltos,
                Abiertos = tickets.Count(t => t.Estado == EstadoTicket.Abierto),
                EnProgreso = tickets.Count(t => t.Estado == EstadoTicket.EnProgreso)
            },
            PorDepartamento = tickets
                .GroupBy(t => Etiqueta(t.Departamento, "Sin departamento"))
                .Select(g => new ConteoEstadisticaDto { Etiqueta = g.Key, Total = g.Count() })
                .OrderByDescending(g => g.Total)
                .ThenBy(g => g.Etiqueta)
                .ToList(),
            PorCategoria = tickets
                .GroupBy(t => Etiqueta(t.Categoria, "Sin categoría"))
                .Select(g => new ConteoEstadisticaDto { Etiqueta = g.Key, Total = g.Count() })
                .OrderByDescending(g => g.Total)
                .ThenBy(g => g.Etiqueta)
                .ToList(),
            EquiposRecurrentes = tickets
                .Where(t => !string.IsNullOrWhiteSpace(t.EquipoAfectado))
                .GroupBy(t => t.EquipoAfectado!.Trim())
                .Select(g =>
                {
                    var resueltosEquipo = g.Count(t => t.Estado == EstadoTicket.Resuelto);

                    return new EquipoRecurrenteDto
                    {
                        Equipo = g.Key,
                        Total = g.Count(),
                        Resueltos = resueltosEquipo,
                        Abiertos = g.Count() - resueltosEquipo,
                        UltimaIncidencia = g.Max(t => t.FechaCreacion),
                        Soluciones = g
                            .Where(t => !string.IsNullOrWhiteSpace(t.SolucionAplicada))
                            .Select(t => t.SolucionAplicada!.Trim())
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .Take(5)
                            .ToList()
                    };
                })
                .OrderByDescending(e => e.Total)
                .ThenByDescending(e => e.UltimaIncidencia)
                .Take(15)
                .ToList()
        });
    }

    [HttpGet("reporte-incidencias")]
    public async Task<IActionResult> ReporteIncidencias(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken ct)
    {
        if (desde.HasValue && hasta.HasValue && desde.Value.Date > hasta.Value.Date)
        {
            return BadRequest("La fecha de inicio no puede ser posterior a la fecha de fin.");
        }

        await CompletarCamposTicketsExistentesAsync(ct);
        var tickets = await ObtenerTicketsFiltradosAsync(desde, hasta, ct);
        var contenido = new StringBuilder();

        contenido.Append('\uFEFF');
        contenido.AppendLine(string.Join(';',
            "ID",
            "Código",
            "Fecha de apertura",
            "Fecha de cierre",
            "Solicitante",
            "Departamento",
            "Categoría",
            "Título",
            "Problema original",
            "Equipo afectado",
            "Solución aplicada",
            "Suministros utilizados",
            "Estado",
            "Prioridad"));

        foreach (var ticket in tickets)
        {
            contenido.AppendLine(string.Join(';',
                Campo(ticket.Id.ToString(CultureInfo.InvariantCulture)),
                Campo(ticket.CodigoNomenclatura),
                Campo(FormatearFecha(ticket.FechaCreacion)),
                Campo(FormatearFecha(ticket.FechaCierre)),
                Campo(ticket.Solicitante),
                Campo(ticket.Departamento),
                Campo(ticket.Categoria),
                Campo(ticket.Titulo),
                Campo(ticket.Descripcion),
                Campo(ticket.EquipoAfectado),
                Campo(ticket.SolucionAplicada),
                Campo(ticket.SuministrosUtilizados),
                Campo(ticket.Estado.ToString()),
                Campo(ticket.Prioridad.ToString())));
        }

        var nombreArchivo = $"Reporte_Incidencias_{DateTime.Now:yyyyMMdd_HHmm}.csv";
        return File(Encoding.UTF8.GetBytes(contenido.ToString()), "text/csv; charset=utf-8", nombreArchivo);
    }

    private async Task CompletarCamposTicketsExistentesAsync(CancellationToken ct)
    {
        var todosLosTickets = await _db.Tickets
            .OrderBy(t => t.Id)
            .ToListAsync(ct);

        if (todosLosTickets.Count == 0)
        {
            return;
        }

        var departamentos = await _db.Departamentos.AsNoTracking().ToListAsync(ct);
        var categorias = await _db.CategoriasTicket.AsNoTracking().ToListAsync(ct);
        var empleados = await _db.Usuarios
            .AsNoTracking()
            .Where(u => u.Rol == Usuario.RolEmpleado)
            .ToListAsync(ct);

        var huboCambios = false;

        foreach (var ticket in todosLosTickets)
        {
            if (string.IsNullOrWhiteSpace(ticket.Categoria))
            {
                ticket.Categoria = ClasificadorPrioridadTicket.ClasificarCategoria(
                    null,
                    categorias,
                    ticket.Titulo,
                    ticket.Descripcion);
                huboCambios = true;
            }

            if (string.IsNullOrWhiteSpace(ticket.Departamento))
            {
                var empleado = string.IsNullOrWhiteSpace(ticket.Solicitante)
                    ? null
                    : empleados.FirstOrDefault(e =>
                        string.Equals(e.NombreCompleto, ticket.Solicitante, StringComparison.OrdinalIgnoreCase));

                ticket.Departamento = Limpiar(empleado?.Departamento) ?? "TI";
                huboCambios = true;
            }
        }

        if (GeneradorNomenclaturaTicket.RecalcularConsecutivosPorDepartamento(todosLosTickets, departamentos))
        {
            huboCambios = true;
        }

        if (huboCambios)
        {
            await _db.SaveChangesAsync(ct);
        }
    }

    private async Task<List<Ticket>> ObtenerTicketsFiltradosAsync(
        DateTime? desde,
        DateTime? hasta,
        CancellationToken ct)
    {
        var consulta = _db.Tickets.AsNoTracking();

        if (desde.HasValue)
        {
            var inicio = DateTime.SpecifyKind(desde.Value.Date, DateTimeKind.Utc);
            consulta = consulta.Where(t => t.FechaCreacion >= inicio);
        }

        if (hasta.HasValue)
        {
            var fin = DateTime.SpecifyKind(hasta.Value.Date, DateTimeKind.Utc).AddDays(1).AddTicks(-1);
            consulta = consulta.Where(t => t.FechaCreacion <= fin);
        }

        return await consulta
            .OrderByDescending(t => t.FechaCreacion)
            .ToListAsync(ct);
    }

    private static string Etiqueta(string? valor, string respaldo)
        => string.IsNullOrWhiteSpace(valor) ? respaldo : valor.Trim();

    private static string FormatearFecha(DateTime? fecha)
        => fecha.HasValue ? fecha.Value.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) : string.Empty;

    private static string Campo(string? valor)
        => $"\"{(valor ?? string.Empty).Replace("\"", "\"\"")}\"";

    private static string? Limpiar(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static TicketDto ADto(Ticket t) => new()
    {
        Id = t.Id,
        Titulo = t.Titulo,
        Descripcion = t.Descripcion,
        Estado = (int)t.Estado,
        Prioridad = (int)t.Prioridad,
        FechaCreacion = t.FechaCreacion,
        FechaCierre = t.FechaCierre,
        SLAHoras = t.SLAHoras,
        Solicitante = t.Solicitante,
        TelegramChatId = t.TelegramChatId,
        CodigoNomenclatura = t.CodigoNomenclatura,
        Departamento = t.Departamento,
        Categoria = t.Categoria,
        EquipoAfectado = t.EquipoAfectado,
        SolucionAplicada = t.SolucionAplicada,
        SuministrosUtilizados = t.SuministrosUtilizados,
        FechaResolucion = t.FechaResolucion
    };

    public sealed class RecordatorioTicketDto
    {
        public int MinutosEspera { get; set; }
    }
}