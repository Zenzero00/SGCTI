using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sgcti.Core.Dtos;
using Sgcti.Core.Entities;
using Sgcti.Infrastructure.Persistence;

namespace Sgcti.Api.Controllers;

[ApiController]
[Route("api/suministros")]
public class SuministrosController : ControllerBase
{
    private readonly SgctiDbContext _db;

    public SuministrosController(SgctiDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SuministroDto>>> GetAll(CancellationToken ct)
    {
        var suministros = await _db.Suministros
            .AsNoTracking()
            .Select(s => new SuministroDto
            {
                Id = s.Id,
                Nombre = s.Nombre,
                Tipo = s.Tipo,
                CantidadActual = s.CantidadActual,
                StockMinimo = s.StockMinimo,
                Observacion = s.Observacion
            })
            .ToListAsync(ct);

        return Ok(suministros);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SuministroDto>> GetById(int id, CancellationToken ct)
    {
        var suministro = await _db.Suministros
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (suministro is null)
        {
            return NotFound();
        }

        return Ok(ADto(suministro));
    }

    [HttpPost]
    public async Task<ActionResult<SuministroDto>> Create(CrearSuministroDto dto, CancellationToken ct)
    {
        var entidad = new Suministro
        {
            Nombre = dto.Nombre.Trim(),
            Tipo = dto.Tipo.Trim(),
            CantidadActual = dto.CantidadActual,
            StockMinimo = dto.StockMinimo,
            Observacion = dto.Observacion?.Trim()
        };

        _db.Suministros.Add(entidad);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = entidad.Id }, ADto(entidad));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ActualizarSuministroDto dto, CancellationToken ct)
    {
        var entidad = await _db.Suministros.FindAsync(new object[] { id }, ct);

        if (entidad is null)
        {
            return NotFound();
        }

        entidad.Nombre = dto.Nombre.Trim();
        entidad.Tipo = dto.Tipo.Trim();
        entidad.CantidadActual = dto.CantidadActual;
        entidad.StockMinimo = dto.StockMinimo;
        entidad.Observacion = dto.Observacion?.Trim();

        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entidad = await _db.Suministros.FindAsync(new object[] { id }, ct);

        if (entidad is null)
        {
            return NotFound();
        }

        _db.Suministros.Remove(entidad);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    private static SuministroDto ADto(Suministro s) => new()
    {
        Id = s.Id,
        Nombre = s.Nombre,
        Tipo = s.Tipo,
        CantidadActual = s.CantidadActual,
        StockMinimo = s.StockMinimo,
        Observacion = s.Observacion
    };
}
