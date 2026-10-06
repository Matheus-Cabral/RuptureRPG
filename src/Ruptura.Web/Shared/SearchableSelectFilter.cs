using System.Globalization;
using System.Text;

namespace Ruptura.Web.Shared;

public sealed record SearchableOption<TValue>(TValue Value, string Label, string? Group = null);

public static class SearchableSelectFilter
{
    public static List<SearchableOption<TValue>> Filter<TValue>(
        IEnumerable<SearchableOption<TValue>> options, string? query)
    {
        if (string.IsNullOrWhiteSpace(query)) return options.ToList();

        var term = Normalize(query);
        return options.Where(o => Normalize(o.Label).Contains(term, StringComparison.Ordinal)).ToList();
    }

    // Lowercased with diacritics stripped, so "magia" finds "Mágia" — GMs type catalog names with
    // accents, players searching on a phone keyboard usually don't.
    public static string Normalize(string text)
    {
        var decomposed = text.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                builder.Append(char.ToLowerInvariant(c));
        }

        return builder.ToString();
    }
}
