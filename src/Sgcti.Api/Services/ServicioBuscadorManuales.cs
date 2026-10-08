using System.Globalization;

namespace Sgcti.Api.Services;

public class ServicioBuscadorManuales : IServicioBuscadorManuales
{
    private const string PlantillaUrlBusqueda =
        "https://www.google.com/search?q=Manual+{0}+filetype:pdf";

    public string ConstruirUrlBusqueda(string modelo)
    {
        if (string.IsNullOrWhiteSpace(modelo))
        {
            return string.Empty;
        }

        var modeloLimpio = LimpiarModelo(modelo);

        if (string.IsNullOrWhiteSpace(modeloLimpio))
        {
            return string.Empty;
        }

        return string.Format(
            CultureInfo.InvariantCulture, PlantillaUrlBusqueda, Uri.EscapeDataString(modeloLimpio));
    }

    private static string LimpiarModelo(string modelo)
    {
        var indiceComa = modelo.IndexOf(',');

        if (indiceComa >= 0)
        {
            modelo = modelo[..indiceComa];
        }

        return modelo.Trim();
    }
}
