using System.Net.Http.Json;
using Pokedex.Models;
using System.Text.Json;

namespace Pokedex.Services;

public interface IPokeApiService
{
    /// <summary>Retorna todos os nomes de Pokémon disponíveis (até ~2000 itens).</summary>
    Task<List<NamedApiResource>> GetAllPokemonNamesAsync(CancellationToken ct = default);
    /// <summary>Retorna uma página de recursos de Pokémon (nome/url) para paginação inicial.</summary>
    Task<List<NamedApiResource>> GetPokemonPageAsync(int offset, int limit, CancellationToken ct = default);
    /// <summary>Obtém um Pokémon por nome ou ID. Normaliza listas nulas e sprites.</summary>
    Task<Pokemon?> GetPokemonAsync(string nameOrId, CancellationToken ct = default);
    /// <summary>Obtém dados da espécie (usado para evolução).</summary>
    Task<PokemonSpecies?> GetSpeciesAsync(int id, CancellationToken ct = default);
    /// <summary>Obtém cadeia de evolução a partir da URL fornecida pela espécie.</summary>
    Task<EvolutionChain?> GetEvolutionChainAsync(string url, CancellationToken ct = default);
    /// <summary>Obtém detalhes da habilidade (usado para tooltip).</summary>
    Task<AbilityDetail?> GetAbilityAsync(string name, CancellationToken ct = default);
    Task<List<NamedApiResource>> GetGenerationSpeciesAsync(int generationId, bool forceRefresh = false, CancellationToken ct = default);
    /// <summary>Obtém relações de dano do tipo (fraquezas, resistências, imunidades).</summary>
    Task<TypeRelationsResponse?> GetTypeRelationsAsync(string typeName, CancellationToken ct = default);
    /// <summary>Obtém movimentos do Pokémon (usado para aba de detalhes).</summary>
    Task<List<MoveSlot>?> GetPokemonMovesAsync(string nameOrId, CancellationToken ct = default);
    /// <summary>Obtém detalhes de um movimento específico.</summary>
    Task<MoveDetail?> GetMoveDetailAsync(string moveName, CancellationToken ct = default);
}

public class PokeApiService : IPokeApiService
{
    private readonly HttpClient _http;
    private readonly Microsoft.Extensions.Logging.ILogger<PokeApiService>? _logger;
    private const int MaxRetries = 2;
    private const int CacheTtlSeconds = 3600;
    private readonly Dictionary<string, CacheEntry<Pokemon>> _memCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, CacheEntry<MoveDetail>> _moveCache = new(StringComparer.OrdinalIgnoreCase);

    public PokeApiService(HttpClient http, Microsoft.Extensions.Logging.ILogger<PokeApiService>? logger = null) { _http = http; _logger = logger; }

