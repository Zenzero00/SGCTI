using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sgcti.Core.Dtos;
using Sgcti.Core.Entities;
using Sgcti.Core.Services;
using Sgcti.Infrastructure.Persistence;

namespace Sgcti.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/departamentos")]
public class DepartamentosController : ControllerBase
{
    private readonly SgctiDbContext _db;

    public DepartamentosController(SgctiDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DepartamentoDto>>> GetAll(CancellationToken ct)
    {
        var departamentos = await _db.Departamentos
            .AsNoTracking()
            .OrderBy(d => d.Nombre)
            .ToListAsync(ct);

        return Ok(departamentos.Select(ADto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DepartamentoDto>> GetById(int id, CancellationToken ct)
    {
        var departamento = await _db.Departamentos.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct);

        if (departamento is null)
        {
            return NotFound();
        }

        return Ok(ADto(departamento));
    }

    [HttpPost]
    public async Task<ActionResult<DepartamentoDto>> Create(CrearDepartamentoDto dto, CancellationToken ct)
    {
        var nombre = dto.Nombre.Trim();
        var sigla = dto.Sigla.Trim().ToUpperInvariant();

        if (await ExisteNombreAsync(nombre, null, ct))
        {
            return BadRequest("Ya existe un departamento con ese nombre.");
        }

        if (await ExisteSiglaAsync(sigla, null, ct))
        {
            return BadRequest("Ya existe un departamento con esa sigla.");
        }

        if (ClasificadorPrioridadTicket.PrioridadDesdeTexto(dto.PrioridadPorDefecto) is not { } prioridad)
        {
            return BadRequest("La prioridad por defecto debe ser Baja, Media o Alta.");
        }

        var departamento = new Departamento
        {
            Nombre = nombre,
            Sigla = sigla,
            PrioridadPorDefecto = prioridad
        };

        _db.Departamentos.Add(departamento);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = departamento.Id }, ADto(departamento));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ActualizarDepartamentoDto dto, CancellationToken ct)
    {
        var departamento = await _db.Departamentos.FirstOrDefaultAsync(d => d.Id == id, ct);

        if (departamento is null)
        {
            return NotFound();
        }

        var nombre = dto.Nombre.Trim();
        var sigla = dto.Sigla.Trim().ToUpperInvariant();

        if (await ExisteNombreAsync(nombre, id, ct))
        {
            return BadRequest("Ya existe un departamento con ese nombre.");
        }

        if (await ExisteSiglaAsync(sigla, id, ct))
        {
            return BadRequest("Ya existe un departamento con esa sigla.");
        }

        if (ClasificadorPrioridadTicket.PrioridadDesdeTexto(dto.PrioridadPorDefecto) is not { } prioridad)
        {
            return BadRequest("La prioridad por defecto debe ser Baja, Media o Alta.");
        }

        departamento.Nombre = nombre;
        departamento.Sigla = sigla;
        departamento.PrioridadPorDefecto = prioridad;

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var departamento = await _db.Departamentos.FirstOrDefaultAsync(d => d.Id == id, ct);

        if (departamento is null)
        {
            return NotFound();
        }

        _db.Departamentos.Remove(departamento);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    private Task<bool> ExisteNombreAsync(string nombre, int? excluirId, CancellationToken ct)
        => _db.Departamentos.AnyAsync(
            d => d.Nombre.ToLower() == nombre.ToLower() && (!excluirId.HasValue || d.Id != excluirId.Value),
            ct);

    private Task<bool> ExisteSiglaAsync(string sigla, int? excluirId, CancellationToken ct)
        => _db.Departamentos.AnyAsync(
            d => d.Sigla.ToLower() == sigla.ToLower() && (!excluirId.HasValue || d.Id != excluirId.Value),
            ct);

    private static DepartamentoDto ADto(Departamento departamento) => new()
    {
        Id = departamento.Id,
        Nombre = departamento.Nombre,
        Sigla = departamento.Sigla,
        PrioridadPorDefecto = departamento.PrioridadPorDefecto.ToString()
    };
}
