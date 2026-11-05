using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Sgcti.Core.Entities;

namespace Sgcti.Core.Services;

public sealed record ClasificacionPrioridad(PrioridadTicket Prioridad, string? Departamento);

public static class FormatoTelegramEmpleado
{
    public static (string? Telefono, string? ChatId) Desglosar(string? valorAlmacenado)
    {
        if (string.IsNullOrWhiteSpace(valorAlmacenado))
        {
            return (null, null);
        }

        var limpio = valorAlmacenado.Trim();

        if (limpio.Contains('|'))
        {
            var partes = limpio.Split('|', 2, StringSplitOptions.TrimEntries);
            var tel = string.IsNullOrWhiteSpace(partes[0]) ? null : FormatearTelefono(partes[0]);
            var chat = partes.Length > 1 && !string.IsNullOrWhiteSpace(partes[1]) ? partes[1] : null;
            return (tel, chat);
        }

        var soloDigitos = new string(limpio.Where(char.IsDigit).ToArray());

        if (EsFormatoTelefono(limpio, soloDigitos))
        {
            return (FormatearTelefono(limpio), null);
        }

        if (soloDigitos.Length >= 6 && soloDigitos.Length == limpio.TrimStart('-').Length)
        {
            return (null, limpio);
        }

        return (null, null);
    }

    public static string? Combinar(string? telefono, string? chatId)
    {
        var telFormateado = FormatearTelefono(telefono);
        var chatLimpio = string.IsNullOrWhiteSpace(chatId) ? null : chatId.Trim();

        if (telFormateado is null && chatLimpio is null)
        {
            return null;
        }

        if (chatLimpio is null)
        {
            return telFormateado;
        }

        return $"{telFormateado ?? string.Empty}|{chatLimpio}";
    }

    public static string? NormalizarDigitosTelefono(string? telefono)
    {
        if (string.IsNullOrWhiteSpace(telefono))
        {
            return null;
        }

        var digitos = new string(telefono.Where(char.IsDigit).ToArray());

        if (digitos.Length < 7)
        {
            return null;
        }

        return digitos.Length > 10 ? digitos[^10..] : digitos;
    }

    public static string? FormatearTelefono(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        var digitos = new string(valor.Where(char.IsDigit).ToArray());

        if (digitos.Length == 0)
        {
            return null;
        }

        if (digitos.StartsWith("58") && digitos.Length == 12)
        {
            digitos = "0" + digitos[2..];
        }
        else if (digitos.Length == 10 && (digitos.StartsWith("4") || digitos.StartsWith("2")))
        {
            digitos = "0" + digitos;
        }

        if (digitos.Length == 11 && digitos.StartsWith("0"))
        {
            return $"{digitos[..4]}-{digitos[4..]}";
        }

        return valor.Trim();
    }

    private static bool EsFormatoTelefono(string textoOriginal, string soloDigitos)
    {
        if (textoOriginal.StartsWith('+'))
        {
            return soloDigitos.Length >= 10;
        }

        if (soloDigitos.Length == 11 && (soloDigitos.StartsWith("04") || soloDigitos.StartsWith("02")))
        {
            return true;
        }

        if (soloDigitos.Length == 12 && soloDigitos.StartsWith("58"))
        {
            return true;
        }

        if (soloDigitos.Length == 10 && (soloDigitos.StartsWith("412") || soloDigitos.StartsWith("414") || soloDigitos.StartsWith("424") || soloDigitos.StartsWith("416") || soloDigitos.StartsWith("426")))
        {
            return true;
        }

        return textoOriginal.Contains('-') && soloDigitos.Length >= 7;
    }
}

public static class ClasificadorPrioridadTicket
{
    private static readonly string[] PalabrasClavePrioritarias = ["gerencia", "[ger]", "gerente", "directiva"];

    private static readonly (string Categoria, string[] Palabras)[] ReglasCategoria =
    [
        ("Impresoras", ["impresora", "imprimir", "impresion", "toner", "atasco", "papel", "escaner", "copiadora", "tinta", "cartucho", "cabezal", "rodillo", "bandeja"]),
        ("Periféricos", ["teclado", "mouse", "raton", "cable", "usb", "audifono", "corneta", "webcam", "camara", "adaptador", "ups", "regleta", "lector", "periferico"]),
        ("Red", ["internet", "red", "wifi", "wi-fi", "conexion", "ip", "router", "switch", "servidor", "cableado", "ping", "lan", "ethernet", "dns", "vpn", "mikrotik"]),
        ("Software", ["windows", "office", "excel", "word", "sistema", "programa", "licencia", "antivirus", "correo", "clave", "contrasena", "software", "virus", "formatear", "actualizar", "instalacion", "instalar", "erp", "driver", "navegador"]),
        ("Hardware", ["pantalla", "monitor", "computadora", "pc", "cpu", "enciende", "prende", "disco", "memoria", "ram", "fuente", "azul", "reinicia", "apaga", "placa", "tarjeta", "pitido", "ventilador", "recalienta", "laptop", "equipo"])
    ];

