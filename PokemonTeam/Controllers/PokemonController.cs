using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokemonTeam.Data;
using PokemonTeam.Exceptions;
using PokemonTeam.Models;

namespace PokemonTeam.Controllers;

/// <summary>
/// This class represents a Pokémon controller.
/// </summary>
/// <author>Mohamed</author>
/// <seealso cref="Pokemon"/>
/// <seealso cref="Skill"/>
/// <seealso cref="TypeChart"/>
/// <seealso cref="NotEnoughPowerPointsException"/>
/// <seealso cref="UseSkillResponse"/>
[ApiController]
[Route("[controller]")]
public class PokemonController : ControllerBase
{
    private readonly PokemonDbContext _ctx;

    public PokemonController(PokemonDbContext ctx) => _ctx = ctx;

    /// <summary>
    /// This method allows a Pokémon to use a skill.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>Damage dealt</returns>
    /// <exception cref="NotEnoughPowerPointsException"></exception>
    [HttpPost("UseSkill")]
    public async Task<ActionResult<UseSkillResponse>> UseSkill([FromBody] UseSkillRequest request)
    {
        try
        {
            Skill skill = request.Skill;
            Pokemon attacker = request.Attacker;
            Pokemon target = request.Target;

            if (skill.PowerPoints > 0)
            {
                skill.PowerPoints--;

                double typeMultiplier = await TypeChart.GetDamageMultiplierAsync(_ctx, skill.Type, target.types.ToArray());
                double damage = (skill.Damage * (attacker.strength / (double)target.defense)) * typeMultiplier;
                int finalDamage = Math.Max(1, (int)damage);

                target.healthPoint -= (short)finalDamage;

                var response = new UseSkillResponse(finalDamage, target);
                return Ok(response);
            }
            else
            {
                throw new NotEnoughPowerPointsException("Not enough power points to use this skill.");
            }
        }
        catch (NotEnoughPowerPointsException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// This method retrieves a Pokémon by its ID.
    /// </summary>
    /// <param name="id">Pokémon ID</param>
    /// <returns>Pokémon details</returns>
    [HttpGet("getPokemonById/{id}")]
    public async Task<ActionResult<Pokemon>> GetPokemonById(int id)
    {
        var pokemon = await _ctx.Pokemons.FindAsync(id);

        if (pokemon == null)
        {
            return NotFound(new { error = "Pokémon not found." });
        }

        return Ok(pokemon);
    }

    
    /// <summary>
    /// This method retrieves all Pokémon.
    /// </summary>
    /// <returns>List of Pokémon</returns>
    [HttpGet("getAllPokemon")]
    public async Task<ActionResult<List<Pokemon>>> GetAllPokemon()
    {
        var pokemons = await _ctx.Pokemons.ToListAsync();
        return Ok(pokemons);
    }
}
