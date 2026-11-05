using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sgcti.Core.Dtos;
using Sgcti.Core.Entities;
using Sgcti.Infrastructure.Persistence;

namespace Sgcti.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/categorias")]
public class CategoriasController : ControllerBase
{
    private readonly SgctiDbContext _db;

    public CategoriasController(SgctiDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoriaTicketDto>>> GetAll(CancellationToken ct)
    {
        var categorias = await _db.CategoriasTicket
            .AsNoTracking()
            .OrderBy(c => c.Nombre)
            .ToListAsync(ct);

        return Ok(categorias.Select(ADto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoriaTicketDto>> GetById(int id, CancellationToken ct)
    {
        var categoria = await _db.CategoriasTicket.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

        if (categoria is null)
        {
            return NotFound();
        }

        return Ok(ADto(categoria));
    }

    [HttpPost]
    public async Task<ActionResult<CategoriaTicketDto>> Create(CrearCategoriaTicketDto dto, CancellationToken ct)
    {
        var nombre = dto.Nombre.Trim();

        if (await ExisteNombreAsync(nombre, null, ct))
        {
            return BadRequest("Ya existe una categoría con ese nombre.");
        }

        var categoria = new CategoriaTicket { Nombre = nombre };

        _db.CategoriasTicket.Add(categoria);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, ADto(categoria));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ActualizarCategoriaTicketDto dto, CancellationToken ct)
    {
        var categoria = await _db.CategoriasTicket.FirstOrDefaultAsync(c => c.Id == id, ct);

        if (categoria is null)
        {
            return NotFound();
        }

        var nombre = dto.Nombre.Trim();

        if (await ExisteNombreAsync(nombre, id, ct))
        {
            return BadRequest("Ya existe una categoría con ese nombre.");
        }

        categoria.Nombre = nombre;

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var categoria = await _db.CategoriasTicket.FirstOrDefaultAsync(c => c.Id == id, ct);

        if (categoria is null)
        {
            return NotFound();
        }

        _db.CategoriasTicket.Remove(categoria);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    private Task<bool> ExisteNombreAsync(string nombre, int? excluirId, CancellationToken ct)
        => _db.CategoriasTicket.AnyAsync(
            c => c.Nombre.ToLower() == nombre.ToLower() && (!excluirId.HasValue || c.Id != excluirId.Value),
            ct);

    private static CategoriaTicketDto ADto(CategoriaTicket categoria) => new()
    {
        Id = categoria.Id,
        Nombre = categoria.Nombre
    };
}
