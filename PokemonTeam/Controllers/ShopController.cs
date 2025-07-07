using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokemonTeam.Data;
using PokemonTeam.Models;

namespace PokemonTeam.Controllers;

[ApiController]
[Route("api/shop")]
public class ShopController : Controller
{
    private readonly PokemonDbContext _context;

    public ShopController(PokemonDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Liste tous les objets dispo.
    /// </summary>
    [HttpGet("list")]
    public async Task<IActionResult> GetItems()
    {
        var items = await _context.Items.ToListAsync();
        return Ok(items);
    }

    /// <summary>
    /// Achète un objet pour le joueur connecté.
    /// </summary>
    [HttpPost("buy")]
    [Authorize]
    public async Task<IActionResult> BuyItem([FromBody] BuyItemRequest request)
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        var player = await _context.Players
            .Include(p => p.UserAuth)
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.UserAuth.Email == email && p.Game == request.Game);

        if (player == null)
            return NotFound("Player not found");

        var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == request.ItemId);

        if (item == null)
            return NotFound("Item not found");

        if (player.Pokedollar < item.Price)
            return BadRequest("Not enough Pokedollar");

        // Déduit le prix
        player.Pokedollar -= item.Price;

        // Ajoute l'objet au joueur (si pas déjà présent)
        if (!player.Items.Any(i => i.Id == item.Id))
        {
            player.Items.Add(item);
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = $"Item '{item.Name}' acheté avec succès", newBalance = player.Pokedollar });
    }
}

public class BuyItemRequest
{
    public int ItemId { get; set; }
    public string Game { get; set; } = string.Empty;
}