    public static PrioridadTicket? PrioridadDesdeTexto(string? valor)
    {
        return Normalizar(valor) switch
        {
            "baja" => PrioridadTicket.Baja,
            "media" => PrioridadTicket.Media,
            "alta" => PrioridadTicket.Alta,
            "critica" => PrioridadTicket.Critica,
            _ => null
        };
    }

    public static ClasificacionPrioridad? Clasificar(
        string? departamento,
        IReadOnlyCollection<Departamento> departamentosConfigurados,
        params string?[] textos)
    {
        var departamentoNormalizado = Normalizar(departamento);

        if (departamentoNormalizado.Length > 0)
        {
            var coincidenciaDirecta = departamentosConfigurados.FirstOrDefault(d =>
                Normalizar(d.Nombre) == departamentoNormalizado
                || Normalizar(d.Sigla) == departamentoNormalizado);

            if (coincidenciaDirecta is not null)
            {
                return new ClasificacionPrioridad(coincidenciaDirecta.PrioridadPorDefecto, coincidenciaDirecta.Nombre);
            }
        }

        var textosNormalizados = textos
            .Select(Normalizar)
            .Where(t => t.Length > 0)
            .ToArray();

        foreach (var configurado in departamentosConfigurados
                     .OrderByDescending(d => Normalizar(d.Nombre).Length))
        {
            var nombre = Normalizar(configurado.Nombre);

            if (nombre.Length > 2 && textosNormalizados.Any(t => t.Contains(nombre)))
            {
                return new ClasificacionPrioridad(configurado.PrioridadPorDefecto, configurado.Nombre);
            }

            var sigla = Normalizar(configurado.Sigla);

            if (sigla.Length >= 2 && textosNormalizados.Any(t => ContienePalabra(t, sigla)))
            {
                return new ClasificacionPrioridad(configurado.PrioridadPorDefecto, configurado.Nombre);
            }
        }

        foreach (var texto in textosNormalizados)
        {
            if (PalabrasClavePrioritarias.Any(p => texto.Contains(p)))
            {
                var gerencia = departamentosConfigurados.FirstOrDefault(d => Normalizar(d.Nombre) == "gerencia");
                return new ClasificacionPrioridad(PrioridadTicket.Alta, gerencia?.Nombre ?? "Gerencia");
            }
        }

        return null;
    }

    public static string ClasificarCategoria(
        string? categoriaActual,
        IReadOnlyCollection<CategoriaTicket>? categoriasConfiguradas = null,
        params string?[] textos)
    {
        if (!string.IsNullOrWhiteSpace(categoriaActual))
        {
            var actualNorm = Normalizar(categoriaActual);
            var coincidencia = categoriasConfiguradas?.FirstOrDefault(c => Normalizar(c.Nombre) == actualNorm);
            return coincidencia?.Nombre ?? categoriaActual.Trim();
        }

        var textosNormalizados = textos
            .Select(Normalizar)
            .Where(t => t.Length > 0)
            .ToArray();

        if (categoriasConfiguradas is not null)
        {
            foreach (var cat in categoriasConfiguradas)
            {
                var catNorm = Normalizar(cat.Nombre);
                if (catNorm.Length > 2 && textosNormalizados.Any(t => t.Contains(catNorm)))
                {
                    return cat.Nombre;
                }
            }
        }

        foreach (var (categoria, palabras) in ReglasCategoria)
        {
            if (textosNormalizados.Any(t => palabras.Any(t.Contains)))
            {
                var configurada = categoriasConfiguradas?.FirstOrDefault(c =>
                    Normalizar(c.Nombre) == Normalizar(categoria));

                return configurada?.Nombre ?? categoria;
            }
        }

        var hardwareConfig = categoriasConfiguradas?.FirstOrDefault(c => Normalizar(c.Nombre) == "hardware");
        return hardwareConfig?.Nombre ?? categoriasConfiguradas?.FirstOrDefault()?.Nombre ?? "Hardware";
    }

    private static bool ContienePalabra(string texto, string palabra)
        => Regex.IsMatch(
            texto,
            $@"(?<![\p{{L}}\p{{N}}]){Regex.Escape(palabra)}(?![\p{{L}}\p{{N}}])",
            RegexOptions.CultureInvariant);

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