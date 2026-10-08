using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Sgcti.Api.Hubs;
using Sgcti.Core.Services;
using Sgcti.Infrastructure.Persistence;

namespace Sgcti.Api.Services;

public class ServicioAlertasAutomaticas : BackgroundService
{
    private const int UmbralDiasAlerta = 14;

    private static readonly TimeSpan IntervaloRevision = TimeSpan.FromHours(24);

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
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<SgctiDbContext>();
                    var servicioPredictivo = scope.ServiceProvider.GetRequiredService<ServicioAnalisisPredictivo>();

                    await RevisarPrediccionesAsync(dbContext, servicioPredictivo, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                if (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Falló la iteración de alertas predictivas automáticas");
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

    private async Task RevisarPrediccionesAsync(
        SgctiDbContext dbContext,
        ServicioAnalisisPredictivo servicioPredictivo,
        CancellationToken stoppingToken)
    {
        var impresoras = await dbContext.Impresoras
            .AsNoTracking()
            .ToListAsync(stoppingToken);

        foreach (var impresora in impresoras)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await servicioPredictivo.CalcularPredicciones(impresora.Id);

                var actualizada = await dbContext.Impresoras
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.Id == impresora.Id, stoppingToken);

                if (actualizada is null)
                {
                    continue;
                }

                if (actualizada.DiasEstimadosAgotamientoTonner >= 0
                    && actualizada.DiasEstimadosAgotamientoTonner <= UmbralDiasAlerta)
                {
                    await _hubContext.Clients.All.SendAsync(
                        "RecibirNotificacion",
                        "⚠️ Alerta Predictiva",
                        $"Tóner crítico en {actualizada.Modelo}: se agotará en aprox. {actualizada.DiasEstimadosAgotamientoTonner} días.",
                        stoppingToken);
                }

                if (actualizada.DiasEstimadosMantenimiento >= 0
                    && actualizada.DiasEstimadosMantenimiento <= UmbralDiasAlerta)
                {
                    await _hubContext.Clients.All.SendAsync(
                        "RecibirNotificacion",
                        "⚠️ Alerta Predictiva",
                        $"Mantenimiento próximo en {actualizada.Modelo}: toca en aprox. {actualizada.DiasEstimadosMantenimiento} días.",
                        stoppingToken);
                }
            }
            catch (Exception ex)
            {
                if (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning(
                        ex,
                        "No se pudieron evaluar las predicciones de la impresora {Modelo} (Id {Id})",
                        impresora.Modelo,
                        impresora.Id);
                }
            }
        }
    }
}