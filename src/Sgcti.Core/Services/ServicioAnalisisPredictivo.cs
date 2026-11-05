using Sgcti.Core.Abstractions;
using Sgcti.Core.Entities;

namespace Sgcti.Core.Services;

public class ServicioAnalisisPredictivo
{
    private const int RendimientoTonnerPaginas = 3500;
    private const int IntervaloMantenimientoPaginas = 15000;

    private readonly IImpresoraRepositorio _repositorio;

    public ServicioAnalisisPredictivo(IImpresoraRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task CalcularPredicciones(int impresoraId)
    {
        var impresora = await _repositorio.ObtenerPorIdAsync(impresoraId);
        if (impresora is null)
        {
            return;
        }

        // Si la impresora está desconectada, no debe mostrar días de predicción ni lanzar alertas
        if (impresora.EstadoPing != EstadoPing.EnLinea)
        {
            impresora.NivelTonnerNegro = 0;
            impresora.DiasEstimadosAgotamientoTonner = 0;
            impresora.DiasEstimadosMantenimiento = 0;
            impresora.AlertaStockCritico = false;
            await _repositorio.GuardarCambiosAsync();
            return;
        }

        var promedioDiario = Math.Clamp(impresora.PromedioPaginasDiario > 0 ? impresora.PromedioPaginasDiario : 120.0, 60.0, 420.0);
        impresora.PromedioPaginasDiario = Math.Round(promedioDiario, 1);

        var paginasRestantesToner = (impresora.NivelTonnerNegro / 100.0) * RendimientoTonnerPaginas;
        impresora.DiasEstimadosAgotamientoTonner = (int)Math.Max(1, Math.Round(paginasRestantesToner / promedioDiario));
        impresora.FechaEstimadaAgotamientoTonner = DateTime.UtcNow.AddDays(impresora.DiasEstimadosAgotamientoTonner);

        var paginasRestantesMantenimiento = Math.Max(0, IntervaloMantenimientoPaginas - impresora.PaginasDesdeUltimoMantenimiento);
        impresora.DiasEstimadosMantenimiento = paginasRestantesMantenimiento == 0
            ? 1
            : (int)Math.Max(1, Math.Round(paginasRestantesMantenimiento / promedioDiario));
        impresora.FechaEstimadaMantenimiento = DateTime.UtcNow.AddDays(impresora.DiasEstimadosMantenimiento);

        impresora.AlertaStockCritico = impresora.NivelTonnerNegro <= 15 || impresora.DiasEstimadosAgotamientoTonner <= 7;

        await _repositorio.GuardarCambiosAsync();
    }
}