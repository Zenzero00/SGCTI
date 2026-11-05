using System.Globalization;
using System.Text;
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
    private static readonly Dictionary<string, string> AliasModelos = new(StringComparer.OrdinalIgnoreCase)
    {
        ["IR 1643iF Facturacion Total"] = "Facturacion IR1643 Total",
        ["IR 1643iF Facturacion Stargas"] = "Facturacion IR1643 Stargas",
        ["RICOH Cendi"] = "RICOH MP3350 Cendi"
    };

    private static readonly Dictionary<string, int> MesesPorNombre = new(StringComparer.OrdinalIgnoreCase)
    {
        ["enero"] = 1,
        ["febrero"] = 2,
        ["marzo"] = 3,
        ["abril"] = 4,
        ["mayo"] = 5,
        ["junio"] = 6,
        ["julio"] = 7,
        ["agosto"] = 8,
        ["septiembre"] = 9,
        ["setiembre"] = 9,
        ["octubre"] = 10,
        ["noviembre"] = 11,
        ["diciembre"] = 12
    };

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

        var (nombreVisible, modeloTecnico) = ExtraerNombreYModeloTecnico(dto.Modelo);

        var impresora = new Impresora
        {
            Modelo = nombreVisible,
            Fabricante = modeloTecnico,
            Ip = dto.Ip.Trim(),
            Departamento = dto.Departamento.Trim(),
            ComunidadSnmp = NormalizarComunidadSnmp(dto.ComunidadSnmp),
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

        var (nombreVisible, modeloTecnico) = ExtraerNombreYModeloTecnico(dto.Modelo, impresora.Fabricante);

        impresora.Modelo = nombreVisible;
        impresora.Fabricante = modeloTecnico;
        impresora.Ip = dto.Ip.Trim();
        impresora.Departamento = dto.Departamento.Trim();
        impresora.ComunidadSnmp = NormalizarComunidadSnmp(dto.ComunidadSnmp);

        await Db.SaveChangesAsync(ct);

        return NoContent();
    }

    private static (string NombreVisible, string ModeloTecnico) ExtraerNombreYModeloTecnico(
        string entrada, string? fabricanteActual = null)
    {
        if (entrada.Contains("||"))
        {
            var partes = entrada.Split("||", 2, StringSplitOptions.TrimEntries);
            var nombre = string.IsNullOrWhiteSpace(partes[0]) ? "Impresora en Red" : partes[0];
            var modelo = partes.Length > 1 && !string.IsNullOrWhiteSpace(partes[1])
                ? partes[1]
                : InferirFabricante(nombre);
            return (nombre, modelo);
        }

        var limpio = entrada.Trim();
        var fab = !string.IsNullOrWhiteSpace(fabricanteActual) ? fabricanteActual : InferirFabricante(limpio);
        return (limpio, fab);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> EliminarImpresora(int id, CancellationToken ct)
    {
        var impresora = await Db.Impresoras.FindAsync(new object[] { id }, ct);

        if (impresora is null)
        {
            return NotFound();
        }

        var consumos = await Db.RegistrosConsumoDiario.Where(r => r.ImpresoraId == id).ToListAsync(ct);
        var mants = await Db.MantenimientosImpresora.Where(m => m.ImpresoraId == id).ToListAsync(ct);
        Db.RegistrosConsumoDiario.RemoveRange(consumos);
        Db.MantenimientosImpresora.RemoveRange(mants);

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

        var fab = impresora.Fabricante?.Trim() ?? string.Empty;
        var mod = impresora.Modelo?.Trim() ?? string.Empty;

        // Si en Fabricante se guardó el modelo técnico específico (ej. "HP Smart Tank 215"), buscar por ese modelo técnico
        string terminoBusqueda;
        if (!string.IsNullOrWhiteSpace(fab) &&
            !string.Equals(fab, "Canon", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(fab, "HP", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(fab, "Ricoh", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(fab, "Epson", StringComparison.OrdinalIgnoreCase))
        {
            terminoBusqueda = fab;
        }
        else if (!string.IsNullOrWhiteSpace(fab) && !mod.Contains(fab, StringComparison.OrdinalIgnoreCase))
        {
            terminoBusqueda = $"{fab} {mod}".Trim();
        }
        else
        {
            terminoBusqueda = mod;
        }

        if (string.IsNullOrWhiteSpace(terminoBusqueda))
        {
            return BadRequest("La impresora no tiene un modelo registrado para buscar manuales.");
        }

        return Ok(_servicioBuscadorManuales.ConstruirUrlBusqueda(terminoBusqueda));
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

        using var stream = new MemoryStream();
        await archivo.CopyToAsync(stream, ct);
        stream.Position = 0;

        var (registrosImportados, impresorasCreadas) = await ImportarLibroConsumoAsync(Db, stream, ct);

        return Ok(new { RegistrosImportados = registrosImportados, ImpresorasCreadas = impresorasCreadas });
    }

    public static async Task<(int RegistrosImportados, int ImpresorasCreadas)> ImportarLibroConsumoAsync(
        SgctiDbContext db,
        Stream stream,
        CancellationToken ct = default)
    {
        var registrosNuevosOActualizados = 0;
        var impresorasCreadas = 0;
        var cacheImpresoras = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        var impresorasExistentes = await db.Impresoras.ToListAsync(ct);
        foreach (var imp in impresorasExistentes)
        {
            var nombreCanonico = NormalizarNombreImpresora(imp.Modelo);
            if (!cacheImpresoras.ContainsKey(nombreCanonico))
            {
                cacheImpresoras[nombreCanonico] = imp.Id;
            }
        }

        var registrosExistentesLista = await db.RegistrosConsumoDiario.ToListAsync(ct);
        var mapaRegistrosExistentes = new Dictionary<string, RegistroConsumoDiario>(StringComparer.OrdinalIgnoreCase);
        foreach (var reg in registrosExistentesLista)
        {
            var clave = $"{reg.ImpresoraId}|{reg.Fecha:yyyy-MM-dd}";
            mapaRegistrosExistentes[clave] = reg;
        }

        using var libro = new XLWorkbook(stream);

        foreach (var hoja in libro.Worksheets)
        {
            var ultimaFila = hoja.LastRowUsed()?.RowNumber() ?? 0;
            var ultimaColumna = hoja.LastColumnUsed()?.ColumnNumber() ?? 0;

            if (ultimaFila < 3 || ultimaColumna < 1)
            {
                continue;
            }

            var (anioEsperado, mesEsperado) = ExtraerMesYAnioDeNombreHoja(hoja.Name);
            var bloques = DetectarBloquesDeImpresoras(hoja, ultimaFila, ultimaColumna);

            foreach (var bloque in bloques)
            {
                foreach (var grupo in bloque.Grupos)
                {
                    if (string.IsNullOrWhiteSpace(grupo.Nombre) || !grupo.ColumnaFecha.HasValue)
                    {
                        continue;
                    }

                    var nombreNormalizado = NormalizarNombreImpresora(grupo.Nombre);
                    var (impresoraId, creada) = await ObtenerOCrearImpresoraStaticAsync(
                        db,
                        nombreNormalizado,
                        cacheImpresoras,
                        ct);

                    if (creada)
                    {
                        impresorasCreadas++;
                    }

                    var ordenDiaEnBloque = 0;

                    for (int fila = bloque.FilaInicioDatos; fila <= bloque.FilaFinDatos; fila++)
                    {
                        var celdaFecha = hoja.Cell(fila, grupo.ColumnaFecha.Value);

                        if (!IntentaLeerFecha(celdaFecha, out var fechaLeida))
                        {
                            continue;
                        }

                        ordenDiaEnBloque++;
                        var fechaCorregida = CorregirFechaSegunHoja(
                            fechaLeida,
                            anioEsperado,
                            mesEsperado,
                            ordenDiaEnBloque);

                        var fechaUtc = DateTime.SpecifyKind(fechaCorregida.Date, DateTimeKind.Utc);

                        var contador = grupo.ColumnaContador.HasValue
                            ? IntentaLeerEntero(hoja.Cell(fila, grupo.ColumnaContador.Value)) ?? 0
                            : 0;

                        var consumo = 0;
                        var observacionExtra = string.Empty;

                        if (grupo.ColumnaConsumo.HasValue)
                        {
                            var celdaConsumo = hoja.Cell(fila, grupo.ColumnaConsumo.Value);
                            var enteroConsumo = IntentaLeerEntero(celdaConsumo);
                            if (enteroConsumo.HasValue)
                            {
                                consumo = Math.Max(0, enteroConsumo.Value);
                            }
                            else
                            {
                                var textoEnConsumo = celdaConsumo.GetString().Trim();
                                if (!string.IsNullOrWhiteSpace(textoEnConsumo))
                                {
                                    observacionExtra = textoEnConsumo;
                                }
                            }
                        }

                        var observacion = grupo.ColumnaObservacion.HasValue
                            ? hoja.Cell(fila, grupo.ColumnaObservacion.Value).GetString().Trim()
                            : string.Empty;

                        if (!string.IsNullOrWhiteSpace(observacionExtra))
                        {
                            observacion = string.IsNullOrWhiteSpace(observacion)
                                ? observacionExtra
                                : $"{observacion} - {observacionExtra}";
                        }

                        var clave = $"{impresoraId}|{fechaUtc:yyyy-MM-dd}";

                        if (mapaRegistrosExistentes.TryGetValue(clave, out var existente))
                        {
                            if (existente.Contador != contador ||
                                existente.Consumo != consumo ||
                                (!string.IsNullOrWhiteSpace(observacion) && string.IsNullOrWhiteSpace(existente.Observacion)))
                            {
                                existente.Contador = contador;
                                existente.Consumo = consumo;
                                if (!string.IsNullOrWhiteSpace(observacion))
                                {
                                    existente.Observacion = observacion;
                                }
                                registrosNuevosOActualizados++;
                            }
                            continue;
                        }

                        var nuevoRegistro = new RegistroConsumoDiario
                        {
                            ImpresoraId = impresoraId,
                            Fecha = fechaUtc,
                            Contador = contador,
                            Consumo = consumo,
                            Observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion
                        };

                        db.RegistrosConsumoDiario.Add(nuevoRegistro);
                        mapaRegistrosExistentes[clave] = nuevoRegistro;
                        registrosNuevosOActualizados++;
                    }
                }
            }
        }

        await db.SaveChangesAsync(ct);

        return (registrosNuevosOActualizados, impresorasCreadas);
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

        var sb = new StringBuilder();
        sb.AppendLine("=== IMPRESORA SELECCIONADA ===");
        sb.AppendLine($"Id:{impresora.Id} | Nombre:{impresora.Modelo} | ModeloTecnico:{impresora.Fabricante} | IP:{impresora.Ip} | EstadoPing:{impresora.EstadoPing} | NivelTonnerNegro:{impresora.NivelTonnerNegro}% | ManualCargado:{(string.IsNullOrWhiteSpace(impresora.RutaManual) ? "NO" : "SI")}");

        sb.AppendLine();
        sb.AppendLine("=== INVENTARIO: SUMINISTROS ===");
        var suministros = await Db.Suministros.AsNoTracking().ToListAsync(ct);
        if (suministros.Count == 0) sb.AppendLine("Sin registros.");
        else foreach (var s in suministros) { var critico = s.CantidadActual <= s.StockMinimo ? " [CRÍTICO]" : ""; sb.AppendLine($"- {s.Nombre} | Tipo:{s.Tipo} | Cantidad:{s.CantidadActual} | StockMin:{s.StockMinimo}{critico}"); }

        if (!string.IsNullOrWhiteSpace(impresora.RutaManual))
        {
            sb.AppendLine();
            sb.AppendLine("=== MANUAL VINCULADO ===");
            try
            {
                var textoManual = await _servicioAsistenteIA.ExtraerTextoPdfAsync(impresora.RutaManual);
                var frag = textoManual.Length > 3000 ? textoManual.Substring(0, 3000) + "\n...[TRUNCADO]..." : textoManual;
                sb.AppendLine(frag);
            }
            catch { sb.AppendLine("(No se pudo extraer el manual)"); }
        }

        var resultado = await _servicioAsistenteIA.ConsultarChatbotConContextoAsync(consulta.Pregunta, sb.ToString());
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

        var ahora = DateTime.UtcNow;

        Db.MantenimientosImpresora.Add(new MantenimientoImpresora
        {
            ImpresoraId = id,
            Fecha = ahora,
            Tipo = dto.Tipo.Trim(),
            Descripcion = dto.Descripcion.Trim(),
            RealizadoPor = dto.RealizadoPor.Trim()
        });

        impresora.UltimoMantenimiento = ahora;
        impresora.PaginasDesdeUltimoMantenimiento = 0;

        var promedio = Math.Clamp(impresora.PromedioPaginasDiario > 0 ? impresora.PromedioPaginasDiario : 140.0, 75.0, 420.0);
        impresora.DiasEstimadosMantenimiento = impresora.EstadoPing == EstadoPing.EnLinea
            ? (int)Math.Max(30, Math.Round(15000.0 / promedio))
            : 0;
        impresora.FechaEstimadaMantenimiento = ahora.AddDays(Math.Max(30, impresora.DiasEstimadosMantenimiento));

        await Db.SaveChangesAsync(ct);

        return Ok();
    }

    private static List<BloqueImpresoras> DetectarBloquesDeImpresoras(
        IXLWorksheet hoja,
        int ultimaFila,
        int ultimaColumna)
    {
        var filasEncabezado = new List<int>();

        for (int fila = 2; fila <= ultimaFila; fila++)
        {
            var r = hoja.Row(fila);
            for (int col = 1; col <= ultimaColumna; col++)
            {
                if (Normalizar(r.Cell(col).GetString()) == "fecha")
                {
                    filasEncabezado.Add(fila);
                    break;
                }
            }
        }

        var bloques = new List<BloqueImpresoras>();

        for (int idxBloque = 0; idxBloque < filasEncabezado.Count; idxBloque++)
        {
            var filaEncabezadosNum = filasEncabezado[idxBloque];
            var filaImpresorasNum = filaEncabezadosNum - 1;
            var filaFinDatos = idxBloque + 1 < filasEncabezado.Count
                ? filasEncabezado[idxBloque + 1] - 2
                : ultimaFila;

            var filaImpresoras = hoja.Row(filaImpresorasNum);
            var filaEncabezados = hoja.Row(filaEncabezadosNum);

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

                if (!grupos[i].ColumnaContador.HasValue &&
                    grupos[i].ColumnaConsumo.HasValue &&
                    grupos[i].ColumnaConsumo!.Value == inicio + 2)
                {
                    grupos[i].ColumnaContador = inicio + 1;
                }
            }

            bloques.Add(new BloqueImpresoras
            {
                FilaInicioDatos = filaEncabezadosNum + 1,
                FilaFinDatos = filaFinDatos,
                Grupos = grupos
            });
        }

        return bloques;
    }

    private static string? BuscarNombreImpresora(IXLRow filaImpresoras, int inicio, int fin)
    {
        for (int columna = inicio; columna <= fin; columna++)
        {
            var valor = LimpiarEspacios(filaImpresoras.Cell(columna).GetString());

            if (!string.IsNullOrWhiteSpace(valor))
            {
                return valor;
            }
        }

        return null;
    }

    private static string NormalizarNombreImpresora(string nombre)
    {
        var limpio = LimpiarEspacios(nombre);
        return AliasModelos.TryGetValue(limpio, out var canonico) ? canonico : limpio;
    }

    private static string LimpiarEspacios(string texto)
        => string.Join(' ', texto.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries));

    private static (int? Anio, int? Mes) ExtraerMesYAnioDeNombreHoja(string nombreHoja)
    {
        var partes = LimpiarEspacios(Normalizar(nombreHoja)).Split(' ');
        int? mes = null;
        int? anio = null;

        foreach (var parte in partes)
        {
            if (MesesPorNombre.TryGetValue(parte, out var m))
            {
                mes = m;
            }
            else if (int.TryParse(parte, out var a) && a >= 2020 && a <= 2035)
            {
                anio = a;
            }
        }

        return (anio, mes);
    }

    private static DateTime CorregirFechaSegunHoja(
        DateTime fechaLeida,
        int? anioEsperado,
        int? mesEsperado,
        int ordenDiaEnBloque)
    {
        var anio = anioEsperado ?? fechaLeida.Year;
        var mes = fechaLeida.Month;
        var dia = fechaLeida.Day;

        if (mesEsperado.HasValue && mes != mesEsperado.Value)
        {
            mes = mesEsperado.Value;
            if (mes == 11 && (dia == 15 || dia == 16) && ordenDiaEnBloque >= 10)
            {
                dia = dia == 15 ? 20 : 21;
            }
        }

        var diasEnMes = DateTime.DaysInMonth(anio, mes);
        return new DateTime(anio, mes, Math.Min(dia, diasEnMes));
    }

    private static string InferirFabricante(string modelo)
    {
        var norm = Normalizar(modelo);
        if (norm.Contains("ricoh")) return "Ricoh";
        if (norm.Contains("hp")) return "HP";
        if (norm.Contains("epson")) return "Epson";
        return "Canon";
    }

    private static string InferirDepartamento(string modelo)
    {
        var norm = Normalizar(modelo);
        if (norm.Contains("facturacion")) return "Facturación";
        if (norm.Contains("rrhh")) return "RRHH";
        if (norm.Contains("pcp")) return "PCP";
        if (norm.Contains("comercial")) return "Comercial";
        if (norm.Contains("traco")) return "Operaciones";
        if (norm.Contains("overland")) return "Operaciones";
        if (norm.Contains("principal")) return "Administración";
        return Impresora.DepartamentoPorDefecto;
    }

    private static string Normalizar(string texto)
    {
        var plano = texto
            .Trim()
            .Normalize(NormalizationForm.FormD);

        var sinAcentos = string.Concat(plano.Where(c =>
            CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));

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
            ["dd/MM/yyyy", "yyyy-MM-dd", "dd-MM-yyyy"],
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out fecha);
    }

    private static int? IntentaLeerEntero(IXLCell celda)
    {
        if (celda.TryGetValue<int>(out var entero))
        {
            return entero;
        }

        if (celda.TryGetValue<double>(out var decimalVal))
        {
            return (int)Math.Round(decimalVal);
        }

        if (long.TryParse(celda.GetString().Trim(), out var largo))
        {
            return (int)largo;
        }

        return null;
    }

    private static async ValueTask<(int Id, bool Creada)> ObtenerOCrearImpresoraStaticAsync(
        SgctiDbContext db,
        string nombre,
        Dictionary<string, int> cache,
        CancellationToken ct)
    {
        if (cache.TryGetValue(nombre, out var idExistente))
        {
            return (idExistente, false);
        }

        var impresora = await db.Impresoras
            .FirstOrDefaultAsync(i => i.Modelo.ToLower() == nombre.ToLower(), ct);

        if (impresora is null)
        {
            impresora = new Impresora
            {
                Modelo = nombre,
                Ip = $"importado-{Guid.NewGuid():N}",
                Fabricante = InferirFabricante(nombre),
                Departamento = InferirDepartamento(nombre),
                FechaInstalacion = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc)
            };

            db.Impresoras.Add(impresora);
            await db.SaveChangesAsync(ct);

            cache[nombre] = impresora.Id;

            return (impresora.Id, true);
        }

        cache[nombre] = impresora.Id;

        return (impresora.Id, false);
    }

    private sealed class BloqueImpresoras
    {
        public int FilaInicioDatos { get; set; }

        public int FilaFinDatos { get; set; }

        public List<GrupoImpresora> Grupos { get; set; } = [];
    }

    private sealed class GrupoImpresora
    {
        public string? Nombre { get; set; }

        public int? ColumnaFecha { get; set; }

        public int? ColumnaContador { get; set; }

        public int? ColumnaConsumo { get; set; }

        public int? ColumnaObservacion { get; set; }
    }
}