    public async Task<List<NamedApiResource>> GetAllPokemonNamesAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("Fetching all pokemon names");
        var list = await Retry(async () => await _http.GetFromJsonAsync<ListResponse<NamedApiResource>>("pokemon?limit=2000&offset=0", ct));
        return list?.Results ?? new List<NamedApiResource>();
    }

    public async Task<List<NamedApiResource>> GetPokemonPageAsync(int offset, int limit, CancellationToken ct = default)
    {
        _logger?.LogInformation("Fetching pokemon page offset={Offset} limit={Limit}", offset, limit);
        var list = await Retry(async () => await _http.GetFromJsonAsync<ListResponse<NamedApiResource>>($"pokemon?limit={limit}&offset={offset}", ct));
        return list?.Results ?? new List<NamedApiResource>();
    }

    public async Task<Pokemon?> GetPokemonAsync(string nameOrId, CancellationToken ct = default)
    {
        var key = nameOrId.Trim().ToLowerInvariant();
        _logger?.LogInformation("GetPokemonAsync iniciado para: {Key}", key);
        
        if (TryGetFromCache(key, out var cached))
        {
            _logger?.LogInformation("Pokemon {Key} encontrado em cache", key);
            return cached;
        }
        
        _logger?.LogInformation("Buscando pokemon {Key} na API", nameOrId);
        Pokemon? p = null;
        
        try
        {
            p = await Retry(async () => await _http.GetFromJsonAsync<Pokemon>($"pokemon/{nameOrId}", ct));
            
            if (p != null)
            {
                _logger?.LogInformation("Pokemon {Key} recebido com sucesso. ID: {Id}, Nome: {Name}", key, p.Id, p.Name);
                
                // Normalização de dados
                p.Types ??= new List<TypeSlot>();
                if (p.Types.Count == 0)
                {
                    _logger?.LogWarning("Pokemon {Key} não tem tipos definidos. Aplicando tipo 'normal'", key);
                    p.Types = new List<TypeSlot> { new TypeSlot { Slot = 1, Type = new NamedApiResource { Name = "normal" } } };
                }
                
                p.Stats ??= new List<StatSlot>();
                p.Abilities ??= new List<AbilitySlot>();
                p.Sprites ??= new Sprites();
                
                // Verificar sprite
                var spriteUrl = p.Sprites.Other?.OfficialArtwork?.FrontDefault ?? p.Sprites.FrontDefault ?? string.Empty;
                if (string.IsNullOrEmpty(spriteUrl))
                {
                    _logger?.LogWarning("Pokemon {Key} não tem sprite disponível", key);
                    // Usar sprite placeholder ou default
                    p.Sprites.FrontDefault = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/0.png";
                }
                
                PutInCache(p);
                _logger?.LogInformation("Pokemon {Key} armazenado em cache com sucesso", key);
            }
            else
            {
                _logger?.LogWarning("Pokemon {Key} retornou null da API", key);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Erro ao buscar pokemon {Key}: {Message}", key, ex.Message);
        }
        
        return p;
    }

    public Task<PokemonSpecies?> GetSpeciesAsync(int id, CancellationToken ct = default)
        => Retry(async () => await _http.GetFromJsonAsync<PokemonSpecies>($"pokemon-species/{id}", ct));

    public async Task<EvolutionChain?> GetEvolutionChainAsync(string url, CancellationToken ct = default)
    {
        var abs = new Uri(url);
        using var req = new HttpRequestMessage(HttpMethod.Get, abs);
        var res = await Retry(async () => await _http.SendAsync(req, ct));
        if (res == null || !res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<EvolutionChain>(cancellationToken: ct);
    }

    public Task<AbilityDetail?> GetAbilityAsync(string name, CancellationToken ct = default)
        => Retry(async () => await _http.GetFromJsonAsync<AbilityDetail>($"ability/{name}", ct));

    public async Task<List<NamedApiResource>> GetGenerationSpeciesAsync(int generationId, bool forceRefresh = false, CancellationToken ct = default)
    {
        var gen = await Retry(async () => await _http.GetFromJsonAsync<JsonElement>($"generation/{generationId}", ct));
        if (gen.ValueKind == JsonValueKind.Undefined) return new List<NamedApiResource>();
        var list = new List<NamedApiResource>();
        if (gen.TryGetProperty("pokemon_species", out var arr) && arr.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in arr.EnumerateArray())
            {
                var name = item.GetProperty("name").GetString() ?? "";
                var url = item.GetProperty("url").GetString() ?? "";
                list.Add(new NamedApiResource { Name = name, Url = url });
            }
        }
        list = list.OrderBy(x => x.Name).ToList();
        return list;
    }

    

    private async Task<T?> Retry<T>(Func<Task<T?>> action)
    {
        int attempt = 0;
        while (true)
        {
            try 
            { 
                _logger?.LogInformation("Tentativa {Attempt} de {MaxRetries}", attempt + 1, MaxRetries + 1);
                var result = await action(); 
                if (result != null)
                {
                    _logger?.LogInformation("Tentativa {Attempt} bem sucedida", attempt + 1);
                    return result;
                }
                _logger?.LogWarning("Tentativa {Attempt} retornou null", attempt + 1);
            }
            catch (HttpRequestException ex) 
            { 
                _logger?.LogWarning(ex, "Erro HTTP na tentativa {Attempt}: {StatusCode} - {Message}", attempt + 1, ex.StatusCode, ex.Message); 
            }
            catch (TaskCanceledException ex) 
            { 
                _logger?.LogWarning(ex, "Timeout na tentativa {Attempt}: {Message}", attempt + 1, ex.Message); 
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Erro inesperado na tentativa {Attempt}: {Message}", attempt + 1, ex.Message);
            }
            
            if (++attempt > MaxRetries) 
            {
                _logger?.LogError("Máximo de tentativas atingido. Retornando default.");
                return default;
            }
            
            var delay = 300 * attempt;
            _logger?.LogInformation("Aguardando {Delay}ms antes da próxima tentativa", delay);
            await Task.Delay(delay);
        }
    }

    private bool TryGetFromCache(string key, out Pokemon? p)
    {
        _logger?.LogInformation("Verificando cache para chave: {Key}", key);
        
        if (_memCache.TryGetValue(key, out var entry))
        {
            var age = (DateTimeOffset.UtcNow - entry.CachedAt).TotalSeconds;
            _logger?.LogInformation("Entrada encontrada em cache para {Key}. Idade: {Age}s", key, age);
            
            if (age <= CacheTtlSeconds)
            {
                _logger?.LogInformation("Cache válido para {Key}", key);
                p = entry.Value;
                return true;
            }
            
            _logger?.LogInformation("Cache expirado para {Key}. Removendo.", key);
            _memCache.Remove(key);
        }
        else
        {
            _logger?.LogInformation("Nenhuma entrada encontrada em cache para {Key}", key);
        }
        
        p = null;
        return false;
    }

    private void PutInCache(Pokemon p)
    {
        var entry = new CacheEntry<Pokemon> { Value = p, CachedAt = DateTimeOffset.UtcNow };
        _memCache[p.Id.ToString()] = entry;
        _memCache[p.Name] = entry;
    }

    private class CacheEntry<T>
    {
        public T Value { get; set; } = default!;
        public DateTimeOffset CachedAt { get; set; }
    }

    public async Task<TypeRelationsResponse?> GetTypeRelationsAsync(string typeName, CancellationToken ct = default)
    {
        var key = typeName.Trim().ToLowerInvariant();
        _logger?.LogInformation("GetTypeRelationsAsync iniciado para tipo: {Key}", key);
        try
        {
            var res = await Retry(async () => await _http.GetFromJsonAsync<TypeRelationsResponse>($"type/{key}", ct));
            _logger?.LogInformation("Relações de tipo {Key} obtidas com sucesso", key);
            return res;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Falha ao obter relações de tipo para {Key}", key);
            return null;
        }
    }

    public async Task<List<MoveSlot>?> GetPokemonMovesAsync(string nameOrId, CancellationToken ct = default)
    {
        var key = nameOrId.Trim().ToLowerInvariant();
        _logger?.LogInformation("GetPokemonMovesAsync iniciado para: {Key}", key);
        try
        {
            var res = await Retry(async () => await _http.GetFromJsonAsync<MoveResponse>($"pokemon/{key}", ct));
            _logger?.LogInformation("Movimentos de {Key} obtidos com sucesso", key);
            return res?.Moves;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Falha ao obter movimentos para {Key}", key);
            return null;
        }
    }

    public async Task<MoveDetail?> GetMoveDetailAsync(string moveName, CancellationToken ct = default)
    {
        var key = moveName.Trim().ToLowerInvariant();
        if (_moveCache.TryGetValue(key, out var entry))
        {
            var age = (DateTimeOffset.UtcNow - entry.CachedAt).TotalSeconds;
            if (age <= CacheTtlSeconds) return entry.Value;
            _moveCache.Remove(key);
        }
        try
        {
            var res = await Retry(async () => await _http.GetFromJsonAsync<MoveDetail>($"move/{key}", ct));
            if (res != null) _moveCache[key] = new CacheEntry<MoveDetail> { Value = res, CachedAt = DateTimeOffset.UtcNow };
            return res;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Falha ao obter detalhes do movimento {Key}", key);
            return null;
        }
    }

}
