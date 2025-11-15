using System.Text.Json.Serialization;

namespace Pokedex.Models;

public class NamedApiResource
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("url")] public string Url { get; set; } = "";
}

public class Pokemon
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("sprites")] public Sprites Sprites { get; set; } = new();
    [JsonPropertyName("types")] public List<TypeSlot> Types { get; set; } = new();
    [JsonPropertyName("stats")] public List<StatSlot> Stats { get; set; } = new();
    [JsonPropertyName("abilities")] public List<AbilitySlot> Abilities { get; set; } = new();
}

public class Sprites
{
    [JsonPropertyName("front_default")] public string? FrontDefault { get; set; }
    [JsonPropertyName("other")] public OtherSprites? Other { get; set; }
}

public class OtherSprites
{
    [JsonPropertyName("official-artwork")] public OfficialArtwork? OfficialArtwork { get; set; }
}

public class OfficialArtwork
{
    [JsonPropertyName("front_default")] public string? FrontDefault { get; set; }
}

public class TypeSlot
{
    [JsonPropertyName("slot")] public int Slot { get; set; }
    [JsonPropertyName("type")] public NamedApiResource Type { get; set; } = new();
}

public class StatSlot
{
    [JsonPropertyName("base_stat")] public int BaseStat { get; set; }
    [JsonPropertyName("stat")] public NamedApiResource Stat { get; set; } = new();
}

public class AbilitySlot
{
    [JsonPropertyName("is_hidden")] public bool IsHidden { get; set; }
    [JsonPropertyName("ability")] public NamedApiResource Ability { get; set; } = new();
}

public class ListResponse<T>
{
    [JsonPropertyName("count")] public int Count { get; set; }
    [JsonPropertyName("results")] public List<T> Results { get; set; } = new();
}

public class PokemonSpecies
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("evolution_chain")] public NamedApiResource EvolutionChain { get; set; } = new();
}

public class EvolutionChain
{
    [JsonPropertyName("chain")] public ChainLink Chain { get; set; } = new();
}

public class ChainLink
{
    [JsonPropertyName("species")] public NamedApiResource Species { get; set; } = new();
    [JsonPropertyName("evolves_to")] public List<ChainLink> EvolvesTo { get; set; } = new();
    [JsonPropertyName("evolution_details")] public List<EvolutionDetail> EvolutionDetails { get; set; } = new();
}

public class EvolutionDetail
{
    [JsonPropertyName("min_level")] public int? MinLevel { get; set; }
    [JsonPropertyName("item")] public NamedApiResource? Item { get; set; }
    [JsonPropertyName("trigger")] public NamedApiResource? Trigger { get; set; }
}

public class AbilityDetail
{
    [JsonPropertyName("effect_entries")] public List<EffectEntry> EffectEntries { get; set; } = new();
}

public class MoveResponse
{
    [JsonPropertyName("moves")] public List<MoveSlot> Moves { get; set; } = new();
}

public class MoveSlot
{
    [JsonPropertyName("move")] public NamedApiResource Move { get; set; } = new();
    [JsonPropertyName("version_group_details")] public List<VersionGroupDetail> VersionGroupDetails { get; set; } = new();
}

public class VersionGroupDetail
{
    [JsonPropertyName("level_learned_at")] public int LevelLearnedAt { get; set; }
    [JsonPropertyName("move_learn_method")] public NamedApiResource MoveLearnMethod { get; set; } = new();
    [JsonPropertyName("version_group")] public NamedApiResource VersionGroup { get; set; } = new();
}

public class MoveDetail
{
    [JsonPropertyName("power")] public int? Power { get; set; }
    [JsonPropertyName("accuracy")] public int? Accuracy { get; set; }
    [JsonPropertyName("pp")] public int? PP { get; set; }
    [JsonPropertyName("type")] public NamedApiResource Type { get; set; } = new();
    [JsonPropertyName("damage_class")] public NamedApiResource DamageClass { get; set; } = new();
}

public class EffectEntry
{
    [JsonPropertyName("effect")] public string Effect { get; set; } = "";
    [JsonPropertyName("short_effect")] public string ShortEffect { get; set; } = "";
    [JsonPropertyName("language")] public NamedApiResource Language { get; set; } = new();
}

public class MoveVM
{
    public string Name { get; set; } = "";
    public string Method { get; set; } = "";
}

public class EvolutionNode
{
    public string Name { get; set; } = "";
    public string? Sprite { get; set; }
    public string Details { get; set; } = "";
    public List<EvolutionNode> Next { get; set; } = new();
}
