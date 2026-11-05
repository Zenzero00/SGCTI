using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sgcti.Core.Entities;
using Sgcti.Infrastructure.Persistence;

namespace Sgcti.Api.Controllers;

public class HistorialConsumoController : SgctiControllerBase<HistorialConsumo>
{
    public HistorialConsumoController(SgctiDbContext db) : base(db)
    {
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<HistorialConsumo>>> GetAllFiltrado([FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin, CancellationToken ct)
    {
        IQueryable<HistorialConsumo> query = Db.Set<HistorialConsumo>().AsNoTracking();

        if (fechaInicio.HasValue)
        {
            query = query.Where(c => c.FechaCambio >= fechaInicio.Value.Date);
        }

        if (fechaFin.HasValue)
        {
            var fechaFinFin = fechaFin.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(c => c.FechaCambio <= fechaFinFin);
        }

        var result = await query.ToListAsync(ct);
        return Ok(result);
    }
}