using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sgcti.Infrastructure.Persistence;

namespace Sgcti.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class SgctiControllerBase<TEntity> : ControllerBase where TEntity : class
{
    protected readonly SgctiDbContext Db;

    protected SgctiControllerBase(SgctiDbContext db)
    {
        Db = db;
    }

    [HttpGet]
    public virtual async Task<ActionResult<IEnumerable<TEntity>>> GetAll(CancellationToken ct)
        => await Db.Set<TEntity>().AsNoTracking().ToListAsync(ct);

    [HttpGet("{id:int}")]
    public virtual async Task<ActionResult<TEntity>> GetById(int id, CancellationToken ct)
    {
        var entity = await Db.Set<TEntity>().FindAsync(new object[] { id }, ct);
        if (entity is null)
        {
            return NotFound();
        }

        return entity;
    }

    [HttpPost]
    public virtual async Task<ActionResult<TEntity>> Create(TEntity entity, CancellationToken ct)
    {
        Db.Set<TEntity>().Add(entity);
        await Db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = GetId(entity) }, entity);
    }

    [HttpPut("{id:int}")]
    public virtual async Task<IActionResult> Update(int id, TEntity entity, CancellationToken ct)
    {
        if (GetId(entity) != id)
        {
            return BadRequest("El identificador del cuerpo no coincide con el de la ruta.");
        }

        var existing = await Db.Set<TEntity>().FindAsync(new object[] { id }, ct);
        if (existing is null)
        {
            return NotFound();
        }

        Db.Entry(existing).CurrentValues.SetValues(entity);
        await Db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public virtual async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entity = await Db.Set<TEntity>().FindAsync(new object[] { id }, ct);
        if (entity is null)
        {
            return NotFound();
        }

        Db.Set<TEntity>().Remove(entity);
        await Db.SaveChangesAsync(ct);

        return NoContent();
    }

    private static int GetId(TEntity entity)
        => (int)(entity.GetType().GetProperty("Id")?.GetValue(entity) ?? 0);
}