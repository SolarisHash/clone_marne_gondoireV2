using Microsoft.AspNetCore.Mvc;

namespace PokemonTeam.Controllers;

public class AuthPageController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}