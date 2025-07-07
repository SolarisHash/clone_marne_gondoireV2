#if false
namespace PokemonTeam.Models
{
    /// <summary>
    /// This class represents a Shop in the game.
    /// </summary>
    /// <remarks>
    /// Attributes :
    /// <list type="bullet">
    ///   <item>
    ///     <description><see cref="Id"/> : Unique identifier of the shop</description>
    ///   </item>
    ///   <item>
    ///     <description><see cref="Items"/> : List of available objects in the shop</description>
    ///   </item>
    /// </list>
    /// </remarks>
    /// <author>
    /// Elerig
    /// </author>
    public class Shop
    {
        public int Id { get; set; }

        public List<Objet> Items { get; set; } = new List<Objet>();
    }

}
#endif