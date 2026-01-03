using Arch.Core;
using Kobold.Core.Abstractions;
using Kobold.Core.Components;
using Kobold.Core.Events;

namespace Kobold.Extensions.GameBases
{
    /// <summary>
    /// System that detects when entities (especially the player) fall below the screen
    /// and triggers death/respawn events.
    /// </summary>
    public class DeathZoneSystem : ISystem
    {
        private readonly World _world;
        private readonly EventBus? _eventBus;
        private readonly float _deathZoneY;
        private bool _deathEventPublished = false;

        public DeathZoneSystem(World world, float deathZoneY, EventBus? eventBus = null)
        {
            _world = world;
            _deathZoneY = deathZoneY;
            _eventBus = eventBus;
        }

        public void Update(float deltaTime)
        {
            // Check player entities
            var playerQuery = new QueryDescription().WithAll<Player, Transform>();

            bool playerExists = false;
            _world.Query(in playerQuery, (Entity entity, ref Transform transform) =>
            {
                playerExists = true;

                // Reset the flag if player is back above the death zone (respawned)
                if (transform.Position.Y <= _deathZoneY && _deathEventPublished)
                {
                    _deathEventPublished = false;
                }

                // Check if player has fallen below the death zone
                if (transform.Position.Y > _deathZoneY && !_deathEventPublished)
                {
                    // Publish player death event (only once)
                    _eventBus?.Publish(new PlayerDeathEvent(entity, PlayerDeathReason.FellOffMap));
                    _deathEventPublished = true;
                }
            });

            // Reset the flag when player no longer exists (during World.Clear)
            if (!playerExists)
            {
                _deathEventPublished = false;
            }
        }
    }

    /// <summary>
    /// Event published when the player dies.
    /// </summary>
    public class PlayerDeathEvent : BaseEvent
    {
        public Entity Player { get; }
        public PlayerDeathReason Reason { get; }

        public PlayerDeathEvent(Entity player, PlayerDeathReason reason)
        {
            Player = player;
            Reason = reason;
        }
    }

    /// <summary>
    /// Reasons why a player might die.
    /// </summary>
    public enum PlayerDeathReason
    {
        FellOffMap,
        EnemyCollision,
        Hazard,
        Other
    }
}
