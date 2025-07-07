namespace PokemonTeam.Models;

/// <summary>
/// This class represents the user authentication entity.
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <item>
///     <description>`Email` : user's email address.</description>
///   </item>
///   <item>
///     <description>`Password` : hashed password.</description>
///   </item>
///   <item>
///     <description>`Created_at` : account creation date.</description>
///   </item>
///   <item>
///     <description>`Updated_at` : last update date.</description>
///   </item>
/// </list>
/// </remarks>
/// <author>
/// Killian
/// </author>
public class UserAuthModel
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // hashed
    public DateTime Created_at { get; set; } = DateTime.UtcNow;
    public DateTime Updated_at { get; set; } = DateTime.UtcNow;
}