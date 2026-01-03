using System;
using Arch.Core;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Abstractions.Input;
using Kobold.Core.Components;
using Kobold.Core.Events;
using Kobold.Extensions.Combat.Components;
using Kobold.Extensions.Combat.Events;

namespace Kobold.Extensions.Combat.Systems
{
    /// <summary>
    /// System that detects combat input and publishes attack request events.
    /// Runs early in the update cycle (INPUT phase).
    ///
    /// For player-controlled entities, listens for attack key presses.
    /// Can be extended to support AI-controlled attacks via a separate component.
    /// </summary>
    public class CombatInputSystem : ISystem
    {
        private readonly World _world;
        private readonly IInputManager _inputManager;
        private readonly EventBus _eventBus;

        public CombatInputSystem(World world, IInputManager inputManager, EventBus eventBus)
        {
            _world = world;
            _inputManager = inputManager;
            _eventBus = eventBus;
        }

        public void Update(float deltaTime)
        {
            // Query entities that have a melee weapon and are player-controlled
            var query = new QueryDescription()
                .WithAll<Transform, MeleeWeaponComponent, Player>()
                .WithNone<AttackCooldownComponent>(); // Can only attack if not on cooldown

            _world.Query(in query, (Entity entity, ref Transform transform, ref MeleeWeaponComponent weapon) =>
            {
                // Check if attack key was pressed THIS frame
                if (_inputManager.IsKeyPressed(weapon.AttackKey))
                {
                    // Publish attack request event
                    _eventBus.Publish(new AttackRequestedEvent(
                        entity,
                        transform.Position,
                        weapon.AttackRadius,
                        weapon.Damage,
                        weapon.TargetTag,
                        weapon.CanDamageSelf
                    ));

                    Console.WriteLine($"Player attack requested at {transform.Position}");
                }
            });
        }
    }
}
