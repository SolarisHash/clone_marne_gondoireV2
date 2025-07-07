using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokemonTeam.Data;
using PokemonTeam.Models;
using PokemonTeam.Models.PlayerDTO;

namespace PokemonTeam.Controllers;

[ApiController]
[Route("api/player")]
public class PlayerController : Controller
{
    private readonly PokemonDbContext _context;

    public PlayerController(PokemonDbContext context)
    {
        _context = context;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyPlayer([FromQuery] string game)
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        var player = await _context.Players
            .Include(p => p.UserAuth)
            .FirstOrDefaultAsync(p => p.UserAuth.Email == email && p.Game == game);

        if (player == null) return NotFound();

        return Ok(player);
    }


    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> CreatePlayer([FromBody] CreatePlayerRequest request)
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var user = await _context.UserAuths.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null) return Unauthorized();

        // Vérifier qu'il n'a pas déjà un Player pour ce jeu
        var existing = await _context.Players
            .FirstOrDefaultAsync(p => p.fk_user_auth == user.Id && p.Game == request.Game);

        if (existing != null) return Conflict("Player déjà existant pour ce jeu.");

        var newPlayer = new Player
        {
            Name = request.Name,
            Game = request.Game,
            Pokedollar = 100,
            Experience = 0,
            fk_user_auth = user.Id
        };

        _context.Players.Add(newPlayer);
        await _context.SaveChangesAsync();

        return Ok(newPlayer);
    }


    [HttpPost("update")]
    [Authorize]
    public async Task<IActionResult> UpdatePlayerProgress([FromBody] UpdatePlayerRequest request)
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        var player = await _context.Players
            .Include(p => p.UserAuth)
            .FirstOrDefaultAsync(p => p.UserAuth.Email == email && p.Game == request.Game);

        if (player == null) return NotFound();

        player.Experience += request.Experience;
        player.Pokedollar += request.Pokedollar;

        await _context.SaveChangesAsync();

        return Ok(player);
    }


    [HttpDelete("delete")]
    [Authorize]
    public async Task<IActionResult> DeleteMyPlayer([FromQuery] string game)
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        var player = await _context.Players
            .Include(p => p.UserAuth)
            .FirstOrDefaultAsync(p => p.UserAuth.Email == email && p.Game == game);

        if (player == null) return NotFound();

        _context.Players.Remove(player);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Player for game {game} deleted" });
    }
    
    [HttpGet("my-items")]
    [Authorize]
    public async Task<IActionResult> GetMyItems([FromQuery] string game)
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        var player = await _context.Players
            .Include(p => p.Items)
            .Include(p => p.UserAuth)
            .FirstOrDefaultAsync(p => p.UserAuth.Email == email && p.Game == game);

        if (player == null) return NotFound();

        return Ok(player.Items);
    }

    [HttpPost("use-item")]
    [Authorize]
    public async Task<IActionResult> UseItem([FromBody] UseItemRequest request)
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        var player = await _context.Players
            .Include(p => p.Items)
            .Include(p => p.UserAuth)
            .FirstOrDefaultAsync(p => p.UserAuth.Email == email && p.Game == request.Game);

        if (player == null) return NotFound();

        var item = player.Items.FirstOrDefault(i => i.Id == request.ItemId);
        if (item == null) return BadRequest("Item non possédé.");

        // Retire l'item
        player.Items.Remove(item);

        await _context.SaveChangesAsync();

        return Ok(new { message = $"Item '{item.Name}' utilisé et retiré de l'inventaire." });
    }

    public class UseItemRequest
    {
        public int ItemId { get; set; }
        public string Game { get; set; } = string.Empty;
    }

}
