namespace AuraLearn.Application.Services;

/// <summary>
/// Normalizador de nombres de materias: primera letra mayúscula en cada
/// palabra significativa, conectores en minúscula (salvo la primera palabra).
/// "cálculo i" → "Cálculo I" · "estadística y probabilidad" → "Estadística y
/// Probabilidad". Preserva las mayúsculas internas ya escritas (TOEFL, EDOs).
/// Regla de producto: las materias se guardan SIEMPRE normalizadas.
/// </summary>
public static class SubjectNormalizer
{
    private static readonly HashSet<string> LowerCaseWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "de", "del", "la", "las", "el", "los", "y", "e", "o", "u", "para", "con", "en", "al", "a",
    };

    public static string Normalize(string subject)
    {
        var trimmed = subject.Trim();
        if (trimmed.Length == 0)
            return trimmed;

        var words = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var result = new List<string>(words.Length);
        for (var i = 0; i < words.Length; i++)
        {
            var word = words[i];
            if (i > 0 && LowerCaseWords.Contains(word))
                result.Add(word.ToLowerInvariant());
            else
                result.Add(char.ToUpperInvariant(word[0]) + word[1..]);
        }

        return string.Join(' ', result);
    }
}
