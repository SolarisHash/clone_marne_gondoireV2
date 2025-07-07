using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using PokemonTeam.Data;
using PokemonTeam.Exceptions;

namespace PokemonTeam.Models
{
    /// <summary>
    /// The Skill class represents a skill.
    /// </summary>
    /// <remarks>
    /// Attributes:
    /// <list type="bullet">
    ///   <item>
    ///     <description><see cref="Id"/>: The unique identifier of the skill.</description>
    ///   </item>
    ///   <item>
    ///     <description><see cref="Name"/>: The name of the skill (string).</description>
    ///   </item>
    ///   <item>
    ///     <description><see cref="fk_type"/>: The foreign key to the type table.</description>
    ///   </item>
    ///   <item>
    ///     <description><see cref="Damage"/>: The damage inflicted (short).</description>
    ///   </item>
    ///   <item>
    ///     <description><see cref="PowerPoints"/>: The number of power points (short).</description>
    ///   </item>
    ///   <item>
    ///     <description><see cref="Accuracy"/>: The accuracy of the skill (short).</description>
    ///   </item>
    /// </list>
    /// </remarks>
    /// <author>
    /// Adam
    /// </author>
    public class Skill
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; private set; } = string.Empty;

        [Required]
        [Column("fk_type")]
        public int fk_type { get; private set; }

        [Required]
        public short Damage { get; private set; }

        [Required]
        [Column("power_point")]
        public short PowerPoints { get; set; }

        [Required]
        public short Accuracy { get; private set; }

        // Propriété de navigation vers TypeChart
        [ForeignKey("fk_type")]
        public virtual TypeChart? TypeChart { get; set; }

        // Propriété calculée pour obtenir le nom du type
        [NotMapped]
        public string Type => TypeChart?.typeName ?? "Unknown";

        // Constructeur sans paramètres pour EF
        protected Skill() { }

        public Skill(string name, int typeId, short damage, short powerPoints, short accuracy)
        {
            Name = name;
            fk_type = typeId;
            Damage = damage;
            PowerPoints = powerPoints;
            Accuracy = accuracy;
        }
        
        /// <summary>
        /// Utilise la compétence et réduit ses points de pouvoir.
        /// </summary>
        /// <exception cref="NotEnoughPowerPointsException">Lancée si la compétence n'a plus de PP.</exception>
        /// <returns>Vrai si la compétence a été utilisée avec succès.</returns>
        public bool Use()
        {
            if (PowerPoints <= 0)
            {
                throw new NotEnoughPowerPointsException($"La compétence {Name} n'a plus de points de pouvoir.");
            }
            
            PowerPoints--;
            return true;
        }
        
        /// <summary>
        /// Calcule les dégâts infligés par cette compétence.
        /// </summary>
        /// <param name="attackerStrength">Force de l'attaquant.</param>
        /// <param name="targetDefense">Défense de la cible.</param>
        /// <param name="typeMultiplier">Multiplicateur de type.</param>
        /// <returns>Dégâts calculés.</returns>
        public int CalculateDamage(int attackerStrength, int targetDefense, decimal typeMultiplier)
        {
            // Formule de calcul des dégâts : (dégâts de base * (force de l'attaquant / défense de la cible)) * multiplicateur de type
            double damage = (Damage * (attackerStrength / (double)targetDefense)) * (double)typeMultiplier;
            
            // Les dégâts minimum sont de 1
            return Math.Max(1, (int)damage);
        }
        
        /// <summary>
        /// Détermine si l'attaque touche sa cible en fonction de la précision.
        /// </summary>
        /// <returns>Vrai si l'attaque touche.</returns>
        public bool HitsTarget()
        {
            if (Accuracy >= 100)
                return true;
                
            Random random = new Random();
            int roll = random.Next(1, 101); // Génère un nombre entre 1 et 100
            
            return roll <= Accuracy;
        }
        
        /// <summary>
        /// Restaure les points de pouvoir de la compétence.
        /// </summary>
        /// <param name="amount">Quantité de PP à restaurer. Si null, restaure au maximum.</param>
        /// <returns>Nombre de PP après restauration.</returns>
        public short RestorePowerPoints(short? amount = null)
        {
            // Si amount est null, on restaure au maximum (valeur par défaut pour un Éther max)
            short maxPP = 10; // Valeur par défaut pour un PP Max
            
            if (amount.HasValue)
            {
                PowerPoints = (short)Math.Min(maxPP, PowerPoints + amount.Value);
            }
            else
            {
                PowerPoints = maxPP;
            }
            
            return PowerPoints;
        }
        
        #region Méthodes statiques d'accès aux données
        
        /// <summary>
        /// Récupère toutes les compétences de la base de données.
        /// </summary>
        /// <param name="context">Contexte de base de données.</param>
        /// <returns>Liste de toutes les compétences.</returns>
        public static async Task<IEnumerable<Skill>> GetAllAsync(PokemonDbContext context)
        {
            return await context.Skills
                .Include(s => s.TypeChart)
                .ToListAsync();
        }
        
        /// <summary>
        /// Récupère une compétence par son identifiant.
        /// </summary>
        /// <param name="id">Identifiant de la compétence.</param>
        /// <param name="context">Contexte de base de données.</param>
        /// <returns>La compétence si trouvée, sinon null.</returns>
        public static async Task<Skill?> GetByIdAsync(int id, PokemonDbContext context)
        {
            return await context.Skills
                .Include(s => s.TypeChart)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        
        /// <summary>
        /// Crée une nouvelle compétence dans la base de données.
        /// </summary>
        /// <param name="skill">La compétence à créer.</param>
        /// <param name="context">Contexte de base de données.</param>
        /// <returns>La compétence créée.</returns>
        public static async Task<Skill> CreateAsync(Skill skill, PokemonDbContext context)
        {
            context.Skills.Add(skill);
            await context.SaveChangesAsync();
            return skill;
        }
        
        /// <summary>
        /// Met à jour une compétence existante.
        /// </summary>
        /// <param name="id">Identifiant de la compétence.</param>
        /// <param name="skill">Nouvelles données de la compétence.</param>
        /// <param name="context">Contexte de base de données.</param>
        /// <returns>True si la mise à jour a réussi, false sinon.</returns>
        public static async Task<bool> UpdateAsync(int id, Skill skill, PokemonDbContext context)
        {
            if (id != skill.Id)
            {
                return false;
            }
            
            context.Entry(skill).State = EntityState.Modified;
            
            try
            {
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ExistsAsync(id, context))
                {
                    return false;
                }
                throw;
            }
        }
        
        /// <summary>
        /// Supprime une compétence de la base de données.
        /// </summary>
        /// <param name="id">Identifiant de la compétence à supprimer.</param>
        /// <param name="context">Contexte de base de données.</param>
        /// <returns>True si la suppression a réussi, false sinon.</returns>
        public static async Task<bool> DeleteAsync(int id, PokemonDbContext context)
        {
            var skill = await context.Skills.FindAsync(id);
            if (skill == null)
            {
                return false;
            }
            
            context.Skills.Remove(skill);
            await context.SaveChangesAsync();
            return true;
        }
        
        /// <summary>
        /// Vérifie si une compétence existe dans la base de données.
        /// </summary>
        /// <param name="id">Identifiant de la compétence.</param>
        /// <param name="context">Contexte de base de données.</param>
        /// <returns>True si la compétence existe, false sinon.</returns>
        public static async Task<bool> ExistsAsync(int id, PokemonDbContext context)
        {
            return await context.Skills.AnyAsync(s => s.Id == id);
        }
        
        #endregion
        
        #region Méthodes de combat
        
        /// <summary>
        /// Utilise la compétence en combat entre deux Pokémon.
        /// </summary>
        /// <param name="attacker">Le Pokémon attaquant.</param>
        /// <param name="target">Le Pokémon cible.</param>
        /// <param name="typeChartService">Service pour obtenir les multiplicateurs de type.</param>
        /// <returns>Le résultat de l'utilisation de la compétence.</returns>
        /// <exception cref="NotEnoughPowerPointsException">Si la compétence n'a plus de PP.</exception>
        /// <exception cref="ArgumentNullException">Si un des paramètres est nul.</exception>
        public async Task<UseSkillResponse> UseInBattle(Pokemon attacker, Pokemon target, PokemonTeam.Services.ITypeChartService typeChartService)
        {
            if (attacker == null)
                throw new ArgumentNullException(nameof(attacker), "L'attaquant ne peut pas être nul.");
                
            if (target == null)
                throw new ArgumentNullException(nameof(target), "La cible ne peut pas être nulle.");
                
            if (typeChartService == null)
                throw new ArgumentNullException(nameof(typeChartService), "Le service de type ne peut pas être nul.");
            
            // Utilisation de la compétence (réduit les PP)
            this.Use();
            
            // Déterminer si l'attaque touche sa cible
            if (!this.HitsTarget())
            {
                // L'attaque a manqué sa cible
                return new UseSkillResponse(0, target);
            }
            
            // Utiliser le nom du type depuis la propriété calculée
            string skillType = this.Type.ToLowerInvariant();
            
            // Calculer le multiplicateur de type basé sur les types de la cible
            decimal typeMultiplier = await typeChartService.Multiplier(skillType, target.types.ToArray());
            
            // Calculer les dégâts
            int damageDealt = this.CalculateDamage(
                attacker.strength,
                target.defense,
                typeMultiplier
            );
            
            // Appliquer les dégâts à la cible
            target.healthPoint = (short)Math.Max(0, target.healthPoint - damageDealt);
            
            // Retourner la réponse
            return new UseSkillResponse(damageDealt, target);
        }
        
        #endregion
    }
}