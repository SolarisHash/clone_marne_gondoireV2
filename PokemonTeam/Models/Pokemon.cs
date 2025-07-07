namespace PokemonTeam.Models;

using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// This class represents a Pokemon.
/// </summary>
/// <remarks>
/// Attributes :
/// <list type="bullet">
///   <item>
///     <description><see cref="name"/> : Name of the pokemon. </description>
///   </item>
///   <item>
///     <description><see cref="types"/> : Type(s) of the pokemon.</description>
///   </item>
///   <item>
///     <description><see cref="healthPoint"/> : Actual health points of the pokemon.</description>
///   </item>
///   <item>
///     <description><see cref="maxHealthPoint"/> : Maximum health points of the pokemon.</description>
///   </item>
///   <item>
///     <description><see cref="accuracy"/> : Accuracy of the pokemon.</description>
///   </item>
///   <item>
///     <description><see cref="defense"/> : Defense of the pokemon.</description>
///   </item>
///   <item>
///     <description><see cref="strength"/> : Strength of the pokemon.</description>
///   </item>
///   <item>
///     <description><see cref="speed"/> : Speed of the pokemon.</description>
///   </item>
///   <item>
///     <description><see cref="unlockedXp"/> : Player's experience needed to get this pokemon.</description>
///   </item>
///   <item>
///     <description><see cref="skills"/> : Skills of the pokemon.</description>
///   </item>
///   <item>
///     <description><see cref="evolveTo"/> : Pokemon evolution of this pokemon. (set null if no evolution)</description>
///   </item>
/// </list>
/// </remarks>
/// <author>Mohamed</author>
/// <seealso cref="Skill"/>
public class Pokemon
{
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string name { get; private set; }

    [NotMapped]
    public List<string> types { get; private set; } = new();

    [Column("health_point")]
    public short healthPoint { get; set; }

    [Column("max_health_point")]
    public short maxHealthPoint { get; private set; }
    
    [Column("defense")]
    public short defense { get; private set; }

    [Column("strength")]
    public short strength { get; private set; }

    [Column("speed")]
    public short speed { get; private set; }

    [Column("unlocked_experience")]
    public int unlockedXp { get; set; }

    // relations
    [NotMapped]
    public List<Skill>? skills { get; set; }

    [NotMapped]
    public Pokemon? evolveTo { get; set; }

    // EF constructor
    public Pokemon() { }
    
    public Pokemon(string name, 
        List<String> types, 
        byte healthPoint, 
        byte maxHealthPoint, 
        byte defense, 
        byte strength, 
        byte speed, 
        int unlockedXp, 
        List<Skill> skills, 
        Pokemon evolveTo = null)
    {
        this.name = name;
        this.types = types;
        this.healthPoint = healthPoint;
        this.maxHealthPoint = maxHealthPoint;
        this.defense = defense;
        this.strength = strength;
        this.speed = speed;
        this.unlockedXp = unlockedXp;
        this.skills = skills;
        this.evolveTo = evolveTo;
    }
}