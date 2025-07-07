using Microsoft.EntityFrameworkCore;
using PokemonTeam.Data;

namespace PokemonTeam.Models;

/// <summary>
/// This class represents a type chart with the following attributes :
/// </summary>
/// <remarks>
/// Attributes :
/// - Id: the unique identifier for the type chart (int)
/// - TypeName: the name of the type (string)
/// - Fire: the effectiveness against Fire type (decimal)
/// - Water: the effectiveness against Water type (decimal)
/// - Grass: the effectiveness against Grass type (decimal)
/// - Electric: the effectiveness against Electric type (decimal)
/// - Ice: the effectiveness against Ice type (decimal)
/// - Fighting: the effectiveness against Fighting type (decimal)
/// - Poison: the effectiveness against Poison type (decimal)
/// - Ground: the effectiveness against Ground type (decimal)
/// - Flying: the effectiveness against Flying type (decimal)
/// - Psychic: the effectiveness against Psychic type (decimal)
/// - Bug: the effectiveness against Bug type (decimal)
/// - Rock: the effectiveness against Rock type (decimal)
/// - Ghost: the effectiveness against Ghost type (decimal)
/// - Dragon: the effectiveness against Dragon type (decimal)
/// - Dark: the effectiveness against Dark type (decimal)
/// - Steel: the effectiveness against Steel type (decimal)
/// - Fairy: the effectiveness against Fairy type (decimal)
/// </remarks>
/// <author>
/// Mael
/// </author>
public class TypeChart
{
    
    public int id { get; private set; }
    public string typeName { get; private set; }
    
    public decimal fire { get; private set; }
    public decimal water { get; private set; }
    public decimal grass { get; private set; }
    public decimal electric { get; private set; }
    public decimal ice { get; private set; }
    public decimal fighting { get; private set; }
    public decimal poison { get; private set; }
    public decimal ground { get; private set; }
    public decimal flying { get; private set; }
    public decimal psychic { get; private set; }
    public decimal bug { get; private set; }
    public decimal rock { get; private set; }
    public decimal ghost { get; private set; }
    public decimal dragon { get; private set; }
    public decimal dark { get; private set; }
    public decimal steel { get; private set; }
    public decimal fairy { get; private set; }
    
    
     private static readonly IReadOnlyDictionary<string, Func<TypeChart, decimal>> _col =
            new Dictionary<string, Func<TypeChart, decimal>>(StringComparer.OrdinalIgnoreCase)
        {
            ["fire"]     = r => r.fire,
            ["water"]    = r => r.water,
            ["grass"]    = r => r.grass,
            ["electric"] = r => r.electric,
            ["ice"]      = r => r.ice,
            ["fighting"] = r => r.fighting,
            ["poison"]   = r => r.poison,
            ["ground"]   = r => r.ground,
            ["flying"]   = r => r.flying,
            ["psychic"]  = r => r.psychic,
            ["bug"]      = r => r.bug,
            ["rock"]     = r => r.rock,
            ["ghost"]    = r => r.ghost,
            ["dragon"]   = r => r.dragon,
            ["dark"]     = r => r.dark,
            ["steel"]    = r => r.steel,
            ["fairy"]    = r => r.fairy
        };

        /// <summary>
        /// Calcule le multiplicateur d’une attaque sur un ou plusieurs types défenseurs.
        /// </summary>
        /// <param name="ctx">DbContext pour récupérer les lignes de la table.</param>
        public static async Task<double> GetDamageMultiplierAsync(
            PokemonDbContext ctx,
            string attackType,
            params string[] defenderTypes)
        {
            if (string.IsNullOrWhiteSpace(attackType))
                throw new ArgumentException("Le type d’attaque est requis.", nameof(attackType));

            if (!_col.TryGetValue(attackType, out var selectColumn))
                throw new ArgumentException($"Type d’attaque inconnu : {attackType}", nameof(attackType));

            if (defenderTypes is null || defenderTypes.Length == 0)
                throw new ArgumentException("Au moins un type défenseur est requis.", nameof(defenderTypes));

            double multiplier = 1.0;

            foreach (var def in defenderTypes)
            {
                var row = await ctx.Set<TypeChart>()       // pas besoin de DbSet nommé
                                   .AsNoTracking()
                                   .SingleOrDefaultAsync(t => t.typeName == def);

                if (row is null)
                    throw new ArgumentException($"Type défenseur inconnu : {def}", nameof(defenderTypes));

                multiplier *= (double)selectColumn(row);
            }

            return multiplier;
        }
    }
