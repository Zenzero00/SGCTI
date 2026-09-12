using Sgcti.Core.Abstractions;
using Sgcti.Core.Entities;

namespace Sgcti.Core.Services;

public class ServicioAnalisisPredictivo
{
    private const int RendimientoTonnerPaginas = 3000;

    private const int IntervaloMantenimientoPaginas = 50000;

    private const int VentanaPromedioDias = 30;

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

        var consumoReciente = await _repositorio.ObtenerHistorialRecienteAsync(
            impresoraId, DateTime.UtcNow.AddDays(-VentanaPromedioDias));

        impresora.PromedioPaginasDiario = CalcularPromedioPaginasDiario(impresora, consumoReciente);

        var promedioDiario = Math.Max(impresora.PromedioPaginasDiario, 1);
        var paginasUsadasTonerActual = consumoReciente.Count > 0
            ? consumoReciente[^1].PaginasImpresas
            : impresora.ContadorTotalPaginas % RendimientoTonnerPaginas;

        impresora.DiasEstimadosAgotamientoTonner = Math.Max(0,
            (int)((RendimientoTonnerPaginas - paginasUsadasTonerActual) / promedioDiario));

        impresora.FechaEstimadaAgotamientoTonner = DateTime.UtcNow.AddDays(impresora.DiasEstimadosAgotamientoTonner);

        impresora.AlertaStockCritico = CalcularAlertaStockCritico(impresora);

        var paginasRestantesMantenimiento = Math.Max(
            IntervaloMantenimientoPaginas - impresora.PaginasDesdeUltimoMantenimiento, 0);

        impresora.DiasEstimadosMantenimiento = (int)(paginasRestantesMantenimiento / promedioDiario);

        impresora.FechaEstimadaMantenimiento = DateTime.UtcNow.AddDays(impresora.DiasEstimadosMantenimiento);

        await _repositorio.GuardarCambiosAsync();
    }

    private static bool CalcularAlertaStockCritico(Impresora impresora)
    {
        if (impresora.ConsumibleToner is null)
        {
            return false;
        }

        return impresora.DiasEstimadosAgotamientoTonner <= impresora.ConsumibleToner.DiasAntelacionPedido
               && impresora.ConsumibleToner.CantidadActual <= impresora.ConsumibleToner.StockMinimo;
    }

    private static double CalcularPromedioPaginasDiario(
        Impresora impresora, IReadOnlyList<HistorialConsumo> consumoReciente)
    {
        if (consumoReciente.Count > 0)
        {
            var diasTranscurridos = Math.Max(
                (int)(consumoReciente[^1].FechaCambio - consumoReciente[0].FechaCambio).TotalDays, 1);

            return Math.Round(consumoReciente.Sum(h => h.PaginasImpresas) / (double)diasTranscurridos, 2);
        }

        var diasDesdeInstalacion = Math.Max(
            (int)(DateTime.UtcNow - impresora.FechaInstalacion).TotalDays, 1);

        return Math.Round(impresora.ContadorTotalPaginas / (double)diasDesdeInstalacion, 2);
    }
}