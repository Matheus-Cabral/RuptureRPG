namespace Ruptura.Web.Shared;

public static class TableFilter
{
    public static bool Matches(string? term, params string?[] fields)
    {
        if (string.IsNullOrWhiteSpace(term)) return true;

        term = term.Trim();

        foreach (var field in fields)
        {
            if (field is not null && field.Contains(term, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
