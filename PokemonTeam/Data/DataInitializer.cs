using PokemonTeam.Models;

namespace PokemonTeam.Data;

public static class DbInitializer
{
    public static void Initialize(PokemonDbContext context)
    {
        // Vérifie si la table Items est déjà peuplée
        if (context.Items.Any())
        {
            return; // DB déjà seedée
        }

        var items = new Item[]
        {
            new Item { Name = "Potion", Price = 50 },
            new Item { Name = "Super Potion", Price = 100 },
            new Item { Name = "Hyper Potion", Price = 200 },
            new Item { Name = "Max Potion", Price = 500 }
        };

        context.Items.AddRange(items);
        context.SaveChanges();
    }
}