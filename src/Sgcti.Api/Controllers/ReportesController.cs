using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sgcti.Core.Dtos;
using Sgcti.Core.Entities;
using Sgcti.Infrastructure.Persistence;

namespace Sgcti.Api.Controllers;

[ApiController]
[Route("api/reportes")]
public class ReportesController : ControllerBase
{
    private readonly SgctiDbContext _db;

    public ReportesController(SgctiDbContext db)
    {
        _db = db;
    }

    [HttpGet("consumo-impresoras")]
    public async Task<IActionResult> ConsumoImpresoras(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        CancellationToken ct)
    {
        if (!fechaInicio.HasValue || !fechaFin.HasValue)
        {
            return BadRequest("Debe especificar los parámetros 'fechaInicio' y 'fechaFin'.");
        }

        var inicio = fechaInicio.Value.Date;
        var fin = fechaFin.Value.Date;

        if (inicio > fin)
        {
            return BadRequest("La fecha de inicio no puede ser posterior a la fecha de fin.");
        }

        var inicioUtc = DateTime.SpecifyKind(inicio, DateTimeKind.Utc);
        var finUtc = DateTime.SpecifyKind(fin, DateTimeKind.Utc).AddDays(1).AddTicks(-1);

        var registros = await _db.RegistrosConsumoDiario
            .AsNoTracking()
            .Include(r => r.Impresora)
            .Where(r => r.Fecha >= inicioUtc && r.Fecha <= finUtc)
            .OrderBy(r => r.Impresora.Departamento)
            .ThenBy(r => r.Impresora.Modelo)
            .ThenBy(r => r.Fecha)
            .ToListAsync(ct);

        var agrupados = registros
            .GroupBy(r => r.ImpresoraId)
            .Select(g => new GrupoConsumo(g.First().Impresora, g.ToList()))
            .ToList();

        var fechaGeneracion = DateTime.Now;

        var pdfBytes = Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("Reporte de Consumo de Impresoras CENDI")
                        .FontSize(18)
                        .Bold()
                        .AlignCenter();

                    col.Item().PaddingTop(4)
                        .Text($"Periodo: {inicio:dd/MM/yyyy} al {fin:dd/MM/yyyy}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1)
                        .AlignCenter();

                    col.Item().PaddingTop(2)
                        .Text($"Fecha de generación: {fechaGeneracion:dd/MM/yyyy HH:mm:ss}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1)
                        .AlignCenter();

                    col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                });

                page.Content().PaddingVertical(16).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(4);
                    });

                    table.Header(header =>
                    {
                        foreach (var titulo in new[] { "Fecha", "Contador", "Consumo", "Observación" })
                        {
                            header.Cell().Background(Colors.Blue.Darken2).Padding(8)
                                .Text(titulo).FontColor(Colors.White).Bold();
                        }
                    });

                    if (agrupados.Count == 0)
                    {
                        table.Cell().ColumnSpan(4).Padding(12).AlignCenter()
                            .Text("No hay registros de consumo en el periodo seleccionado.")
                            .FontColor(Colors.Grey.Darken1);
                    }

                    foreach (var grupo in agrupados)
                    {
                        var departamento = string.IsNullOrWhiteSpace(grupo.Impresora.Departamento)
                            ? "Sin departamento"
                            : grupo.Impresora.Departamento;

                        table.Cell().ColumnSpan(4).Padding(8).Background(Colors.Grey.Lighten3)
                            .Text($"{grupo.Impresora.Modelo} · {grupo.Impresora.Ip} — {departamento}")
                            .Bold()
                            .FontSize(11);

                        foreach (var registro in grupo.Registros)
                        {
                            Celda(table, registro.Fecha.ToString("dd/MM/yyyy"));
                            Celda(table, registro.Contador.ToString("N0"));
                            Celda(table, registro.Consumo.ToString("N0"));
                            Celda(table, registro.Observacion ?? "");
                        }
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Página ");
                    text.CurrentPageNumber();
                    text.Span(" de ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf();

        return File(pdfBytes, "application/pdf", "ConsumoImpresorasCendi.pdf");
    }

    [HttpGet("grafica-consumo")]
    public async Task<ActionResult<IEnumerable<object>>> GraficaConsumo(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        CancellationToken ct)
    {
        if (!fechaInicio.HasValue || !fechaFin.HasValue)
        {
            return BadRequest("Debe especificar los parámetros 'fechaInicio' y 'fechaFin'.");
        }

        var inicio = fechaInicio.Value.Date;
        var fin = fechaFin.Value.Date;

        if (inicio > fin)
        {
            return BadRequest("La fecha de inicio no puede ser posterior a la fecha de fin.");
        }

        var inicioUtc = DateTime.SpecifyKind(inicio, DateTimeKind.Utc);
        var finUtc = DateTime.SpecifyKind(fin, DateTimeKind.Utc).AddDays(1).AddTicks(-1);

        var datos = await _db.RegistrosConsumoDiario
            .AsNoTracking()
            .Where(r => r.Fecha >= inicioUtc && r.Fecha <= finUtc)
            .GroupBy(r => r.Impresora.Modelo)
            .Select(g => new { impresora = g.Key, total = g.Sum(x => x.Consumo) })
            .OrderByDescending(g => g.total)
            .ToListAsync(ct);

        return Ok(datos);
    }

    private static void Celda(TableDescriptor table, string contenido)
    {
        table.Cell().Padding(8).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
            .Text(contenido);
    }

    [HttpGet("datos-consumo")]
    public async Task<ActionResult<IEnumerable<object>>> DatosConsumo(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        CancellationToken ct)
    {
        if (!fechaInicio.HasValue || !fechaFin.HasValue)
        {
            return BadRequest("Debe especificar los parámetros 'fechaInicio' y 'fechaFin'.");
        }

        var inicio = fechaInicio.Value.Date;
        var fin = fechaFin.Value.Date;

        if (inicio > fin)
        {
            return BadRequest("La fecha de inicio no puede ser posterior a la fecha de fin.");
        }

        var inicioUtc = DateTime.SpecifyKind(inicio, DateTimeKind.Utc);
        var finUtc = DateTime.SpecifyKind(fin, DateTimeKind.Utc).AddDays(1).AddTicks(-1);

        var registros = await _db.RegistrosConsumoDiario
            .AsNoTracking()
            .Where(r => r.Fecha >= inicioUtc && r.Fecha <= finUtc)
            .OrderByDescending(r => r.Fecha)
            .ThenBy(r => r.Impresora.Modelo)
            .Select(r => new
            {
                r.Id,
                r.Fecha,
                r.Contador,
                r.Consumo,
                r.Observacion,
                Impresora = r.Impresora.Modelo
            })
            .ToListAsync(ct);

        return Ok(registros);
    }

    [HttpPut("consumo/{id:int}/observacion")]
    public async Task<IActionResult> ActualizarObservacion(int id, ActualizarObservacionDto dto, CancellationToken ct)
    {
        var registro = await _db.RegistrosConsumoDiario.FindAsync(new object[] { id }, ct);

        if (registro is null)
        {
            return NotFound();
        }

        registro.Observacion = string.IsNullOrWhiteSpace(dto.Observacion) ? null : dto.Observacion.Trim();

        await _db.SaveChangesAsync(ct);

        return Ok();
    }

    private sealed record GrupoConsumo(Impresora Impresora, List<RegistroConsumoDiario> Registros);
}