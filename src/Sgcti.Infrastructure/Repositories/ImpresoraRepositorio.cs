using Microsoft.EntityFrameworkCore;
using Sgcti.Core.Abstractions;
using Sgcti.Core.Entities;
using Sgcti.Infrastructure.Persistence;

namespace Sgcti.Infrastructure.Repositories;

public class ImpresoraRepositorio : IImpresoraRepositorio
{
    private readonly SgctiDbContext _db;

    public ImpresoraRepositorio(SgctiDbContext db)
    {
        _db = db;
    }

    public Task<Impresora?> ObtenerPorIdAsync(int impresoraId)
        => _db.Impresoras
            .Include(i => i.ConsumibleToner)
            .FirstOrDefaultAsync(i => i.Id == impresoraId);

    public async Task<IReadOnlyList<HistorialConsumo>> ObtenerHistorialRecienteAsync(int impresoraId, DateTime desde)
        => await _db.HistorialConsumo
            .Where(h => h.ImpresoraId == impresoraId && h.FechaCambio >= desde)
            .OrderBy(h => h.FechaCambio)
            .ToListAsync();

    public Task GuardarCambiosAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}