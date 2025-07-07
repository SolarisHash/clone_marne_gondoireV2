using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using PokemonTeam.Models;
using PokemonTeam.Services;
using PokemonTeam.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using PokemonTeam.Models.Auth;

namespace PokemonTeam.Controllers;
/// <summary>
/// This controller manages authentication operations: registration, login, logout and token validation.
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <item>
///     <description>`Register()` : creates a new user account.</description>
///   </item>
///   <item>
///     <description>`Login()` : validates credentials and returns a JWT token in a cookie.</description>
///   </item>
///   <item>
///     <description>`Logout()` : deletes the authentication cookie.</description>
///   </item>
///   <item>
///     <description>`Check()` : validates that the current user is authenticated.</description>
///   </item>
/// </list>
/// </remarks>
/// <author>
/// Killian
/// </author>
[ApiController]
[Route("auth")]
public class AuthController : Controller
{
    private readonly PokemonDbContext _context;
    private readonly IPasswordHasher<UserAuthModel> _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthController(PokemonDbContext context,
                          IPasswordHasher<UserAuthModel> passwordHasher,
                          ITokenService tokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest("Email ou mot de passe invalide");

        if (await _context.UserAuths.AnyAsync(u => u.Email == request.Email))
            return Conflict("Email déjà utilisé");

        var user = new UserAuthModel
        {
            Email = request.Email
        };

        user.Password = _passwordHasher.HashPassword(user, request.Password);

        _context.UserAuths.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Utilisateur enregistré avec succès" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest("Requête invalide");

        var user = await _context.UserAuths.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
        {
            Console.WriteLine($"DEBUG: Utilisateur non trouvé pour email: {request.Email}");
            return Unauthorized("Email ou mot de passe incorrect");
        }

        Console.WriteLine($"DEBUG: Utilisateur trouvé: {user.Email}");
        Console.WriteLine($"DEBUG: Hash stocké: {user.Password}");
        Console.WriteLine($"DEBUG: Mot de passe reçu: {request.Password}");

        var result = _passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);
        Console.WriteLine($"DEBUG: Résultat vérification: {result}");

        if (result == PasswordVerificationResult.Failed)
        {
            Console.WriteLine("DEBUG: Échec de la vérification du mot de passe");
            return Unauthorized("Email ou mot de passe incorrect");
        }

        var token = _tokenService.CreateToken(user.Id, user.Email);

        Response.Cookies.Append("access_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(60),
            Path = "/"
        });

        Console.WriteLine("DEBUG: Connexion réussie, token créé");
        return Ok(new { message = "Connexion réussie" });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("access_token");
        return Ok(new { message = "Déconnexion réussie" });
    }

    [HttpGet("check")]
    [Authorize]
    public IActionResult Check()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        return Ok(new { message = "Utilisateur authentifié", userId, email });
    }
}
