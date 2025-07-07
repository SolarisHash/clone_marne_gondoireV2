using System.ComponentModel.DataAnnotations;

namespace PokemonTeam.Models.Auth;

/// <summary>
/// This class is used to receive user registration data.
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <item>
///     <description>`Email` : new user's email.</description>
///   </item>
///   <item>
///     <description>`Password` : new user's password (to be hashed).</description>
///   </item>
/// </list>
/// </remarks>
/// <author>
/// Killian
/// </author>
public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}