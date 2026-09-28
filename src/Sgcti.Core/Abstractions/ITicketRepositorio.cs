using Sgcti.Core.Entities;

namespace Sgcti.Core.Abstractions;

public interface ITicketRepositorio
{
    Task<IReadOnlyList<Ticket>> ObtenerTodosAsync(CancellationToken ct = default);

    Task<Ticket?> ObtenerPorIdAsync(int ticketId, CancellationToken ct = default);

    Task<Ticket> AgregarAsync(Ticket ticket, CancellationToken ct = default);

    Task ActualizarAsync(Ticket ticket, CancellationToken ct = default);

    Task EliminarAsync(int ticketId, CancellationToken ct = default);

    Task GuardarCambiosAsync(CancellationToken ct = default);
}