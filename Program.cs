using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Pokedex.Services;

namespace Pokedex;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddLogging();
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://pokeapi.co/api/v2/") });
        builder.Services.AddScoped<IPokeApiService, PokeApiService>();
        builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();

        await builder.Build().RunAsync();
    }
}
