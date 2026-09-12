using Microsoft.AspNetCore.Mvc;
using Sgcti.Core.Entities;
using Sgcti.Core.Services;
using Sgcti.Infrastructure.Persistence;

namespace Sgcti.Api.Controllers;

public class ImpresorasController : SgctiControllerBase<Impresora>
{
    private readonly ServicioAnalisisPredictivo _servicioAnalisis;

    public ImpresorasController(SgctiDbContext db, ServicioAnalisisPredictivo servicioAnalisis) : base(db)
    {
        _servicioAnalisis = servicioAnalisis;
    }

    public override async Task<IActionResult> Update(int id, Impresora entity, CancellationToken ct)
    {
        var resultado = await base.Update(id, entity, ct);

        if (resultado is NoContentResult)
        {
            await _servicioAnalisis.CalcularPredicciones(id);
        }

        return resultado;
    }
}