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
    private const string OidContadorTotalPaginas = "1.3.6.1.2.1.43.10.2.1.4.1.1";
    private const string OidRamaDescripcionConsumibles = "1.3.6.1.2.1.43.11.1.1.6.1";
    private const string OidBaseCapacidadMaximaToner = "1.3.6.1.2.1.43.11.1.1.8.1.";
    private const string OidBaseNivelActualToner = "1.3.6.1.2.1.43.11.1.1.9.1.";
    private const string ComunidadSnmpPorDefecto = Impresora.ComunidadSnmpPorDefecto;

    private const int PuertoSnmpEstandar = 161;
    private const int PuertoBasePuenteDocker = 16100;
    private const int RendimientoCartuchoPaginas = 3500;
    private const int UmbralMantenimientoPaginas = 15000;

    private static readonly Dictionary<string, string> DuplicadosUnificar = new(StringComparer.OrdinalIgnoreCase)
    {
        ["IR 1643iF Facturacion Total"] = "Facturacion IR1643 Total",
        ["IR 1643iF Facturacion Stargas"] = "Facturacion IR1643 Stargas",
        ["RICOH Cendi"] = "RICOH MP3350 Cendi"
    };

    // Mapa exacto 1 a 1 de las 13 impresoras del CENDI con emulador_snmp_cendi.py
    private static readonly Dictionary<string, (int AgenteId, string Ip)> MapaAgentesCendi = new(StringComparer.OrdinalIgnoreCase)
    {
        ["MF264 Stargas"]              = (1,  "127.0.0.11"),
        ["MF455 Overland"]             = (2,  "127.0.0.12"),
        ["MF455 Traco"]                = (3,  "127.0.0.13"),
        ["MF264 Rrhh"]                 = (4,  "127.0.0.14"),
        ["RICOH MP3350 Cendi"]         = (5,  "127.0.0.15"),
        ["Facturacion IR1643 Total"]   = (6,  "127.0.0.16"),
        ["Facturacion IR1643 Stargas"] = (7,  "127.0.0.17"),
        ["MF264 PCP"]                  = (8,  "127.0.0.18"),
        ["MF264 Overland"]             = (9,  "127.0.0.19"),
        ["MF455 Principal CENDI"]      = (10, "127.0.0.20"),
        ["MF264 Comercial OV-JD"]      = (11, "127.0.0.21"),
        ["IR1643 RRHH"]                = (12, "127.0.0.22"),
        ["MF264 Comercial"]            = (13, "127.0.0.23"),
    };

    private static readonly string[] PalabrasClaveTonerNegro = ["Black", "Negro"];
    private static readonly string[] PalabrasClaveConsumible = ["Toner", "Tóner", "Cartridge", "Cartucho", "Ink", "Tinta"];
    private static readonly string[] PalabrasClaveColor =
    [
        "Cyan", "Magenta", "Amarill", "Yellow", "Rojo", "Red", "Azul", "Blue",
        "Blanco", "White", "Verde", "Green", "Violeta", "Purple", "Rosa", "Pink"
    ];

    private static readonly TimeSpan IntervaloDepuracion = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan TimeoutSnmpLocal = TimeSpan.FromMilliseconds(350);
    private static readonly TimeSpan TimeoutSnmpRedReal = TimeSpan.FromMilliseconds(900);

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
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<SgctiDbContext>();

                await MonitorearYCalcularPrediccionesAsync(dbContext, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                if (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Falló la iteración del monitoreo SNMP de impresoras.");
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

    public static async Task MonitorearYCalcularPrediccionesAsync(
        SgctiDbContext dbContext,
        CancellationToken stoppingToken = default)
    {
        await UnificarImpresorasDuplicadasYDepartamentosAsync(dbContext, stoppingToken);

        var impresoras = await dbContext.Impresoras
            .OrderBy(i => i.Id)
            .ToListAsync(stoppingToken);

        if (impresoras.Count == 0)
        {
            return;
        }

        var estadisticasConsumo = await dbContext.RegistrosConsumoDiario
            .AsNoTracking()
            .GroupBy(r => r.ImpresoraId)
            .Select(g => new
            {
                ImpresoraId = g.Key,
                UltimoContador = g.Max(r => r.Contador),
                TotalConsumo = g.Sum(r => r.Consumo),
                PromedioDiario = g.Where(r => r.Consumo > 0 && r.Consumo < 2500).Average(r => (double?)r.Consumo) ?? 140.0
            })
            .ToListAsync(stoppingToken);

        var mapaConsumo = estadisticasConsumo.ToDictionary(x => x.ImpresoraId);

        var ultimosMantenimientos = await dbContext.MantenimientosImpresora
            .AsNoTracking()
            .GroupBy(m => m.ImpresoraId)
            .Select(g => new
            {
                ImpresoraId = g.Key,
                UltimaFecha = g.Max(m => m.Fecha)
            })
            .ToListAsync(stoppingToken);

        var mapaMantenimientos = ultimosMantenimientos.ToDictionary(x => x.ImpresoraId, x => x.UltimaFecha);

        // Asignar IP de simulador solo a las 13 impresoras del CENDI; respetar cualquier IP física real (ej. 192.168.100.57)
        var asignaciones = new List<(Impresora Impresora, int NumeroAgente)>();

        foreach (var imp in impresoras)
        {
            if (MapaAgentesCendi.TryGetValue(imp.Modelo.Trim(), out var config))
            {
                imp.Ip = config.Ip;
                asignaciones.Add((imp, config.AgenteId));
            }
            else
            {
                // Es una impresora agregada manualmente (ej. 192.168.100.57): NumeroAgente = 0 para NO usar el simulador
                asignaciones.Add((imp, 0));
            }

            if (string.IsNullOrWhiteSpace(imp.ComunidadSnmp))
            {
                imp.ComunidadSnmp = ComunidadSnmpPorDefecto;
            }
        }

        // Consultar todas las impresoras en PARALELO
        var tareasSnmp = asignaciones
            .Select(a => ConsultarAgenteSnmpRealAsync(a.Impresora.Ip, a.Impresora.ComunidadSnmp, a.NumeroAgente, stoppingToken))
            .ToArray();

        var resultadosSnmp = await Task.WhenAll(tareasSnmp);

        for (int i = 0; i < asignaciones.Count; i++)
        {
            var impresora = asignaciones[i].Impresora;
            var numeroAgente = asignaciones[i].NumeroAgente;
            var resultadoSnmp = resultadosSnmp[i];

            mapaConsumo.TryGetValue(impresora.Id, out var stats);
            var promedioDiario = Math.Round(Math.Clamp(stats?.PromedioDiario ?? 140.0, 75.0, 420.0), 1);
            impresora.PromedioPaginasDiario = promedioDiario;

            var contadorBaseHistorico = Math.Max(impresora.ContadorTotalPaginas, stats?.UltimoContador ?? 0);
            if (contadorBaseHistorico == 0 && stats is not null)
            {
                contadorBaseHistorico = stats.TotalConsumo;
            }

            // Calcular desgaste de mantenimiento según el último mantenimiento registrado
            if (mapaMantenimientos.TryGetValue(impresora.Id, out var fechaUltimoMant))
            {
                impresora.UltimoMantenimiento = fechaUltimoMant;
                var paginasPosteriores = await dbContext.RegistrosConsumoDiario
                    .AsNoTracking()
                    .Where(r => r.ImpresoraId == impresora.Id && r.Fecha > fechaUltimoMant)
                    .SumAsync(r => r.Consumo, stoppingToken);

                impresora.PaginasDesdeUltimoMantenimiento = paginasPosteriores;
            }
            else
            {
                impresora.PaginasDesdeUltimoMantenimiento = numeroAgente switch
                {
                    3 => 13950, // MF455 Traco -> Próximo a mantenimiento (~4 días)
                    7 => 13600, // Facturacion IR1643 Stargas -> Próximo a mantenimiento (~5 días)
                    _ => stats is not null ? Math.Min(11500, stats.TotalConsumo % UmbralMantenimientoPaginas) : 3500
                };
            }

            // Si la impresora no responde por SNMP (Desconectada), poner Tóner y Días de Predicción en 0
            if (!resultadoSnmp.EnLinea)
            {
                impresora.EstadoPing = EstadoPing.Desconectado;
                impresora.LatenciaMs = 0;
                impresora.NivelTonnerNegro = 0;
                impresora.DiasEstimadosAgotamientoTonner = 0;
                impresora.DiasEstimadosMantenimiento = 0;
                impresora.AlertaStockCritico = false;
                impresora.ContadorTotalPaginas = contadorBaseHistorico;
                continue;
            }

            // Si respondió por SNMP real:
            impresora.EstadoPing = EstadoPing.EnLinea;
            impresora.LatenciaMs = resultadoSnmp.LatenciaMs;
            impresora.NivelTonnerNegro = resultadoSnmp.NivelToner ?? 0;

            // NUNCA sobrescribir impresora.Modelo (para respetar el nombre personalizado del usuario).
            // Solo si Fabricante está vacío o genérico y SNMP devolvió un modelo útil, guardarlo en Fabricante.
            if (!string.IsNullOrWhiteSpace(resultadoSnmp.DescripcionSnmp) &&
                !resultadoSnmp.DescripcionSnmp.Contains("ETHERNET MULTI-ENVIRONMENT", StringComparison.OrdinalIgnoreCase) &&
                (string.IsNullOrWhiteSpace(impresora.Fabricante) ||
                 impresora.Fabricante is "Canon" or "HP" or "Ricoh" or "Epson"))
            {
                impresora.Fabricante = resultadoSnmp.DescripcionSnmp.Trim();
            }

            if (resultadoSnmp.ContadorPaginas.HasValue && resultadoSnmp.ContadorPaginas.Value > 0)
            {
                if (resultadoSnmp.ContadorPaginas.Value > contadorBaseHistorico && contadorBaseHistorico > 0)
                {
                    var delta = resultadoSnmp.ContadorPaginas.Value - contadorBaseHistorico;
                    impresora.PaginasDesdeUltimoMantenimiento += delta;
                }
                impresora.ContadorTotalPaginas = Math.Max(contadorBaseHistorico, resultadoSnmp.ContadorPaginas.Value);
            }
            else
            {
                impresora.ContadorTotalPaginas = contadorBaseHistorico;
            }

            // 1. Días estimados de agotamiento de tóner
            var paginasRestantesCartucho = (impresora.NivelTonnerNegro / 100.0) * RendimientoCartuchoPaginas;
            var diasToner = (int)Math.Max(1, Math.Round(paginasRestantesCartucho / promedioDiario));
            impresora.DiasEstimadosAgotamientoTonner = diasToner;
            impresora.FechaEstimadaAgotamientoTonner = DateTime.UtcNow.AddDays(diasToner);
            impresora.AlertaStockCritico = impresora.NivelTonnerNegro <= 15 || diasToner <= 7;

            // 2. Días estimados de próximo mantenimiento preventivo
            var paginasRestantesMant = Math.Max(0, UmbralMantenimientoPaginas - impresora.PaginasDesdeUltimoMantenimiento);
            var diasMant = paginasRestantesMant == 0
                ? 1
                : (int)Math.Max(1, Math.Round(paginasRestantesMant / promedioDiario));
            impresora.DiasEstimadosMantenimiento = diasMant;
            impresora.FechaEstimadaMantenimiento = DateTime.UtcNow.AddDays(diasMant);
        }

        await dbContext.SaveChangesAsync(stoppingToken);
    }

    private static async Task UnificarImpresorasDuplicadasYDepartamentosAsync(
        SgctiDbContext dbContext,
        CancellationToken ct)
    {
        var impresoras = await dbContext.Impresoras.OrderBy(i => i.Id).ToListAsync(ct);
        var huboCambios = false;

        foreach (var imp in impresoras.ToList())
        {
            var nombreLimpio = string.Join(' ', imp.Modelo.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries));
            var ipLimpia = imp.Ip?.Trim() ?? string.Empty;

            // Eliminar únicamente filas vacías o de pruebas viejas (Brother / 192.168.1.50), conservando 192.168.100.57
            if (string.IsNullOrWhiteSpace(nombreLimpio) ||
                ipLimpia == "192.168.1.50" ||
                nombreLimpio.Contains("Brother NC-8800w", StringComparison.OrdinalIgnoreCase))
            {
                var consumos = await dbContext.RegistrosConsumoDiario.Where(r => r.ImpresoraId == imp.Id).ToListAsync(ct);
                var mants = await dbContext.MantenimientosImpresora.Where(m => m.ImpresoraId == imp.Id).ToListAsync(ct);
                dbContext.RegistrosConsumoDiario.RemoveRange(consumos);
                dbContext.MantenimientosImpresora.RemoveRange(mants);
                dbContext.Impresoras.Remove(imp);
                impresoras.Remove(imp);
                huboCambios = true;
                continue;
            }

            if (DuplicadosUnificar.TryGetValue(nombreLimpio, out var nombreCanonico))
            {
                var principal = impresoras.FirstOrDefault(i =>
                    i.Id != imp.Id && string.Equals(i.Modelo.Trim(), nombreCanonico, StringComparison.OrdinalIgnoreCase));

                if (principal is not null)
                {
                    var registrosDuplicados = await dbContext.RegistrosConsumoDiario
                        .Where(r => r.ImpresoraId == imp.Id)
                        .ToListAsync(ct);

                    var fechasPrincipal = (await dbContext.RegistrosConsumoDiario
                        .Where(r => r.ImpresoraId == principal.Id)
                        .Select(r => r.Fecha.Date)
                        .ToListAsync(ct)).ToHashSet();

                    foreach (var reg in registrosDuplicados)
                    {
                        if (!fechasPrincipal.Contains(reg.Fecha.Date))
                        {
                            reg.ImpresoraId = principal.Id;
                        }
                        else
                        {
                            dbContext.RegistrosConsumoDiario.Remove(reg);
                        }
                    }

                    dbContext.Impresoras.Remove(imp);
                    impresoras.Remove(imp);
                    huboCambios = true;
                    continue;
                }

                imp.Modelo = nombreCanonico;
                huboCambios = true;
            }
            else if (imp.Modelo != nombreLimpio)
            {
                imp.Modelo = nombreLimpio;
                huboCambios = true;
            }

            // Solo auto-asignar departamento a las 13 impresoras del CENDI o si el departamento está vacío
            if (MapaAgentesCendi.ContainsKey(imp.Modelo) || string.IsNullOrWhiteSpace(imp.Departamento))
            {
                var deptoReal = InferirDepartamentoReal(imp.Modelo);
                if (imp.Departamento != deptoReal)
                {
                    imp.Departamento = deptoReal;
                    huboCambios = true;
                }
            }

            if (string.IsNullOrWhiteSpace(imp.Fabricante))
            {
                imp.Fabricante = InferirFabricanteReal(imp.Modelo);
                huboCambios = true;
            }
        }

        if (huboCambios)
        {
            await dbContext.SaveChangesAsync(ct);
        }
    }

    private static string InferirDepartamentoReal(string modelo)
    {
        var norm = modelo.ToLowerInvariant();
        if (norm.Contains("facturacion")) return "Facturación";
        if (norm.Contains("rrhh")) return "RRHH";
        if (norm.Contains("pcp")) return "PCP";
        if (norm.Contains("comercial")) return "Comercial";
        if (norm.Contains("traco")) return "Operaciones (Traco)";
        if (norm.Contains("overland")) return "Operaciones (Overland)";
        if (norm.Contains("principal") || norm.Contains("ricoh")) return "Administración CENDI";
        return "Stargas General";
    }

    private static string InferirFabricanteReal(string modelo)
    {
        var norm = modelo.ToLowerInvariant();
        if (norm.Contains("ricoh")) return "Ricoh";
        if (norm.Contains("hp")) return "HP";
        return "Canon";
    }

    private static async Task<(bool EnLinea, int LatenciaMs, int? NivelToner, int? ContadorPaginas, string? DescripcionSnmp)> ConsultarAgenteSnmpRealAsync(
        string ipTexto,
        string? comunidad,
        int numeroAgente,
        CancellationToken ct)
    {
        var endpointsCandidatos = new List<IPEndPoint>();
        var esSimuladorCendi = numeroAgente > 0 && ipTexto.StartsWith("127.0.0.", StringComparison.OrdinalIgnoreCase);
        var timeout = esSimuladorCendi ? TimeoutSnmpLocal : TimeoutSnmpRedReal;

        if (esSimuladorCendi)
        {
            try
            {
                var ipsHostDocker = await Dns.GetHostAddressesAsync("host.docker.internal", ct);
                var ipv4Host = ipsHostDocker.FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                if (ipv4Host is not null)
                {
                    endpointsCandidatos.Add(new IPEndPoint(ipv4Host, PuertoBasePuenteDocker + numeroAgente));
                }
            }
            catch
            {
                // Ejecución local fuera de Docker
            }

            if (IPAddress.TryParse(ipTexto.Trim(), out var ipLoopback))
            {
                endpointsCandidatos.Add(new IPEndPoint(ipLoopback, PuertoBasePuenteDocker + numeroAgente));
                endpointsCandidatos.Add(new IPEndPoint(ipLoopback, PuertoSnmpEstandar));
            }
        }
        else if (IPAddress.TryParse(ipTexto.Trim(), out var ipFisicaReal))
        {
            // Impresora física real en red (ej. 192.168.100.57): consultar exclusivamente su IP real en el puerto 161
            endpointsCandidatos.Add(new IPEndPoint(ipFisicaReal, PuertoSnmpEstandar));
        }

        foreach (var endpoint in endpointsCandidatos)
        {
            try
            {
                var cronometro = Stopwatch.StartNew();
                var descr = await ConsultarOidTextoAsync(endpoint, comunidad, OidSysDescr, timeout, ct);
                cronometro.Stop();

                if (string.IsNullOrWhiteSpace(descr))
                {
                    continue;
                }

                var latencia = Math.Max(1, (int)cronometro.ElapsedMilliseconds);
                var indiceToner = esSimuladorCendi ? 1 : (await ObtenerIndiceTonerNegroAsync(endpoint, comunidad, timeout, ct) ?? 1);
                var nivelToner = await ObtenerNivelTonnerSnmpAsync(endpoint, comunidad, indiceToner, timeout, ct);

                int? contadorPaginas = null;
                var contadorTxt = await ConsultarOidTextoAsync(endpoint, comunidad, OidContadorTotalPaginas, timeout, ct);
                if (int.TryParse(contadorTxt, out var contParsed) && contParsed > 0)
                {
                    contadorPaginas = contParsed;
                }

                return (true, latencia, nivelToner, contadorPaginas, descr);
            }
            catch
            {
                // Intentar siguiente endpoint candidato
            }
        }

        return (false, 0, null, null, null);
    }

    private static async Task<int?> ObtenerNivelTonnerSnmpAsync(
        IPEndPoint endpoint,
        string? comunidad,
        int indiceToner,
        TimeSpan timeout,
        CancellationToken ct)
    {
        var actualTexto = await ConsultarOidTextoAsync(endpoint, comunidad, $"{OidBaseNivelActualToner}{indiceToner}", timeout, ct);
        var maxTexto = await ConsultarOidTextoAsync(endpoint, comunidad, $"{OidBaseCapacidadMaximaToner}{indiceToner}", timeout, ct);

        if (!int.TryParse(actualTexto, out var actual) || !int.TryParse(maxTexto, out var maximo) || maximo <= 0)
        {
            return null;
        }

        var porcentaje = actual * 100m / maximo;
        return (int)Math.Clamp(Math.Round(porcentaje), 0, 100);
    }

    private static async Task<int?> ObtenerIndiceTonerNegroAsync(
        IPEndPoint endpoint,
        string? comunidad,
        TimeSpan timeout,
        CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);

        var consumibles = new List<Variable>();
        try
        {
            await Messenger.WalkAsync(
                VersionCode.V2,
                endpoint,
                new OctetString(string.IsNullOrWhiteSpace(comunidad) ? ComunidadSnmpPorDefecto : comunidad),
                new ObjectIdentifier(OidRamaDescripcionConsumibles),
                consumibles,
                WalkMode.WithinSubtree,
                cts.Token);
        }
        catch
        {
            return 1;
        }

        foreach (var item in consumibles)
        {
            var desc = item.Data?.ToString() ?? string.Empty;
            var oidStr = item.Id.ToString();
            var pos = oidStr.LastIndexOf('.');
            if (pos >= 0 && int.TryParse(oidStr[(pos + 1)..], out var idx))
            {
                if (PalabrasClaveTonerNegro.Any(p => desc.Contains(p, StringComparison.OrdinalIgnoreCase)) ||
                    (PalabrasClaveConsumible.Any(p => desc.Contains(p, StringComparison.OrdinalIgnoreCase)) &&
                     !PalabrasClaveColor.Any(c => desc.Contains(c, StringComparison.OrdinalIgnoreCase))))
                {
                    return idx;
                }
            }
        }

        return 1;
    }

    private static async Task<string?> ConsultarOidTextoAsync(
        IPEndPoint endpoint,
        string? comunidad,
        string oid,
        TimeSpan timeout,
        CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);

        var variables = new List<Variable> { new(new ObjectIdentifier(oid)) };
        var respuesta = await Messenger.GetAsync(
            VersionCode.V2,
            endpoint,
            new OctetString(string.IsNullOrWhiteSpace(comunidad) ? ComunidadSnmpPorDefecto : comunidad),
            variables,
            cts.Token);

        return respuesta.FirstOrDefault()?.Data?.ToString();
    }
}