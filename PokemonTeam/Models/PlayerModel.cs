namespace PokemonTeam.Models;

public class Player
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Game { get; set; } = string.Empty;
    public int Pokedollar { get; set; } = 0;
    public int Experience { get; set; } = 0;
    
    public int fk_user_auth { get; set; }
    public UserAuthModel? UserAuth { get; set; }

    // public ICollection<Pokemon> Pokemons { get; set; } = new List<Pokemon>();
    public ICollection<Item> Items { get; set; } = new List<Item>();

}