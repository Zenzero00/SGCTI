using System.Net.NetworkInformation;
using Microsoft.EntityFrameworkCore;
using Sgcti.Core.Entities;
using Sgcti.Infrastructure.Persistence;

namespace Sgcti.Api.Services;

public class ServicioMonitoreoRed : BackgroundService
{
    private static readonly TimeSpan IntervaloEjecucion = TimeSpan.FromMinutes(5);

    private static readonly TimeSpan TimeoutPing = TimeSpan.FromSeconds(3);

    private readonly IServiceScopeFactory _scopeFactory;

    private readonly ILogger<ServicioMonitoreoRed> _logger;

    public ServicioMonitoreoRed(IServiceScopeFactory scopeFactory, ILogger<ServicioMonitoreoRed> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await MonitorearImpresorasAsync(stoppingToken);

            try
            {
                await Task.Delay(IntervaloEjecucion, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task MonitorearImpresorasAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SgctiDbContext>();

        var impresoras = await dbContext.Impresoras
            .Where(i => !string.IsNullOrWhiteSpace(i.Ip) && i.Ip != "0.0.0.0")
            .ToListAsync(stoppingToken);

        foreach (var impresora in impresoras)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            impresora.EstadoPing = await ObtenerEstadoPingAsync(impresora.Ip);
        }

        await dbContext.SaveChangesAsync(stoppingToken);
    }

    private async Task<EstadoPing> ObtenerEstadoPingAsync(string direccionIp)
    {
        using var ping = new Ping();

        try
        {
            var respuesta = await ping.SendPingAsync(direccionIp, (int)TimeoutPing.TotalMilliseconds);
            return respuesta.Status == IPStatus.Success
                ? EstadoPing.EnLinea
                : EstadoPing.Desconectado;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo realizar el ping a la IP {DireccionIp}", direccionIp);
            return EstadoPing.Desconectado;
        }
    }
}