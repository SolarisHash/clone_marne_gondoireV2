using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PokemonTeam.Data;
using PokemonTeam.Models;

namespace PokemonTeam.Services
{
    public interface ITypeChartService
    {
        /// <summary>
        /// Retourne le multiplicateur de dégâts d’une attaque <paramref name="attackType"/>
        /// appliquée à un ou plusieurs types défenseurs.
        /// </summary>
        Task<decimal> Multiplier(string attackType,
                                               params string[] defenderTypes);
    }

    public class TypeChartService : ITypeChartService
    {
        private readonly PokemonDbContext _ctx;

        // Map « nom du type attaquant » → colonne correspondante
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

        public TypeChartService(PokemonDbContext ctx) => _ctx = ctx;

        public async Task<decimal> Multiplier(string attackType,
                                                            params string[] defenderTypes)
        {
            if (string.IsNullOrWhiteSpace(attackType))
                throw new ArgumentException("Le type d’attaque est requis.", nameof(attackType));

            if (!_col.TryGetValue(attackType, out var selectColumn))
                throw new ArgumentException($"Type d’attaque inconnu : {attackType}", nameof(attackType));

            if (defenderTypes is null || defenderTypes.Length == 0)
                throw new ArgumentException("Au moins un type défenseur est requis.", nameof(defenderTypes));

            var multiplier = 1m;

            foreach (var def in defenderTypes)
            {
                var row = await _ctx.TypeChart
                                    .AsNoTracking()
                                    .SingleOrDefaultAsync(t => t.typeName == def);

                if (row is null)
                    throw new ArgumentException($"Type défenseur inconnu : {def}", nameof(defenderTypes));

                multiplier *= selectColumn(row);
            }

            return multiplier;
        }
    }
}