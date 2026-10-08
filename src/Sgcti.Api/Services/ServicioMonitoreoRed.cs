using System.Diagnostics;
using System.Net;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Microsoft.EntityFrameworkCore;
using Sgcti.Core.Entities;
using Sgcti.Infrastructure.Persistence;

namespace Sgcti.Api.Services;

public class ServicioMonitoreoRed : BackgroundService
{
    private const string OidSysDescr = "1.3.6.1.2.1.1.1.0";

    private const string OidRamaDescripcionConsumibles = "1.3.6.1.2.1.43.11.1.1.6.1";

    private const string OidBaseCapacidadMaximaToner = "1.3.6.1.2.1.43.11.1.1.8.1.";

    private const string OidBaseNivelActualToner = "1.3.6.1.2.1.43.11.1.1.9.1.";

    private const string ComunidadSnmpPorDefecto = Impresora.ComunidadSnmpPorDefecto;

    private const int PuertoSnmp = 161;

    private const int LongitudMaximaModelo = 100;

    private static readonly string[] PalabrasClaveTonerNegro = ["Black", "Negro"];

    private static readonly string[] PalabrasClaveConsumible = ["Toner", "Tóner", "Cartridge", "Cartucho"];

    private static readonly string[] PalabrasClaveColor =
    [
        "Cyan",
        "Magenta",
        "Amarill",
        "Yellow",
        "Rojo",
        "Red",
        "Azul",
        "Blue",
        "Blanco",
        "White",
        "Verde",
        "Green",
        "Violeta",
        "Purple",
        "Rosa",
        "Pink",
        "Gris",
        "Gray",
        "Grey",
        "Naranja",
        "Orange",
        "Marrón",
        "Marron"
    ];

    private static readonly TimeSpan IntervaloDepuracion = TimeSpan.FromSeconds(15);

    private static readonly TimeSpan TimeoutSnmp = TimeSpan.FromSeconds(3);

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
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<SgctiDbContext>();

