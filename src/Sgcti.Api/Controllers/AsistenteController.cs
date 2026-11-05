using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sgcti.Api.Services;
using Sgcti.Core.Dtos;
using Sgcti.Core.Entities;
using Sgcti.Infrastructure.Persistence;

namespace Sgcti.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/Asistente")]
public class AsistenteController : ControllerBase
{
    private readonly IServicioAsistenteIA _servicioAsistenteIA;
    private readonly IServicioBuscadorManuales _servicioBuscadorManuales;
    private readonly SgctiDbContext _db;

    public AsistenteController(
        IServicioAsistenteIA servicioAsistenteIA,
        IServicioBuscadorManuales servicioBuscadorManuales,
        SgctiDbContext db)
    {
        _servicioAsistenteIA = servicioAsistenteIA;
        _servicioBuscadorManuales = servicioBuscadorManuales;
        _db = db;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> ProcesarChat([FromBody] ConsultaIA_Dto consulta, CancellationToken ct)
    {
        if (consulta is null || string.IsNullOrWhiteSpace(consulta.Pregunta))
        {
            return BadRequest(new { mensaje = "La consulta no puede estar vacía." });
        }

        try
        {
            var contextoSistema = await ConstruirContextoCompletoSistemaAsync(ct);

            var resultado = await _servicioAsistenteIA.ConsultarChatbotConContextoAsync(
                consulta.Pregunta,
                contextoSistema,
                consulta.Historial);

            return Ok(new { respuesta = resultado });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("manuales")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SubirManual(
        [FromForm] IFormFile? archivo,
        [FromForm] int? impresoraId,
        CancellationToken ct)
    {
        if (archivo is null || archivo.Length <= 0)
        {
            return BadRequest(new { mensaje = "Debes seleccionar un archivo PDF válido." });
        }

        var esPdf = archivo.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)
            || Path.GetExtension(archivo.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase);

        if (!esPdf)
        {
            return BadRequest(new { mensaje = "El manual técnico debe estar en formato PDF." });
        }

        try
        {
            var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Manuales");
            Directory.CreateDirectory(carpeta);

            string nombreArchivo;
            Impresora? impresoraVinculada = null;

            if (impresoraId.HasValue && impresoraId.Value > 0)
            {
                impresoraVinculada = await _db.Impresoras.FindAsync(new object[] { impresoraId.Value }, ct);
                nombreArchivo = $"manual_impresora_{impresoraId.Value}.pdf";
            }
            else
            {
                var nombreLimpio = Path.GetFileNameWithoutExtension(archivo.FileName);
                nombreLimpio = new string(nombreLimpio.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-').ToArray());
                if (string.IsNullOrWhiteSpace(nombreLimpio))
                {
                    nombreLimpio = "manual_general";
                }
                nombreArchivo = $"manual_{nombreLimpio}.pdf";
            }

            var rutaFisica = Path.Combine(carpeta, nombreArchivo);

            await using (var stream = new FileStream(rutaFisica, FileMode.Create))
            {
                await archivo.CopyToAsync(stream, ct);
            }

            var rutaRelativa = $"Uploads/Manuales/{nombreArchivo}";

            if (impresoraVinculada is not null)
            {
                impresoraVinculada.RutaManual = rutaRelativa;
                await _db.SaveChangesAsync(ct);
            }

            var textoExtraido = await _servicioAsistenteIA.ExtraerTextoPdfAsync(rutaRelativa);
            var paginasAprox = Math.Max(1, textoExtraido.Length / 1800);

            var mensajeConfirmacion = impresoraVinculada is not null
                ? $"✅ Manual '{archivo.FileName}' cargado y vinculado a la impresora '{impresoraVinculada.Modelo}' (~{paginasAprox} págs. procesadas). Ya puedes hacerme cualquier consulta técnica o de códigos de error sobre este manual."
                : $"✅ Manual '{archivo.FileName}' cargado correctamente en el sistema (~{paginasAprox} págs. procesadas). Ya he leído su contenido y puedes preguntarme sobre sus procedimientos, repuestos o códigos de error.";

            return Ok(new { mensaje = mensajeConfirmacion, ruta = rutaRelativa });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = $"No se pudo procesar el manual PDF: {ex.Message}" });
        }
    }

    private async Task<string> ConstruirContextoCompletoSistemaAsync(CancellationToken ct)
    {
        var sb = new StringBuilder();

        // 1. Impresoras, estado SNMP y predicciones
        var impresoras = await _db.Impresoras
            .AsNoTracking()
            .OrderBy(i => i.Id)
            .ToListAsync(ct);

        var consumosAgrupados = await _db.RegistrosConsumoDiario
            .AsNoTracking()
            .GroupBy(r => r.ImpresoraId)
            .Select(g => new
            {
                ImpresoraId = g.Key,
                TotalPaginas = g.Sum(x => x.Consumo),
                PromedioDiario = g.Average(x => (double)x.Consumo),
                UltimoContador = g.Max(x => x.Contador),
                UltimaFecha = g.Max(x => x.Fecha),
                CantidadRegistros = g.Count()
            })
            .ToListAsync(ct);

        var mapaConsumo = consumosAgrupados.ToDictionary(x => x.ImpresoraId);

        sb.AppendLine("=== 1. IMPRESORAS REGISTRADAS, ESTADO DE RED Y PREDICCIONES ===");
        if (impresoras.Count == 0)
        {
            sb.AppendLine("No hay impresoras registradas.");
        }
        else
        {
            foreach (var imp in impresoras)
            {
                mapaConsumo.TryGetValue(imp.Id, out var hist);
                var contadorReal = Math.Max(imp.ContadorTotalPaginas, hist?.UltimoContador ?? 0);
                var promedioReal = imp.PromedioPaginasDiario > 0
                    ? imp.PromedioPaginasDiario
                    : Math.Round(hist?.PromedioDiario ?? 0, 1);
                var totalHistorico = hist?.TotalPaginas ?? 0;

                sb.AppendLine(
                    $"- ID:{imp.Id} | Modelo:{imp.Modelo} | Depto:{imp.Departamento} | IP:{imp.Ip} | " +
                    $"Estado:{imp.EstadoPing} ({imp.LatenciaMs} ms) | Tóner Negro:{imp.NivelTonnerNegro}% | " +
                    $"Contador Actual:{contadorReal:N0} págs | Total Impreso en Histórico:{totalHistorico:N0} págs | " +
                    $"Promedio Diario:{promedioReal:N1} págs/día | " +
                    $"Días Estimados Agotamiento Tóner:{imp.DiasEstimadosAgotamientoTonner} días | " +
                    $"Días Estimados Próximo Mantenimiento:{imp.DiasEstimadosMantenimiento} días | " +
                    $"Págs desde último mantenimiento:{imp.PaginasDesdeUltimoMantenimiento:N0} | " +
                    $"Último Mantenimiento:{(imp.UltimoMantenimiento.HasValue ? imp.UltimoMantenimiento.Value.ToString("dd/MM/yyyy") : "Sin registro")} | " +
                    $"Manual PDF:{(string.IsNullOrWhiteSpace(imp.RutaManual) ? "No" : imp.RutaManual)}");
            }
        }

        // 2. Inventario de Suministros y Consumibles
        sb.AppendLine();
        sb.AppendLine("=== 2. INVENTARIO DE SUMINISTROS Y CONSUMIBLES ===");
        var suministros = await _db.Suministros.AsNoTracking().OrderBy(s => s.Nombre).ToListAsync(ct);
        if (suministros.Count == 0)
        {
            sb.AppendLine("Sin suministros registrados.");
        }
        else
        {
            foreach (var s in suministros)
            {
                var estadoStock = s.CantidadActual <= s.StockMinimo ? " [ALERTA: STOCK CRÍTICO]" : " [OK]";
                sb.AppendLine($"- Suministro: {s.Nombre} | Tipo: {s.Tipo} | Cantidad Actual: {s.CantidadActual} | Stock Mínimo: {s.StockMinimo}{estadoStock} | Obs: {s.Observacion ?? "-"}");
            }
        }

        var consumibles = await _db.Consumibles.AsNoTracking().ToListAsync(ct);
        foreach (var c in consumibles)
        {
            var estadoStock = c.StockActual <= c.StockMinimo ? " [ALERTA: STOCK CRÍTICO]" : " [OK]";
            sb.AppendLine($"- Consumible: {c.Tipo} ({c.ModeloCompatible}) | Stock Actual: {c.StockActual} | Stock Mínimo: {c.StockMinimo}{estadoStock}");
        }

        // 3. Mesa de Ayuda (Tickets)
        sb.AppendLine();
        sb.AppendLine("=== 3. MESA DE AYUDA (TICKETS DE SOPORTE TI) ===");
        var tickets = await _db.Tickets
            .AsNoTracking()
            .OrderByDescending(t => t.FechaCreacion)
            .ToListAsync(ct);

        var abiertos = tickets.Count(t => t.Estado == EstadoTicket.Abierto);
        var enProgreso = tickets.Count(t => t.Estado == EstadoTicket.EnProgreso);
        var resueltos = tickets.Count(t => t.Estado == EstadoTicket.Resuelto);
        sb.AppendLine($"Resumen Tickets -> Total: {tickets.Count} | Abiertos: {abiertos} | En Progreso: {enProgreso} | Resueltos: {resueltos}");

        foreach (var t in tickets.Take(20))
        {
            sb.AppendLine(
                $"- Ticket {t.CodigoNomenclatura ?? "#" + t.Id} | Estado:{t.Estado} | Prioridad:{t.Prioridad} | " +
                $"Solicitante:{t.Solicitante ?? "-"} | Depto:{t.Departamento ?? "-"} | Categoría:{t.Categoria ?? "-"} | " +
                $"Título:{t.Titulo} | Problema:{t.Descripcion} | Equipo:{t.EquipoAfectado ?? "-"} | Solución:{t.SolucionAplicada ?? "-"}");
        }

        // 4. Mantenimientos de Impresoras y Bitácora
        sb.AppendLine();
        sb.AppendLine("=== 4. ÚLTIMOS MANTENIMIENTOS DE IMPRESORAS Y BITÁCORA ===");
        var mantenimientos = await _db.MantenimientosImpresora
            .AsNoTracking()
            .OrderByDescending(m => m.Fecha)
            .Take(10)
            .ToListAsync(ct);

        foreach (var m in mantenimientos)
        {
            var nombreImp = impresoras.FirstOrDefault(i => i.Id == m.ImpresoraId)?.Modelo ?? $"Impresora #{m.ImpresoraId}";
            sb.AppendLine($"- Mantenimiento {m.Tipo} en {nombreImp} ({m.Fecha:dd/MM/yyyy}) por {m.RealizadoPor}: {m.Descripcion}");
        }

        var bitacora = await _db.BitacoraActividades
            .AsNoTracking()
            .OrderByDescending(b => b.HoraInicio)
            .Take(10)
            .ToListAsync(ct);

        foreach (var b in bitacora)
        {
            sb.AppendLine($"- Bitácora [{b.Estado} - {b.Etiqueta}] Analista: {b.AnalistaId} | Actividad: {b.DescripcionActividad} | Equipo: {b.EquipoIntervenido ?? "-"}");
        }

        // 5. Manuales Técnicos PDF cargados en Uploads/Manuales
        var carpetaManuales = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Manuales");
        if (Directory.Exists(carpetaManuales))
        {
            var archivosPdf = Directory.GetFiles(carpetaManuales, "*.pdf")
                .OrderByDescending(System.IO.File.GetLastWriteTimeUtc)
                .Take(3)
                .ToList();

            if (archivosPdf.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("=== 5. CONTENIDO DE MANUALES TÉCNICOS PDF CARGADOS ===");
                foreach (var rutaPdf in archivosPdf)
                {
                    try
                    {
                        var nombrePdf = Path.GetFileName(rutaPdf);
                        var rutaRelativa = Path.Combine("Uploads", "Manuales", nombrePdf);
                        var textoPdf = await _servicioAsistenteIA.ExtraerTextoPdfAsync(rutaRelativa);
                        if (!string.IsNullOrWhiteSpace(textoPdf))
                        {
                            var fragmento = textoPdf.Length > 4500
                                ? textoPdf[..4500] + "\n...[MANUAL TRUNCADO]..."
                                : textoPdf;
                            sb.AppendLine($"--- Manual: {nombrePdf} ---");
                            sb.AppendLine(fragmento);
                        }
                    }
                    catch
                    {
                        // Si un PDF está dañado se ignora sin bloquear el chat
                    }
                }
            }
        }

        return sb.ToString();
    }
}