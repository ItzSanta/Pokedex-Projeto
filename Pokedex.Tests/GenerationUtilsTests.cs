using System;
using System.Collections.Generic;
using System.Linq;

namespace Pokedex.Utils;

public static class GenerationUtils
{
    /// <summary>
    /// Valida os ranges de gerações verificando:
    /// - Start <= End
    /// - Ranges contínuos (sem buracos)
    /// - Ranges não sobrepostos
    /// </summary>
    public static (bool Ok, string? Error) ValidateRanges(
        Dictionary<int, (int Start, int End)> ranges)
    {
        if (ranges == null || ranges.Count == 0)
            return (false, "No ranges provided");

        // Ordena por chave (geração)
        var ordered = ranges.OrderBy(r => r.Key).ToList();

        for (int i = 0; i < ordered.Count; i++)
        {
            var (generation, (start, end)) = ordered[i];

            // Start deve ser <= End
            if (start > end)
                return (false, $"Generation {generation} has Start > End");

            // Se não é o primeiro, checar continuidade
            if (i > 0)
            {
                var prevEnd = ordered[i - 1].Value.End;

                if (start != prevEnd + 1)
                    return (false, $"Generation {generation} is not continuous with previous range");
            }
        }

        return (true, null);
    }

    /// <summary>
    /// Gera uma lista sequencial de IDs entre Start e End (inclusive).
    /// </summary>
    public static List<int> BuildSequentialIds((int Start, int End) range)
    {
        var (start, end) = range;

        if (start > end)
            return new List<int>();

        return Enumerable.Range(start, end - start + 1).ToList();
    }
}
