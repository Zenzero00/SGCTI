using Microsoft.AspNetCore.Mvc;
using Sgcti.Core.Entities;
using Sgcti.Infrastructure.Persistence;

namespace Sgcti.Api.Controllers;

public class ConsumiblesController : SgctiControllerBase<Consumible>
{
    public ConsumiblesController(SgctiDbContext db) : base(db)
    {
    }
}