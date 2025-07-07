namespace PokemonTeam.Models.PlayerDTO;

public class UpdatePlayerRequest
{
    public string Game { get; set; } = string.Empty; 
    public int Experience { get; set; }
    public int Pokedollar { get; set; }
}
