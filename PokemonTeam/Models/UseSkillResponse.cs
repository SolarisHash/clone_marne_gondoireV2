namespace PokemonTeam.Models;

/// <summary>
/// This class represents a response to the use of a skill.
/// </summary>
/// <remarks>
/// Attributes :
/// <list type="bullet">
///  <item>
///   <description><see cref="DamageDealt"/> : The amount of damage dealt.</description>
/// </item>
/// <item>
///  <description><see cref="Target"/> : The target Pokémon.</description>
/// </item>
/// </list>
/// </remarks>
/// <author>Mohamed</author>
/// <seealso cref="Pokemon"/>
/// <seealso cref="Skill"/>
public class UseSkillResponse
{
    public int DamageDealt { get; set; }
    public Pokemon Target { get; set; }
    
    public UseSkillResponse(int damageDealt, Pokemon target)
    {
        DamageDealt = damageDealt;
        Target = target;
    }
}