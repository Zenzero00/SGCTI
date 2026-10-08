using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sgcti.Api.Services;
using Sgcti.Core.Dtos;
using Sgcti.Core.Entities;
using Sgcti.Core.Services;
using Sgcti.Infrastructure.Persistence;

namespace Sgcti.Api.Controllers;

public class ImpresorasController : SgctiControllerBase<Impresora>
{
    private readonly ServicioAnalisisPredictivo _servicioAnalisis;

    private readonly IServicioBuscadorManuales _servicioBuscadorManuales;

    private readonly IServicioAsistenteIA _servicioAsistenteIA;

    public ImpresorasController(
        SgctiDbContext db,
        ServicioAnalisisPredictivo servicioAnalisis,
        IServicioBuscadorManuales servicioBuscadorManuales,
        IServicioAsistenteIA servicioAsistenteIA) : base(db)
    {
        _servicioAnalisis = servicioAnalisis;
        _servicioBuscadorManuales = servicioBuscadorManuales;
        _servicioAsistenteIA = servicioAsistenteIA;
    }

    [NonAction]
    public override Task<ActionResult<Impresora>> Create(Impresora entity, CancellationToken ct)
        => throw new NotSupportedException("La creación de impresoras debe usar el DTO CrearImpresoraDto.");

    [NonAction]
    public override Task<IActionResult> Update(int id, Impresora entity, CancellationToken ct)
        => throw new NotSupportedException("La actualización de impresoras debe usar el DTO ActualizarImpresoraDto.");

    [NonAction]
    public override Task<IActionResult> Delete(int id, CancellationToken ct)
        => throw new NotSupportedException("La eliminación de impresoras se expone en EliminarImpresora.");

