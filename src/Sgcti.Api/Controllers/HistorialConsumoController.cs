using Microsoft.AspNetCore.Mvc;
using Sgcti.Core.Entities;
using Sgcti.Infrastructure.Persistence;

namespace Sgcti.Api.Controllers;

public class HistorialConsumoController : SgctiControllerBase<HistorialConsumo>
{
    public HistorialConsumoController(SgctiDbContext db) : base(db)
    {
    }
}