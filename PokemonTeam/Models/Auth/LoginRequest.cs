using System.ComponentModel.DataAnnotations;

namespace PokemonTeam.Models.Auth;

/// <summary>
/// This class is used to receive user login credentials.
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <item>
///     <description>`Email` : user's email.</description>
///   </item>
///   <item>
///     <description>`Password` : user's raw password to validate.</description>
///   </item>
/// </list>
/// </remarks>
/// <author>
/// Killian
/// </author>
public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}