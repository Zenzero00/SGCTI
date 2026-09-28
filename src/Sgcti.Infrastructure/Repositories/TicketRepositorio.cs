using Microsoft.EntityFrameworkCore;
using Sgcti.Core.Abstractions;
using Sgcti.Core.Entities;
using Sgcti.Infrastructure.Persistence;

namespace Sgcti.Infrastructure.Repositories;

public class TicketRepositorio : ITicketRepositorio
{
    private readonly SgctiDbContext _db;

    public TicketRepositorio(SgctiDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Ticket>> ObtenerTodosAsync(CancellationToken ct = default)
        => await _db.Tickets
            .AsNoTracking()
            .OrderByDescending(t => t.FechaCreacion)
            .ToListAsync(ct);

    public Task<Ticket?> ObtenerPorIdAsync(int ticketId, CancellationToken ct = default)
        => _db.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId, ct);

    public async Task<Ticket> AgregarAsync(Ticket ticket, CancellationToken ct = default)
    {
        await _db.Tickets.AddAsync(ticket, ct);
        await _db.SaveChangesAsync(ct);
        return ticket;
    }

    public async Task ActualizarAsync(Ticket ticket, CancellationToken ct = default)
    {
        _db.Tickets.Update(ticket);
        await _db.SaveChangesAsync(ct);
    }

    public async Task EliminarAsync(int ticketId, CancellationToken ct = default)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId, ct);
        if (ticket is null)
        {
            return;
        }

        _db.Tickets.Remove(ticket);
        await _db.SaveChangesAsync(ct);
    }

    public Task GuardarCambiosAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}