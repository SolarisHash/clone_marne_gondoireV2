namespace PokemonTeam.Services;

public interface ITokenService
{
    string CreateToken(int userId, string email);
}