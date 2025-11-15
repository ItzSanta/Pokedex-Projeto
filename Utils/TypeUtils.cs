using System.Collections.Generic;
using System.Linq;

namespace Pokedex.Utils;

public static class TypeUtils
{
    public static List<string> NormalizeTypes(IEnumerable<string>? types)
    {
        var list = types?
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim().ToLowerInvariant())
            .Distinct()
            .ToList() ?? new List<string>();
        if (list.Count == 0) list = new List<string> { "normal" };
        return list;
    }

    public static bool MatchesType(IEnumerable<string>? types, string filter)
    {
        var list = NormalizeTypes(types);
        if (string.IsNullOrWhiteSpace(filter)) return true;
        var f = filter.Trim().ToLowerInvariant();
        return list.Contains(f);
    }
}