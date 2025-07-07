namespace PokemonTeam.Controllers;

using Microsoft.AspNetCore.Mvc;

public class PokeGachaController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}