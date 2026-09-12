using Microsoft.AspNetCore.Mvc;
using Sgcti.Core.Entities;
using Sgcti.Infrastructure.Persistence;

namespace Sgcti.Api.Controllers;

public class TicketsController : SgctiControllerBase<Ticket>
{
    public TicketsController(SgctiDbContext db) : base(db)
    {
    }
}