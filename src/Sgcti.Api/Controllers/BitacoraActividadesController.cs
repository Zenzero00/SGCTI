using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sgcti.Core.Dtos;
using Sgcti.Core.Entities;
using Sgcti.Infrastructure.Persistence;

namespace Sgcti.Api.Controllers;

[ApiController]
[Route("api/bitacora")]
public class BitacoraActividadesController : ControllerBase
{
    private readonly SgctiDbContext _db;

    public BitacoraActividadesController(SgctiDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BitacoraDto>>> GetAll(CancellationToken ct)
    {
        var actividades = await _db.BitacoraActividades
            .AsNoTracking()
            .OrderByDescending(b => b.HoraInicio)
            .Select(b => new BitacoraDto
            {
                Id = b.Id,
                AnalistaId = b.AnalistaId,
                DescripcionActividad = b.DescripcionActividad,
                EquipoIntervenido = b.EquipoIntervenido,
                HoraInicio = b.HoraInicio,
                HoraFin = b.HoraFin,
                Etiqueta = b.Etiqueta,
                Estado = b.Estado
            })
            .ToListAsync(ct);

        return Ok(actividades);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BitacoraDto>> GetById(int id, CancellationToken ct)
    {
        var actividad = await _db.BitacoraActividades
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (actividad is null)
        {
            return NotFound();
        }

        return Ok(ADto(actividad));
    }

    [HttpPost]
    public async Task<ActionResult<BitacoraDto>> Create(CrearBitacoraDto dto, CancellationToken ct)
    {
        var entidad = new BitacoraActividad
        {
            AnalistaId = dto.AnalistaId.Trim(),
            DescripcionActividad = dto.DescripcionActividad.Trim(),
            EquipoIntervenido = string.IsNullOrWhiteSpace(dto.EquipoIntervenido) ? null : dto.EquipoIntervenido.Trim(),
            HoraInicio = DateTime.UtcNow,
            Etiqueta = dto.Etiqueta.Trim(),
            Estado = BitacoraActividad.EstadoPendiente
        };

        _db.BitacoraActividades.Add(entidad);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = entidad.Id }, ADto(entidad));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ActualizarBitacoraDto dto, CancellationToken ct)
    {
        var entidad = await _db.BitacoraActividades.FindAsync(new object[] { id }, ct);

        if (entidad is null)
        {
            return NotFound();
        }

        entidad.Estado = dto.Estado.Trim();

        if (!string.IsNullOrWhiteSpace(dto.DescripcionActividad))
        {
            entidad.DescripcionActividad = dto.DescripcionActividad.Trim();
        }

        if (dto.HoraFin.HasValue)
        {
            entidad.HoraFin = DateTime.SpecifyKind(dto.HoraFin.Value, DateTimeKind.Utc);
        }

        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entidad = await _db.BitacoraActividades.FindAsync(new object[] { id }, ct);

        if (entidad is null)
        {
            return NotFound();
        }

        _db.BitacoraActividades.Remove(entidad);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    private static BitacoraDto ADto(BitacoraActividad b) => new()
    {
        Id = b.Id,
        AnalistaId = b.AnalistaId,
        DescripcionActividad = b.DescripcionActividad,
        EquipoIntervenido = b.EquipoIntervenido,
        HoraInicio = b.HoraInicio,
        HoraFin = b.HoraFin,
        Etiqueta = b.Etiqueta,
        Estado = b.Estado
    };
}