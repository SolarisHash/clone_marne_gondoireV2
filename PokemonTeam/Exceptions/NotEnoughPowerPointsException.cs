namespace PokemonTeam.Exceptions;

public class NotEnoughPowerPointsException : Exception 
{
    public NotEnoughPowerPointsException(string message) : base(message) { }
}