namespace PokemonTeam.Models;

/// <summary>
/// This class represents a request to use a skill.
/// </summary>
/// <remarks>
/// Attributes :
/// <list type="bullet">
///  <item>
///  <description><see cref="Attacker"/> : The Pokémon that uses the skill.</description>
/// </item>
/// <item>
/// <description><see cref="Target"/> : The target Pokémon.</description>
/// </item>
/// <item>
/// <description><see cref="Skill"/> : The skill used.</description>
/// </item>
/// </list>
/// </remarks>
/// <author>Mohamed</author>
/// <seealso cref="Pokemon"/>
/// <seealso cref="Skill"/>
public class UseSkillRequest
{
    public Pokemon Attacker { get; set; }
    public Pokemon Target { get; set; }
    public Skill Skill { get; set; }

    public UseSkillRequest() { }

    public UseSkillRequest(Pokemon attacker, Pokemon target, Skill skill)
    {
        Attacker = attacker;
        Target = target;
        Skill = skill;
    }
}