                    await MonitorearImpresorasAsync(dbContext, stoppingToken);
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
                    _logger.LogError(ex, "Falló la iteración del monitoreo de impresoras");
                }
            }

            try
            {
                await Task.Delay(IntervaloDepuracion, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task MonitorearImpresorasAsync(SgctiDbContext dbContext, CancellationToken stoppingToken)
    {
        var impresoras = await dbContext.Impresoras
            .Where(i => !string.IsNullOrWhiteSpace(i.Ip) && i.Ip != "0.0.0.0")
            .ToListAsync(stoppingToken);

        foreach (var impresora in impresoras)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            await ActualizarEstadoAsync(impresora, stoppingToken);
        }

        await dbContext.SaveChangesAsync(stoppingToken);
    }

    private async Task ActualizarEstadoAsync(Impresora impresora, CancellationToken cancellationToken)
    {
        try
        {
            impresora.NivelTonnerNegro = 0;
            impresora.EstadoPing = EstadoPing.Desconectado;
            impresora.LatenciaMs = 0;

            if (string.IsNullOrWhiteSpace(impresora.ComunidadSnmp))
            {
                impresora.ComunidadSnmp = ComunidadSnmpPorDefecto;
            }

            var cronometro = Stopwatch.StartNew();

            var modelo = await ObtenerModeloSnmpAsync(
                impresora.Ip,
                impresora.ComunidadSnmp,
                cancellationToken);

            cronometro.Stop();

            impresora.LatenciaMs = (int)Math.Min(cronometro.ElapsedMilliseconds, int.MaxValue);

            if (!string.IsNullOrWhiteSpace(modelo))
            {
                impresora.Modelo = modelo.Length > LongitudMaximaModelo
                    ? modelo[..LongitudMaximaModelo]
                    : modelo;

                impresora.EstadoPing = EstadoPing.EnLinea;
            }

            var indiceTonerNegro = await ObtenerIndiceTonerNegroAsync(
                impresora.Ip,
                impresora.ComunidadSnmp,
                cancellationToken);

            if (indiceTonerNegro is null)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning(
                        "No se encontró ningún cartucho negro en la rama de consumibles de {DireccionIp}",
                        impresora.Ip);
                }

                return;
            }

            var nivelTonner = await ObtenerNivelTonnerSnmpAsync(
                impresora.Ip,
                impresora.ComunidadSnmp,
                indiceTonerNegro.Value,
                cancellationToken);

            if (nivelTonner is not null)
            {
                impresora.NivelTonnerNegro = nivelTonner.Value;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            impresora.EstadoPing = EstadoPing.Desconectado;
            impresora.LatenciaMs = 0;
            impresora.NivelTonnerNegro = 0;

            if (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "No se pudo consultar la impresora {DireccionIp} por SNMP", impresora.Ip);
            }
        }
    }

    private async Task<int?> ObtenerNivelTonnerSnmpAsync(
        string direccionIp,
        string? comunidad,
        int indiceToner,
        CancellationToken cancellationToken)
    {
        var nivelActual = await ConsultarOidAsync(
            direccionIp,
            comunidad,
            $"{OidBaseNivelActualToner}{indiceToner}",
            cancellationToken);

        var nivelMaximo = await ConsultarOidAsync(
            direccionIp,
            comunidad,
            $"{OidBaseCapacidadMaximaToner}{indiceToner}",
            cancellationToken);

        if (nivelActual is null || nivelMaximo is null || nivelMaximo.Value <= 0)
        {
            return null;
        }

        var porcentaje = nivelActual.Value * 100m / nivelMaximo.Value;

        return (int)Math.Clamp(Math.Round(porcentaje), 0, 100);
    }

    private async Task<string?> ObtenerModeloSnmpAsync(
        string direccionIp,
        string? comunidad,
        CancellationToken cancellationToken)
    {
        var valor = await ConsultarOidTextoAsync(direccionIp, comunidad, OidSysDescr, cancellationToken);

        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }

    private async Task<int?> ObtenerIndiceTonerNegroAsync(
        string direccionIp,
        string? comunidad,
        CancellationToken cancellationToken)
    {
        using var limiteTiempo = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        limiteTiempo.CancelAfter(TimeoutSnmp);

        var consumibles = new List<Variable>();

        try
        {
            await Messenger.WalkAsync(
                VersionCode.V2,
                new IPEndPoint(IPAddress.Parse(direccionIp), PuertoSnmp),
                new OctetString(string.IsNullOrWhiteSpace(comunidad) ? ComunidadSnmpPorDefecto : comunidad),
                new ObjectIdentifier(OidRamaDescripcionConsumibles),
                consumibles,
                WalkMode.WithinSubtree,
                limiteTiempo.Token);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "No se pudo consultar la rama de consumibles de {DireccionIp} por SNMP", direccionIp);

            return null;
        }

        var candidatos = new List<(int Indice, string Descripcion)>();

        foreach (var consumible in consumibles)
        {
            var descripcion = consumible.Data?.ToString();

            if (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(
                    "[DEPURACION] Consumible {Oid} -> descripcion: '{Descripcion}'",
                    consumible.Id.ToString(),
                    descripcion);
            }

            if (string.IsNullOrWhiteSpace(descripcion))
            {
                continue;
            }

            if (int.TryParse(ObtenerUltimoSegmentoOid(consumible.Id.ToString()), out var indice))
            {
                candidatos.Add((indice, descripcion));
            }
        }

        var indiceNegroExplicito = candidatos
            .Where(candidato => ContieneAlguna(candidato.Descripcion, PalabrasClaveTonerNegro))
            .Select(candidato => candidato.Indice)
            .FirstOrDefault();

        if (indiceNegroExplicito > 0)
        {
            return indiceNegroExplicito;
        }

        var indiceConsumibleSinColor = candidatos
            .Where(candidato => ContieneAlguna(candidato.Descripcion, PalabrasClaveConsumible))
            .Where(candidato => !ContieneAlguna(candidato.Descripcion, PalabrasClaveColor))
            .Select(candidato => candidato.Indice)
            .FirstOrDefault();

        if (indiceConsumibleSinColor > 0)
        {
            return indiceConsumibleSinColor;
        }

        return null;
    }

    private static bool ContieneAlguna(string texto, IEnumerable<string> palabrasClave) =>
        palabrasClave.Any(palabra => texto.Contains(palabra, StringComparison.OrdinalIgnoreCase));

    private static string ObtenerUltimoSegmentoOid(string oid)
    {
        var posicionSeparador = oid.LastIndexOf('.');

        return posicionSeparador >= 0 && posicionSeparador < oid.Length - 1
            ? oid[(posicionSeparador + 1)..]
            : string.Empty;
    }

    private async Task<int?> ConsultarOidAsync(
        string direccionIp,
        string? comunidad,
        string oid,
        CancellationToken cancellationToken)
    {
        var valor = await ConsultarOidTextoAsync(direccionIp, comunidad, oid, cancellationToken);

        return int.TryParse(valor, out var resultado) ? resultado : null;
    }

    private async Task<string?> ConsultarOidTextoAsync(
        string direccionIp,
        string? comunidad,
        string oid,
        CancellationToken cancellationToken)
    {
        using var limiteTiempo = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        limiteTiempo.CancelAfter(TimeoutSnmp);

        try
        {
            var variables = new List<Variable> { new Variable(new ObjectIdentifier(oid)) };

            var respuesta = await Messenger.GetAsync(
                VersionCode.V2,
                new IPEndPoint(IPAddress.Parse(direccionIp), PuertoSnmp),
                new OctetString(string.IsNullOrWhiteSpace(comunidad) ? ComunidadSnmpPorDefecto : comunidad),
                variables,
                limiteTiempo.Token);

            return respuesta.FirstOrDefault()?.Data?.ToString();
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "No se pudo consultar la impresora {DireccionIp} por SNMP (OID {Oid})", direccionIp, oid);

            return null;
        }
    }
}