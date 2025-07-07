#if false
using System.Numerics;

using Microsoft.EntityFrameworkCore;


namespace PokemonTeam.Models
{
    /// <summary>
    /// This class represents the Entity Framework database context for the ASP PokemonTeam project.
    /// </summary>
    /// <remarks>
    /// It defines the mapping between C# models and SQL Server tables,
    /// enabling LINQ queries, database migrations, and relational data access.
    /// 
    /// Tables managed:
    /// <list type="bullet">
    ///   <item><description><see cref="UserAuths"/>: Stores login credentials and creation timestamps.</description></item>
    ///   <item><description><see cref="Players"/>: Represents a player account linked to user authentication.</description></item>
    ///   <item><description><see cref="Pokemons"/>: Represents all Pokémon with stats and evolution logic.</description></item>
    ///   <item><description><see cref="Types"/>: Manages type effectiveness multipliers for all elements.</description></item>
    ///   <item><description><see cref="Skills"/>: Contains the attacks and their effects by type.</description></item>
    ///   <item><description><see cref="Objects"/>: Game items like potions, buffs, etc.</description></item>
    ///   <item><description><see cref="Logs"/>: Historical logs of actions or combat events.</description></item>
    ///   <item><description><see cref="PlayerPokemons"/>: Many-to-many linking between players and their pokémons.</description></item>
    ///   <item><description><see cref="PokemonSkills"/>: Skills learned by each Pokémon.</description></item>
    ///   <item><description><see cref="PokemonTypes"/>: Types assigned to each Pokémon.</description></item>
    ///   <item><description><see cref="PlayerObjects"/>: Items possessed by players.</description></item>
    /// </list>
    /// </remarks>
    /// <author>
    /// Elerig
    /// </author>
    public class ArenaContext : DbContext
    {
        public ArenaContext(DbContextOptions<ArenaContext> options) : base(options) { }

        public DbSet<UserAuth> UserAuths { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Pokemon> Pokemons { get; set; }
        public DbSet<Type> Types { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<Object> Objects { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<PlayerPokemon> PlayerPokemons { get; set; }
        public DbSet<PokemonSkill> PokemonSkills { get; set; }
        public DbSet<PokemonType> PokemonTypes { get; set; }
        public DbSet<PlayerObject> PlayerObjects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Définition des relations Many-to-Many
            modelBuilder.Entity<PlayerPokemon>()
                .HasKey(pp => new { pp.FkPlayer, pp.FkPokemon });

            modelBuilder.Entity<PokemonSkill>()
                .HasKey(ps => new { ps.FkPokemon, ps.FkSkill });

            modelBuilder.Entity<PokemonType>()
                .HasKey(pt => new { pt.FkPokemon, pt.FkType });

            modelBuilder.Entity<PlayerObject>()
                .HasKey(po => new { po.FkPlayer, po.FkObject });
        }
    }

}
#endif