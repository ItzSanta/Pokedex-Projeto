using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Pokedex.Models;

public class TypeRelationsResponse
{
    [JsonPropertyName("damage_relations")]
    public DamageRelations DamageRelations { get; set; } = new();
}

public class DamageRelations
{
    [JsonPropertyName("double_damage_from")]
    public List<NamedApiResource> DoubleDamageFrom { get; set; } = new();

    [JsonPropertyName("half_damage_from")]
    public List<NamedApiResource> HalfDamageFrom { get; set; } = new();

    [JsonPropertyName("no_damage_from")]
    public List<NamedApiResource> NoDamageFrom { get; set; } = new();

    [JsonPropertyName("double_damage_to")]
    public List<NamedApiResource> DoubleDamageTo { get; set; } = new();

    [JsonPropertyName("half_damage_to")]
    public List<NamedApiResource> HalfDamageTo { get; set; } = new();

    [JsonPropertyName("no_damage_to")]
    public List<NamedApiResource> NoDamageTo { get; set; } = new();
}

public class TypeRelationsVM
{
    public List<string> Weaknesses { get; set; } = new();
    public List<string> Resistances { get; set; } = new();
    public List<string> Immunities { get; set; } = new();
}