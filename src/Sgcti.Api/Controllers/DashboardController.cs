using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sgcti.Core.Entities;
using Sgcti.Infrastructure.Persistence;

namespace Sgcti.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private const int UmbralDiasAlerta = 14;

    private readonly SgctiDbContext _db;

    public DashboardController(SgctiDbContext db)
    {
        _db = db;
    }

    [HttpGet("resumen")]
    public async Task<IActionResult> Resumen(CancellationToken ct)
    {
        var hoyUtc = DateTime.UtcNow.Date;
        var mananaUtc = hoyUtc.AddDays(1);

        var totalTicketsAbiertos = await _db.Tickets
            .CountAsync(t => t.Estado != EstadoTicket.Resuelto, ct);

        var suministrosCriticos = await _db.Suministros
            .CountAsync(s => s.CantidadActual <= s.StockMinimo || s.CantidadActual < 3, ct);

        var impresorasDesconectadas = await _db.Impresoras
            .CountAsync(i => i.EstadoPing == EstadoPing.Desconectado, ct);

        var actividadesHoy = await _db.BitacoraActividades
            .CountAsync(b => b.HoraInicio >= hoyUtc && b.HoraInicio < mananaUtc, ct);

        var alertasSuministros = await _db.Suministros
            .AsNoTracking()
            .Where(s => s.CantidadActual <= s.StockMinimo || s.CantidadActual < 3)
            .OrderBy(s => s.Nombre)
            .Select(s => new
            {
                nombre = s.Nombre,
                tipo = s.Tipo,
                cantidadActual = s.CantidadActual,
                stockMinimo = s.StockMinimo
            })
            .ToListAsync(ct);

        var actividadReciente = await _db.BitacoraActividades
            .AsNoTracking()
            .OrderByDescending(b => b.HoraInicio)
            .Take(5)
            .Select(b => new { analista = b.AnalistaId, actividad = b.DescripcionActividad, hora = b.HoraInicio })
            .ToListAsync(ct);

        var impresorasEnLinea = await _db.Impresoras
            .AsNoTracking()
            .Where(i => i.EstadoPing == EstadoPing.EnLinea)
            .OrderBy(i => i.Id)
            .ToListAsync(ct);

        var alertasPredictivas = new List<string>();

        foreach (var imp in impresorasEnLinea)
        {
            if (imp.DiasEstimadosAgotamientoTonner > 0
                && imp.DiasEstimadosAgotamientoTonner <= UmbralDiasAlerta)
            {
                alertasPredictivas.Add(
                    $"⚠️ Tóner crítico ({imp.NivelTonnerNegro}%) en {imp.Modelo}: se agotará en aprox. {imp.DiasEstimadosAgotamientoTonner} días.");
            }

            if (imp.PaginasDesdeUltimoMantenimiento > 0
                && imp.DiasEstimadosMantenimiento > 0
                && imp.DiasEstimadosMantenimiento <= UmbralDiasAlerta)
            {
                alertasPredictivas.Add(
                    $"⚠️ Mantenimiento próximo en {imp.Modelo}: toca en aprox. {imp.DiasEstimadosMantenimiento} días.");
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