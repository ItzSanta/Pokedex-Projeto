using System.Collections.Generic;
using System.Linq;

namespace Pokedex.Utils;

public static class GenerationUtils
{
    /// <summary>
    /// Valida intervalos sequenciais por geração garantindo início em 1 e ausência de lacunas/sobreposições.
    /// </summary>
    public static (bool Ok, string? Error) ValidateRanges(Dictionary<int,(int Start,int End)> ranges)
    {
        if (ranges.Count == 0) return (false, "Sem intervalos definidos.");
        var ordered = ranges.OrderBy(k => k.Value.Start).ToList();
        if (ordered[0].Value.Start != 1) return (false, "Intervalos inválidos: devem começar em 1.");
        for (int i = 1; i < ordered.Count; i++)
        {
            var prev = ordered[i-1].Value; var curr = ordered[i].Value;
            if (prev.End + 1 != curr.Start) return (false, "Intervalos com lacunas ou sobreposições detectadas.");
        }
        return (true, null);
    }

    /// <summary>
    /// Constrói uma lista de IDs contínua e ordenada a partir do intervalo informado.
    /// </summary>
    public static List<int> BuildSequentialIds((int Start,int End) range)
        => Enumerable.Range(range.Start, range.End - range.Start + 1).ToList();
}