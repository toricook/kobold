using Arch.Core;
using Kobold.Core.Components;
using Kobold.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Systems
{
    public class SystemManager
    {
        private readonly List<ISystem> _systems = new();
        private readonly List<ISystem> _gameplaySystems = new();
        private readonly List<ISystem> _alwaysUpdateSystems = new();
        private readonly EventBus _eventBus;
        private readonly World _world;

        public SystemManager(EventBus eventBus, World world)
        {
            _eventBus = eventBus;
            _world = world;
        }

        public void AddSystem(ISystem system, bool requiresGameplayState = true)
        {
            _systems.Add(system);

            if (requiresGameplayState)
            {
                _gameplaySystems.Add(system);
            }
            else
            {
                _alwaysUpdateSystems.Add(system);
            }
        }

        public void RemoveSystem(ISystem system)
        {
            _systems.Remove(system);
            _gameplaySystems.Remove(system);
            _alwaysUpdateSystems.Remove(system);
        }

        public void UpdateAll(float deltaTime)
        {
            // Always update these systems (UI, rendering, game state management)
            foreach (var system in _alwaysUpdateSystems)
            {
                system.Update(deltaTime);
            }

            // Only update gameplay systems if game is playing
            if (IsGamePlaying())
            {
                foreach (var system in _gameplaySystems)
                {
                    system.Update(deltaTime);
                }
            }
        }

        private bool IsGamePlaying()
        {
            var gameStateQuery = new QueryDescription().WithAll<GameState>();
            bool isPlaying = true;

            _world.Query(in gameStateQuery, (ref GameState gameState) =>
            {
                isPlaying = gameState.IsPlaying;
            });

            return isPlaying;
        }

        public void ClearSystems()
        {
            _systems.Clear();
            _gameplaySystems.Clear();
            _alwaysUpdateSystems.Clear();
        }

        public T GetSystem<T>() where T : class, ISystem
        {
            foreach (var system in _systems)
            {
                if (system is T typedSystem)
                {
                    return typedSystem;
                }
            }
            return null;
        }
    }
}
