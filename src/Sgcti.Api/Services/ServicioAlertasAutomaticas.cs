using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Sgcti.Api.Hubs;
using Sgcti.Core.Entities;
using Sgcti.Infrastructure.Persistence;

namespace Sgcti.Api.Services;

public class ServicioAlertasAutomaticas : BackgroundService
{
    private const int UmbralDiasAlerta = 14;

    private static readonly TimeSpan IntervaloRevision = TimeSpan.FromMinutes(5);

    private readonly IServiceScopeFactory _scopeFactory;

    private readonly IHubContext<NotificacionesHub> _hubContext;

    private readonly ILogger<ServicioAlertasAutomaticas> _logger;

    public ServicioAlertasAutomaticas(
        IServiceScopeFactory scopeFactory,
        IHubContext<NotificacionesHub> hubContext,
        ILogger<ServicioAlertasAutomaticas> logger)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<SgctiDbContext>();

                await ServicioMonitoreoRed.MonitorearYCalcularPrediccionesAsync(dbContext, stoppingToken);
                await RevisarPrediccionesAsync(dbContext, stoppingToken);
                await RevisarStockBajoAsync(dbContext, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                if (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Falló la iteración de alertas predictivas automáticas.");
                }
            }

            try
            {
                await Task.Delay(IntervaloRevision, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task RevisarStockBajoAsync(SgctiDbContext dbContext, CancellationToken stoppingToken)
    {
        var suministrosCriticos = await dbContext.Suministros
            .AsNoTracking()
            .Where(s => s.CantidadActual <= s.StockMinimo || s.CantidadActual < 3)
            .OrderBy(s => s.Nombre)
            .ToListAsync(stoppingToken);

        foreach (var suministro in suministrosCriticos)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            await _hubContext.Clients.All.SendAsync(
                "RecibirNotificacion",
                "⚠️ Stock Bajo",
                $"{suministro.Nombre} - {suministro.Tipo} · Quedan {suministro.CantidadActual} unidades (mínimo {suministro.StockMinimo}).",
                stoppingToken);
        }
    }

    private async Task RevisarPrediccionesAsync(
        SgctiDbContext dbContext,
        CancellationToken stoppingToken)
    {
        var impresoras = await dbContext.Impresoras
            .AsNoTracking()
            .ToListAsync(stoppingToken);

        foreach (var actualizada in impresoras)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            if (actualizada.EstadoPing == EstadoPing.EnLinea
                && actualizada.DiasEstimadosAgotamientoTonner > 0
                && actualizada.DiasEstimadosAgotamientoTonner <= UmbralDiasAlerta)
            {
                await _hubContext.Clients.All.SendAsync(
                    "RecibirNotificacion",
                    "⚠️ Alerta Predictiva (Tóner)",
                    $"Tóner crítico ({actualizada.NivelTonnerNegro}%) en {actualizada.Modelo}: se agotará en aprox. {actualizada.DiasEstimadosAgotamientoTonner} días.",
                    stoppingToken);
            }

            if (actualizada.DiasEstimadosMantenimiento > 0
                && actualizada.DiasEstimadosMantenimiento <= UmbralDiasAlerta)
            {
                await _hubContext.Clients.All.SendAsync(
                    "RecibirNotificacion",
                    "⚠️ Alerta Predictiva (Mantenimiento)",
                    $"Mantenimiento preventivo próximo en {actualizada.Modelo}: toca en aprox. {actualizada.DiasEstimadosMantenimiento} días.",
                    stoppingToken);
            }
        }
    }
}