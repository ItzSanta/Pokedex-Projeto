using Microsoft.JSInterop;

namespace Pokedex.Services;

public interface ILocalStorageService
{
    Task<T?> GetAsync<T>(string key);
    Task<bool> SetAsync<T>(string key, T value);
    Task<bool> RemoveAsync(string key);
}

public class LocalStorageService : ILocalStorageService
{
    private readonly IJSRuntime _js;
    public LocalStorageService(IJSRuntime js) { _js = js; }

    public async Task<T?> GetAsync<T>(string key)
    {
        var json = await _js.InvokeAsync<string?>("pokedexLocalStorage.getItem", key);
        if (string.IsNullOrWhiteSpace(json)) return default;
        try { return System.Text.Json.JsonSerializer.Deserialize<T>(json); }
        catch { return default; }
    }

    public async Task<bool> SetAsync<T>(string key, T value)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(value);
        return await _js.InvokeAsync<bool>("pokedexLocalStorage.setItem", key, json);
    }

    public Task<bool> RemoveAsync(string key)
        => _js.InvokeAsync<bool>("pokedexLocalStorage.removeItem", key).AsTask();
}
