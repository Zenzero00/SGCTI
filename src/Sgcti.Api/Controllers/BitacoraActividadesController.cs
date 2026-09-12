using Microsoft.AspNetCore.Mvc;
using Sgcti.Core.Entities;
using Sgcti.Infrastructure.Persistence;

namespace Sgcti.Api.Controllers;

public class BitacoraActividadesController : SgctiControllerBase<BitacoraActividad>
{
    public BitacoraActividadesController(SgctiDbContext db) : base(db)
    {
    }
}