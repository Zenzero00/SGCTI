using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sgcti.Core.Entities;
using Sgcti.Core.Services;
using Sgcti.Infrastructure.Persistence;

namespace Sgcti.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private const int UmbralDiasAlerta = 14;

    private readonly SgctiDbContext _db;

    private readonly ServicioAnalisisPredictivo _servicioPredictivo;

    public DashboardController(SgctiDbContext db, ServicioAnalisisPredictivo servicioPredictivo)
    {
        _db = db;
        _servicioPredictivo = servicioPredictivo;
    }

    [HttpGet("resumen")]
    public async Task<IActionResult> Resumen(CancellationToken ct)
    {
        var hoyUtc = DateTime.UtcNow.Date;
        var mananaUtc = hoyUtc.AddDays(1);

        var totalTicketsAbiertos = await _db.Tickets
            .CountAsync(t => t.Estado != EstadoTicket.Resuelto, ct);

        var suministrosCriticos = await _db.Suministros
            .CountAsync(s => s.CantidadActual <= s.StockMinimo, ct);

        var impresorasDesconectadas = await _db.Impresoras
            .CountAsync(i => i.EstadoPing == EstadoPing.Desconectado, ct);

        var actividadesHoy = await _db.BitacoraActividades
            .CountAsync(b => b.HoraInicio >= hoyUtc && b.HoraInicio < mananaUtc, ct);

        var alertasSuministros = await _db.Suministros
            .AsNoTracking()
            .Where(s => s.CantidadActual <= s.StockMinimo)
            .OrderBy(s => s.Nombre)
            .Select(s => new { nombre = s.Nombre, cantidadActual = s.CantidadActual })
            .ToListAsync(ct);

        var actividadReciente = await _db.BitacoraActividades
            .AsNoTracking()
            .OrderByDescending(b => b.HoraInicio)
            .Take(5)
            .Select(b => new { analista = b.AnalistaId, actividad = b.DescripcionActividad, hora = b.HoraInicio })
            .ToListAsync(ct);

        var impresoras = await _db.Impresoras
            .AsNoTracking()
            .ToListAsync(ct);

        var alertasPredictivas = new List<string>();

        foreach (var impresora in impresoras)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await _servicioPredictivo.CalcularPredicciones(impresora.Id);
            }
            catch
            {
                continue;
            }

            var actualizada = await _db.Impresoras
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == impresora.Id, ct);

            if (actualizada is null)
            {
                continue;
            }

            if (actualizada.DiasEstimadosAgotamientoTonner >= 0
                && actualizada.DiasEstimadosAgotamientoTonner <= UmbralDiasAlerta)
            {
                alertasPredictivas.Add(
                    $"⚠️ Tóner crítico en {actualizada.Modelo}: se agotará en aprox. {actualizada.DiasEstimadosAgotamientoTonner} días.");
            }

            if (actualizada.DiasEstimadosMantenimiento >= 0
                && actualizada.DiasEstimadosMantenimiento <= UmbralDiasAlerta)
            {
                alertasPredictivas.Add(
                    $"⚠️ Mantenimiento próximo en {actualizada.Modelo}: toca en aprox. {actualizada.DiasEstimadosMantenimiento} días.");
            }
        }

        return Ok(new
        {
            totalTicketsAbiertos,
            suministrosCriticos,
            impresorasDesconectadas,
            actividadesHoy,
            alertasSuministros,
            alertasPredictivas,
            actividadReciente
        });
    }
}