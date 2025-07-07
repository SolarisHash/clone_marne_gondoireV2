using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using PokemonTeam.Data;
using PokemonTeam.Models;

[ApiController]
[Route("api/[controller]")]
public class TypeChartController : ControllerBase
{
    private readonly PokemonDbContext _ctx;
    public TypeChartController(PokemonDbContext ctx) => _ctx = ctx;

    [HttpGet("multiplier")]
    public async Task<IActionResult> GetMultiplier(
        [FromQuery] string attackType,
        [FromQuery] string[] defenderTypes)
    {
        var value = await TypeChart.GetDamageMultiplierAsync(
            _ctx, attackType, defenderTypes);
        return Ok(new { multiplier = value });
    }
}