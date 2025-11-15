using FluentAssertions;
using Pokedex.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class TypeRelationsTests
{
    [Fact]
    public void NormalizeTypes_RemovesDuplicates()
    {
        var res = Pokedex.Utils.TypeUtils.NormalizeTypes(new[] { "fire", "fire", "water" });
        res.Should().BeEquivalentTo(new[] { "fire", "water" });
    }

    [Fact]
    public void NormalizeTypes_TrimsAndLowercases()
    {
        var res = Pokedex.Utils.TypeUtils.NormalizeTypes(new[] { " Fire ", " WATER" });
        res.Should().BeEquivalentTo(new[] { "fire", "water" });
    }

    [Fact]
    public void MatchesType_HandlesNull()
    {
        Pokedex.Utils.TypeUtils.MatchesType((IEnumerable<string>?)null, "normal").Should().BeTrue();
        Pokedex.Utils.TypeUtils.MatchesType(new List<string>(), "normal").Should().BeTrue();
    }

    [Fact]
    public void MatchesType_MatchesCaseInsensitive()
    {
        Pokedex.Utils.TypeUtils.MatchesType(new[] { "Water" }, "water").Should().BeTrue();
        Pokedex.Utils.TypeUtils.MatchesType(new[] { "fire" }, "Fire").Should().BeTrue();
    }
}

public class CapitalizeTests
{
    [Theory]
    [InlineData("fire", "Fire")]
    [InlineData("WATER", "Water")]
    [InlineData("", "")]
    [InlineData(null, null)]
    public void Cap_FormatsCorrectly(string input, string expected)
    {
        Pokedex.Shared.PokemonTabs.Cap(input).Should().Be(expected);
    }
}