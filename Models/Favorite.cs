namespace Pokedex.Models;

public class Favorite
{
    public string FavId { get; set; } = Guid.NewGuid().ToString();
    public int PokemonId { get; set; }
    public string Note { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}
