using Microsoft.AspNetCore.Mvc;
using Sgcti.Core.Entities;
using Sgcti.Infrastructure.Persistence;

namespace Sgcti.Api.Controllers;

public class MetricasRedController : SgctiControllerBase<MetricaRed>
{
    public MetricasRedController(SgctiDbContext db) : base(db)
    {
    }
}