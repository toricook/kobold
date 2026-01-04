using Kobold.Extensions.Physics.Systems;
using Kobold.Extensions.Physics.Components;
using Kobold.Extensions.Collision.Systems;
using Kobold.Extensions.Collision.Components;
using Kobold.Extensions.Input.Systems;
using Kobold.Extensions.Input.Components;
using Kobold.Extensions.Boundaries.Systems;
using Kobold.Extensions.Boundaries.Components;
using Kobold.Extensions.Triggers.Systems;
using Kobold.Extensions.Destruction.Systems;
using Kobold.Extensions.Destruction.Components;
using Kobold.Extensions.Gameplay.Components;
using Kobold.Extensions.GameState.Systems;
namespace CaveExplorer.Components
{
    /// <summary>
    /// Component for tracking entity health points
    /// </summary>
    public struct Health
    {
        /// <summary>
        /// Current health points
        /// </summary>
        public int CurrentHealth { get; set; }

        /// <summary>
        /// Maximum health points
        /// </summary>
        public int MaxHealth { get; set; }

        /// <summary>
        /// Whether this entity is currently alive
        /// </summary>
        public bool IsAlive => CurrentHealth > 0;

        public Health(int maxHealth)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
        }

        /// <summary>
        /// Heal the entity by the specified amount, capped at MaxHealth
        /// </summary>
        /// <param name="amount">Amount to heal</param>
        /// <returns>Actual amount healed</returns>
        public int Heal(int amount)
        {
            int oldHealth = CurrentHealth;
            CurrentHealth = Math.Min(CurrentHealth + amount, MaxHealth);
            return CurrentHealth - oldHealth;
        }

        /// <summary>
        /// Damage the entity by the specified amount
        /// </summary>
        /// <param name="amount">Amount of damage</param>
        /// <returns>Actual damage dealt</returns>
        public int TakeDamage(int amount)
        {
            int oldHealth = CurrentHealth;
            CurrentHealth = Math.Max(CurrentHealth - amount, 0);
            return oldHealth - CurrentHealth;
        }
    }
}
