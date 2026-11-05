using System.Globalization;
using System.Text;
using Sgcti.Core.Entities;

namespace Sgcti.Core.Services;

public static class GeneradorNomenclaturaTicket
{
    private const string SiglaRespaldo = "TCK";

    public static string Generar(
        int consecutivoDepartamento,
        string? departamento,
        IReadOnlyCollection<Departamento> departamentosConfigurados)
    {
        var sigla = ObtenerSigla(departamento, departamentosConfigurados);
        var numero = Math.Max(1, consecutivoDepartamento);
        return $"{sigla}-{numero:D3}";
    }

    public static bool RecalcularConsecutivosPorDepartamento(
        IEnumerable<Ticket> tickets,
        IReadOnlyCollection<Departamento> departamentosConfigurados)
    {
        var contadoresPorSigla = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var huboCambios = false;

        foreach (var ticket in tickets.OrderBy(t => t.Id))
        {
            var sigla = ObtenerSigla(ticket.Departamento, departamentosConfigurados);
            var siguiente = contadoresPorSigla.GetValueOrDefault(sigla, 0) + 1;
            contadoresPorSigla[sigla] = siguiente;

            var codigoEsperado = $"{sigla}-{siguiente:D3}";
            if (!string.Equals(ticket.CodigoNomenclatura, codigoEsperado, StringComparison.Ordinal))
            {
                ticket.CodigoNomenclatura = codigoEsperado;
                huboCambios = true;
            }
        }

        return huboCambios;
    }

    public static string ObtenerSigla(
        string? departamento,
        IReadOnlyCollection<Departamento> departamentosConfigurados)
    {
        var normalizado = Normalizar(departamento);

        if (normalizado.Length == 0)
        {
            return SiglaRespaldo;
        }

        var configurado = departamentosConfigurados.FirstOrDefault(d =>
            Normalizar(d.Nombre) == normalizado || Normalizar(d.Sigla) == normalizado);

        if (!string.IsNullOrWhiteSpace(configurado?.Sigla))
        {
            return configurado.Sigla.Trim().ToUpperInvariant();
        }

        var caracteres = new string(normalizado
            .Where(char.IsLetterOrDigit)
            .Take(3)
            .ToArray())
            .ToUpperInvariant();

        return caracteres.Length > 0 ? caracteres : SiglaRespaldo;
    }

    private static string Normalizar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return string.Empty;
        }

        var descompuesto = valor.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var resultado = new StringBuilder(descompuesto.Length);

        foreach (var caracter in descompuesto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caracter) != UnicodeCategory.NonSpacingMark)
            {
                resultado.Append(caracter);
            }
        }

        return resultado.ToString();
    }
}