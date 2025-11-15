using FluentAssertions;
using System.Collections.Generic;
using Xunit;

public class TypeUtilsTests
{
    [Fact]
    public void NormalizeTypes_ReturnsNormal_WhenNull_InputNull()
    {
        var res = Pokedex.Utils.TypeUtils.NormalizeTypes((IEnumerable<string>?)null);
        res.Should().ContainSingle("normal");
    }

    [Fact]
    public void NormalizeTypes_ReturnsNormal_WhenEmpty_ListEmpty()
    {
        var res = Pokedex.Utils.TypeUtils.NormalizeTypes(new List<string>());
        res.Should().ContainSingle("normal");
    }

    [Fact]
    public void NormalizeTypes_LowercasesAndDistincts_MixedCases()
    {
        var res = Pokedex.Utils.TypeUtils.NormalizeTypes(new[] { "Fire", "FIRE", "water" });
        res.Should().BeEquivalentTo(new[] { "fire", "water" });
    }

    [Fact]
    public void MatchesType_True_WhenFilterEmpty_StringEmpty()
    {
        Pokedex.Utils.TypeUtils.MatchesType(new[] { "fire" }, "").Should().BeTrue();
    }

    [Fact]
    public void MatchesType_ConsidersNormal_WhenNoTypes_ListEmpty()
    {
        Pokedex.Utils.TypeUtils.MatchesType(new List<string>(), "normal").Should().BeTrue();
    }
}