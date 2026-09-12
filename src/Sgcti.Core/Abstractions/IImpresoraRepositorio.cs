using Sgcti.Core.Entities;

namespace Sgcti.Core.Abstractions;

public interface IImpresoraRepositorio
{
    Task<Impresora?> ObtenerPorIdAsync(int impresoraId);

    Task<IReadOnlyList<HistorialConsumo>> ObtenerHistorialRecienteAsync(int impresoraId, DateTime desde);

    Task GuardarCambiosAsync(CancellationToken ct = default);
}