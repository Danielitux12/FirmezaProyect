namespace Firmeza.Application.DTOs.Excel;

/// <summary>
/// Representa una fila leída de un archivo Excel con sus valores mapeados por nombre de columna.
/// </summary>
public class ExcelRowData
{
    public int RowIndex { get; set; }
    public string SheetName { get; set; } = string.Empty;
    public Dictionary<string, string> Values { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Retorna el primer valor no vacío que coincida con alguna de las claves provistas.
    /// </summary>
    public string? GetValue(params string[] candidateKeys)
    {
        foreach (var key in candidateKeys)
        {
            var normalizedKey = NormalizeKey(key);
            if (Values.TryGetValue(normalizedKey, out var val) && !string.IsNullOrWhiteSpace(val))
            {
                return val.Trim();
            }
        }
        return null;
    }

    /// <summary>
    /// Normaliza una clave eliminando tildes, espacios y caracteres especiales para comparación insensible.
    /// </summary>
    public static string NormalizeKey(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        return text.Trim().ToLowerInvariant()
            .Replace(" ", "")
            .Replace("_", "")
            .Replace("-", "")
            .Replace(".", "")
            .Replace("á", "a")
            .Replace("é", "e")
            .Replace("í", "i")
            .Replace("ó", "o")
            .Replace("ú", "u")
            .Replace("ñ", "n");
    }
}
