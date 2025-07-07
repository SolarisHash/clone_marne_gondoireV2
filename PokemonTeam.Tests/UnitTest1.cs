using Xunit;
using PokemonTeam.Models;

namespace PokemonTeam.Tests;

public class PokemonTests
{
    [Fact]
    public void Constructor_Should_Create_Valid_Pokemon()
    {
        // Arrange
        var name = "Pikachu";
        var types = new List<string> { "electric" };
        var hp = 35;
        var maxHp = 35;
        var accuracy = 95;
        var defense = 40;
        var strength = 55;
        var speed = 90;
        var xp = 0;
        var skills = new List<Skill>(); // Tu peux faire un mock ou vide pour l'instant

        // Act
        var pikachu = new Pokemon(
            name,
            types,
            hp,
            maxHp,
            accuracy,
            defense,
            strength,
            speed,
            xp,
            skills
        );

        // Assert
        Assert.Equal(name, pikachu.name);
        Assert.Equal(types, pikachu.types);
        Assert.Equal(hp, pikachu.healthPoint);
        Assert.Equal(maxHp, pikachu.maxHealthPoint);
        Assert.Equal(accuracy, pikachu.accuracy);
        Assert.Equal(defense, pikachu.defense);
        Assert.Equal(strength, pikachu.strength);
        Assert.Equal(speed, pikachu.speed);
        Assert.Equal(xp, pikachu.unlockedXp);
        Assert.Equal(skills, pikachu.skills);
        Assert.Null(pikachu.evolveTo);
    }
}
