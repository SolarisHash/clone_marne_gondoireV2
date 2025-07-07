using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace PokemonTeam.Controllers;

public class PokemonArenaController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Combat()
    {
        return View();
    }

    public IActionResult Shop()
    {
        return View();
    }

    public IActionResult Profile()
    {
        return View();
    }
}