    [HttpPost]
    public async Task<ActionResult<Impresora>> CrearImpresora(
        [FromBody] CrearImpresoraDto dto, CancellationToken ct)
    {
        if (dto is null)
        {
            return BadRequest("Los datos de la impresora no pueden estar vacíos.");
        }

        if (await ExisteIpAsync(dto.Ip.Trim(), null, ct))
        {
            return BadRequest("Ya existe una impresora con esa IP.");
        }

        var impresora = new Impresora
        {
            Modelo = dto.Modelo.Trim(),
            Ip = dto.Ip.Trim(),
            Departamento = dto.Departamento.Trim(),
            ComunidadSnmp = NormalizarComunidadSnmp(dto.ComunidadSnmp),
            Fabricante = "Desconocido",
            FechaInstalacion = DateTime.UtcNow
        };

        Db.Impresoras.Add(impresora);
        await Db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = impresora.Id }, impresora);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> ActualizarImpresora(
        int id, [FromBody] ActualizarImpresoraDto dto, CancellationToken ct)
    {
        if (dto is null)
        {
            return BadRequest("Los datos de la impresora no pueden estar vacíos.");
        }

        var impresora = await Db.Impresoras.FindAsync(new object[] { id }, ct);

        if (impresora is null)
        {
            return NotFound();
        }

        if (await ExisteIpAsync(dto.Ip.Trim(), id, ct))
        {
            return BadRequest("Ya existe una impresora con esa IP.");
        }

        impresora.Modelo = dto.Modelo.Trim();
        impresora.Ip = dto.Ip.Trim();
        impresora.Departamento = dto.Departamento.Trim();
        impresora.ComunidadSnmp = NormalizarComunidadSnmp(dto.ComunidadSnmp);

        await Db.SaveChangesAsync(ct);

        await _servicioAnalisis.CalcularPredicciones(id);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> EliminarImpresora(int id, CancellationToken ct)
    {
        var impresora = await Db.Impresoras.FindAsync(new object[] { id }, ct);

        if (impresora is null)
        {
            return NotFound();
        }

        Db.Impresoras.Remove(impresora);
        await Db.SaveChangesAsync(ct);

        return NoContent();
    }

    private static string NormalizarComunidadSnmp(string? comunidad)
        => string.IsNullOrWhiteSpace(comunidad)
            ? Impresora.ComunidadSnmpPorDefecto
            : comunidad.Trim();

    private async Task<bool> ExisteIpAsync(
        string ip, int? excluirId, CancellationToken ct)
    {
        return await Db.Impresoras.AnyAsync(
            i => i.Ip == ip && i.Id != excluirId, ct);
    }

    [HttpGet("{id:int}/buscar-manuales")]
    public async Task<ActionResult<string>> BuscarManuales(int id, CancellationToken ct)
    {
        var impresora = await Db.Impresoras.FindAsync(new object[] { id }, ct);

        if (impresora is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(impresora.Modelo))
        {
            return BadRequest("La impresora no tiene un modelo registrado para buscar manuales.");
        }

        return Ok(_servicioBuscadorManuales.ConstruirUrlBusqueda(impresora.Modelo));
    }

    [HttpPost("{id}/subir-manual")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SubirManual(
        int id, IFormFile? archivo, CancellationToken ct)
    {
        var impresora = await Db.Impresoras.FindAsync(new object[] { id }, ct);

        if (impresora is null)
        {
            return NotFound();
        }

        if (archivo is null || archivo.Length <= 0)
        {
            return BadRequest("El archivo no puede ser nulo o estar vacío.");
        }

        var esPdf = archivo.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)
            || Path.GetExtension(archivo.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase);

        if (!esPdf)
        {
            return BadRequest("El archivo debe ser un PDF.");
        }

        var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Manuales");
        Directory.CreateDirectory(carpeta);

        var nombreArchivo = $"manual_impresora_{id}.pdf";
        var rutaFisica = Path.Combine(carpeta, nombreArchivo);

        await using (var stream = new FileStream(rutaFisica, FileMode.Create))
        {
            await archivo.CopyToAsync(stream, ct);
        }

        var rutaRelativa = $"Uploads/Manuales/{nombreArchivo}";

        impresora.RutaManual = rutaRelativa;
        await Db.SaveChangesAsync(ct);

        return Ok(new { Mensaje = "Manual subido correctamente.", Ruta = rutaRelativa });
    }

    [HttpPost("importar-excel")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportarExcel(IFormFile? archivo, CancellationToken ct)
    {
        if (archivo is null || archivo.Length <= 0)
        {
            return BadRequest("El archivo no puede ser nulo o estar vacío.");
        }

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();

        if (extension is not ".xlsx" and not ".xls")
        {
            return BadRequest("El archivo debe ser un Excel (.xlsx o .xls).");
        }

        var registrosNuevos = 0;
        var impresorasCreadas = 0;
        var cacheImpresoras = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var clavesExistentes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using (var stream = new MemoryStream())
        {
            await archivo.CopyToAsync(stream, ct);
            stream.Position = 0;

            using var libro = new XLWorkbook(stream);

            foreach (var hoja in libro.Worksheets)
            {
                var ultimaFila = hoja.LastRowUsed()?.RowNumber() ?? 0;
                var ultimaColumna = hoja.LastColumnUsed()?.ColumnNumber() ?? 0;

                if (ultimaFila < 3 || ultimaColumna < 1)
                {
                    continue;
                }

                var grupos = DetectarColumnasPorImpresora(hoja, ultimaColumna);

                if (grupos.Count == 0)
                {
                    continue;
                }

                var impresorasDeLaHoja = new List<int>();

                foreach (var grupo in grupos)
                {
                    if (string.IsNullOrWhiteSpace(grupo.Nombre))
                    {
                        continue;
                    }

                    var (impresoraId, creada) = await ObtenerOCrearImpresoraAsync(
                        grupo.Nombre, cacheImpresoras, ct);

                    if (creada)
                    {
                        impresorasCreadas++;
                    }

                    grupo.ImpresoraId = impresoraId;
                    impresorasDeLaHoja.Add(impresoraId);
                }

                if (impresorasDeLaHoja.Count == 0)
                {
                    continue;
                }

                var fechasYaRegistradas = await Db.RegistrosConsumoDiario
                    .AsNoTracking()
                    .Where(r => impresorasDeLaHoja.Contains(r.ImpresoraId))
                    .Select(r => r.Fecha.Date)
                    .ToListAsync(ct);

                var fechasExistentes = fechasYaRegistradas.ToHashSet();

                foreach (var grupo in grupos)
                {
                    if (grupo.ImpresoraId is null || !grupo.ColumnaFecha.HasValue)
                    {
                        continue;
                    }

                    for (int fila = 3; fila <= ultimaFila; fila++)
                    {
                        var celdaFecha = hoja.Cell(fila, grupo.ColumnaFecha.Value);

                        if (!IntentaLeerFecha(celdaFecha, out var fecha))
                        {
                            continue;
                        }

                        var fechaUtc = DateTime.SpecifyKind(fecha.Date, DateTimeKind.Utc);

                        var contador = grupo.ColumnaContador.HasValue
                            ? IntentaLeerEntero(hoja.Cell(fila, grupo.ColumnaContador.Value)) ?? 0
                            : 0;

                        var consumo = grupo.ColumnaConsumo.HasValue
                            ? IntentaLeerEntero(hoja.Cell(fila, grupo.ColumnaConsumo.Value)) ?? 0
                            : 0;

                        var observacion = grupo.ColumnaObservacion.HasValue
                            ? hoja.Cell(fila, grupo.ColumnaObservacion.Value).GetString().Trim()
                            : string.Empty;

                        var clave = $"{grupo.ImpresoraId}|{fechaUtc:yyyy-MM-dd}";

                        if (fechasExistentes.Contains(fechaUtc) || clavesExistentes.Contains(clave))
                        {
                            continue;
                        }

                        clavesExistentes.Add(clave);

                        Db.RegistrosConsumoDiario.Add(new RegistroConsumoDiario
                        {
                            ImpresoraId = grupo.ImpresoraId.Value,
                            Fecha = fechaUtc,
                            Contador = contador,
                            Consumo = consumo,
                            Observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion
                        });

                        registrosNuevos++;
                    }
                }
            }
        }

        await Db.SaveChangesAsync(ct);

        return Ok(new { RegistrosImportados = registrosNuevos, ImpresorasCreadas = impresorasCreadas });
    }

    [HttpPost("{id:int}/chat")]
    public async Task<IActionResult> Chat(int id, [FromBody] ConsultaIA_Dto consulta, CancellationToken ct)
    {
        if (consulta is null || string.IsNullOrWhiteSpace(consulta.Pregunta))
        {
            return BadRequest("La pregunta no puede estar vacía.");
        }

        var impresora = await Db.Impresoras.FindAsync(new object[] { id }, ct);

        if (impresora is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(impresora.RutaManual))
        {
            return BadRequest("Sube un manual primero");
        }

        string textoManual;

        try
        {
            textoManual = await _servicioAsistenteIA.ExtraerTextoPdfAsync(impresora.RutaManual);
        }
        catch (FileNotFoundException)
        {
            return BadRequest("Sube un manual primero");
        }

        var resultado = await _servicioAsistenteIA.ConsultarChatbotAsync(textoManual, consulta.Pregunta);

        return Ok(new { respuesta = resultado });
    }

    [HttpGet("{id:int}/mantenimientos")]
    public async Task<ActionResult<IEnumerable<MantenimientoImpresora>>> ObtenerMantenimientos(
        int id, CancellationToken ct)
    {
        var impresora = await Db.Impresoras.FindAsync(new object[] { id }, ct);

        if (impresora is null)
        {
            return NotFound();
        }

        var mantenimientos = await Db.MantenimientosImpresora
            .AsNoTracking()
            .Where(m => m.ImpresoraId == id)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync(ct);

        return Ok(mantenimientos);
    }

    [HttpPost("{id:int}/mantenimientos")]
    public async Task<IActionResult> CrearMantenimiento(
        int id, [FromBody] CrearMantenimientoDto dto, CancellationToken ct)
    {
        if (dto is null)
        {
            return BadRequest("Los datos del mantenimiento no pueden estar vacíos.");
        }

        var impresora = await Db.Impresoras.FindAsync(new object[] { id }, ct);

        if (impresora is null)
        {
            return NotFound();
        }

        Db.MantenimientosImpresora.Add(new MantenimientoImpresora
        {
            ImpresoraId = id,
            Fecha = DateTime.UtcNow,
            Tipo = dto.Tipo.Trim(),
            Descripcion = dto.Descripcion.Trim(),
            RealizadoPor = dto.RealizadoPor.Trim()
        });

        await Db.SaveChangesAsync(ct);

        await _servicioAnalisis.CalcularPredicciones(id);

        return Ok();
    }

    private static List<GrupoImpresora> DetectarColumnasPorImpresora(IXLWorksheet hoja, int ultimaColumna)
    {
        var filaImpresoras = hoja.Row(1);
        var filaEncabezados = hoja.Row(2);

        var grupos = new List<GrupoImpresora>();
        GrupoImpresora? grupoActual = null;

        for (int columna = 1; columna <= ultimaColumna; columna++)
        {
            var encabezado = Normalizar(filaEncabezados.Cell(columna).GetString());

            if (encabezado == "fecha")
            {
                grupoActual = new GrupoImpresora
                {
                    ColumnaFecha = columna
                };

                grupos.Add(grupoActual);
                continue;
            }

            if (grupoActual is null)
            {
                continue;
            }

            switch (encabezado)
            {
                case "contador":
                    grupoActual.ColumnaContador = columna;
                    break;
                case "consumo":
                    grupoActual.ColumnaConsumo = columna;
                    break;
                case "observacion":
                    grupoActual.ColumnaObservacion = columna;
                    break;
            }
        }

        for (int i = 0; i < grupos.Count; i++)
        {
            var inicio = grupos[i].ColumnaFecha!.Value;
            var fin = i + 1 < grupos.Count ? grupos[i + 1].ColumnaFecha!.Value - 1 : ultimaColumna;

            grupos[i].Nombre = BuscarNombreImpresora(filaImpresoras, inicio, fin);
        }

        return grupos;
    }

    private static string? BuscarNombreImpresora(IXLRow filaImpresoras, int inicio, int fin)
    {
        for (int columna = inicio; columna <= fin; columna++)
        {
            var valor = filaImpresoras.Cell(columna).GetString().Trim();

            if (!string.IsNullOrWhiteSpace(valor))
            {
                return valor;
            }
        }

        return null;
    }

    private static string Normalizar(string texto)
    {
        var plano = texto
            .Trim()
            .Normalize(System.Text.NormalizationForm.FormD);

        var sinAcentos = string.Concat(plano.Where(c =>
            System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) !=
            System.Globalization.UnicodeCategory.NonSpacingMark));

        return sinAcentos.ToLowerInvariant();
    }

    private static bool IntentaLeerFecha(IXLCell celda, out DateTime fecha)
    {
        if (celda.TryGetValue<DateTime>(out fecha))
        {
            return fecha != default;
        }

        var texto = celda.GetString().Trim();

        return DateTime.TryParse(texto, out fecha) || DateTime.TryParseExact(
            texto,
            new[] { "dd/MM/yyyy", "yyyy-MM-dd", "dd-MM-yyyy" },
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out fecha);
    }

    private static int? IntentaLeerEntero(IXLCell celda)
    {
        if (celda.TryGetValue<int>(out var entero))
        {
            return entero;
        }

        if (long.TryParse(celda.GetString().Trim(), out var largo))
        {
            return (int)largo;
        }

        return null;
    }

    private async ValueTask<(int Id, bool Creada)> ObtenerOCrearImpresoraAsync(
        string nombre, Dictionary<string, int> cache, CancellationToken ct)
    {
        if (cache.TryGetValue(nombre, out var idExistente))
        {
            return (idExistente, false);
        }

        var impresora = await Db.Impresoras
            .FirstOrDefaultAsync(i => i.Modelo == nombre, ct);

        if (impresora is null)
        {
            impresora = new Impresora
            {
                Modelo = nombre,
                Ip = $"importado-{Guid.NewGuid():N}",
                Fabricante = "Desconocido",
                Departamento = Impresora.DepartamentoPorDefecto,
                FechaInstalacion = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc)
            };

            Db.Impresoras.Add(impresora);
            await Db.SaveChangesAsync(ct);

            cache[nombre] = impresora.Id;

            return (impresora.Id, true);
        }

        cache[nombre] = impresora.Id;

        return (impresora.Id, false);
    }

    private sealed class GrupoImpresora
    {
        public string? Nombre { get; set; }

        public int? ImpresoraId { get; set; }

        public int? ColumnaFecha { get; set; }

        public int? ColumnaContador { get; set; }

        public int? ColumnaConsumo { get; set; }

        public int? ColumnaObservacion { get; set; }
    }
}
