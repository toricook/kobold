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
    /// <summary>
    /// System execution order constants
    /// </summary>
    public static class SystemUpdateOrder
    {
        public const int INPUT = 0;
        public const int PHYSICS = 100;
        public const int AI = 200;
        public const int COLLISION = 300;
        public const int GAME_LOGIC = 400;
        public const int UI = 500;
        public const int CLEANUP = 600;
    }

    /// <summary>
    /// Enhanced system manager with ordering and render system support
    /// </summary>
    public class SystemManager
    {
        private readonly List<SystemInfo> _allSystems = new();
        private readonly List<SystemInfo> _gameplaySystems = new();
        private readonly List<SystemInfo> _alwaysUpdateSystems = new();
        private readonly List<IRenderSystem> _renderSystems = new();
        private readonly EventBus _eventBus;
        private readonly World _world;

        public SystemManager(EventBus eventBus, World world)
        {
            _eventBus = eventBus;
            _world = world;
        }

        /// <summary>
        /// Add a regular update system
        /// </summary>
        public void AddSystem(ISystem system, int order = SystemUpdateOrder.GAME_LOGIC, bool requiresGameplayState = true)
        {
            var systemInfo = new SystemInfo(system, order, requiresGameplayState);

            _allSystems.Add(systemInfo);

            if (requiresGameplayState)
            {
                _gameplaySystems.Add(systemInfo);
                _gameplaySystems.Sort((a, b) => a.Order.CompareTo(b.Order));
            }
            else
            {
                _alwaysUpdateSystems.Add(systemInfo);
                _alwaysUpdateSystems.Sort((a, b) => a.Order.CompareTo(b.Order));
            }
        }

        /// <summary>
        /// Add a render system (these are always active)
        /// </summary>
        public void AddRenderSystem(IRenderSystem renderSystem)
        {
            _renderSystems.Add(renderSystem);
        }

        /// <summary>
        /// Add a system that implements both ISystem and IRenderSystem
        /// </summary>
        public void AddHybridSystem(ISystem system, int order = SystemUpdateOrder.GAME_LOGIC, bool requiresGameplayState = true)
        {
            // Add as regular system
            AddSystem(system, order, requiresGameplayState);

            // If it also implements IRenderSystem, add it as render system
            if (system is IRenderSystem renderSystem)
            {
                AddRenderSystem(renderSystem);
            }
        }

        /// <summary>
        /// Remove a system
        /// </summary>
        public void RemoveSystem(ISystem system)
        {
            _allSystems.RemoveAll(s => s.System == system);
            _gameplaySystems.RemoveAll(s => s.System == system);
            _alwaysUpdateSystems.RemoveAll(s => s.System == system);

            if (system is IRenderSystem renderSystem)
            {
                _renderSystems.Remove(renderSystem);
            }
        }

        /// <summary>
        /// Remove a render system
        /// </summary>
        public void RemoveRenderSystem(IRenderSystem renderSystem)
        {
            _renderSystems.Remove(renderSystem);
        }

        /// <summary>
        /// Update all systems in order
        /// </summary>
        public void UpdateAll(float deltaTime)
        {
            // Always update these systems (UI, game state management, etc.)
            foreach (var systemInfo in _alwaysUpdateSystems)
            {
                systemInfo.System.Update(deltaTime);
            }

            // Only update gameplay systems if game is playing
            if (IsGamePlaying())
            {
                foreach (var systemInfo in _gameplaySystems)
                {
                    systemInfo.System.Update(deltaTime);
                }
            }
        }

        /// <summary>
        /// Render all render systems
        /// </summary>
        public void RenderAll()
        {
            foreach (var renderSystem in _renderSystems)
            {
                renderSystem.Render();
            }
        }

        /// <summary>
        /// Get a specific system by type
        /// </summary>
        public T GetSystem<T>() where T : class
        {
            foreach (var systemInfo in _allSystems)
            {
                if (systemInfo.System is T typedSystem)
                {
                    return typedSystem;
                }
            }

            // Also check render systems
            foreach (var renderSystem in _renderSystems)
            {
                if (renderSystem is T typedRenderSystem)
                {
                    return typedRenderSystem;
                }
            }

            return null;
        }

        /// <summary>
        /// Get all systems of a specific type
        /// </summary>
        public IEnumerable<T> GetSystems<T>() where T : class
        {
            var systems = new List<T>();

            foreach (var systemInfo in _allSystems)
            {
                if (systemInfo.System is T typedSystem)
                {
                    systems.Add(typedSystem);
                }
            }

            foreach (var renderSystem in _renderSystems)
            {
                if (renderSystem is T typedRenderSystem)
                {
                    systems.Add(typedRenderSystem);
                }
            }

            return systems;
        }

        /// <summary>
        /// Get system information for debugging
        /// </summary>
        public SystemManagerInfo GetSystemInfo()
        {
            return new SystemManagerInfo
            {
                TotalSystems = _allSystems.Count,
                GameplaySystems = _gameplaySystems.Count,
                AlwaysUpdateSystems = _alwaysUpdateSystems.Count,
                RenderSystems = _renderSystems.Count,
                IsGamePlaying = IsGamePlaying()
            };
        }

        /// <summary>
        /// Clear all systems
        /// </summary>
        public void ClearSystems()
        {
            _allSystems.Clear();
            _gameplaySystems.Clear();
            _alwaysUpdateSystems.Clear();
            _renderSystems.Clear();
        }

        /// <summary>
        /// Enable or disable a system
        /// </summary>
        public void SetSystemEnabled(ISystem system, bool enabled)
        {
            var systemInfo = _allSystems.FirstOrDefault(s => s.System == system);
            if (systemInfo != null)
            {
                systemInfo.IsEnabled = enabled;
            }
        }

        /// <summary>
        /// Check if a system is enabled
        /// </summary>
        public bool IsSystemEnabled(ISystem system)
        {
            var systemInfo = _allSystems.FirstOrDefault(s => s.System == system);
            return systemInfo?.IsEnabled ?? false;
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
    }

    /// <summary>
    /// Information about a system for management
    /// </summary>
    public class SystemInfo
    {
        public ISystem System { get; }
        public int Order { get; }
        public bool RequiresGameplayState { get; }
        public bool IsEnabled { get; set; }

        public SystemInfo(ISystem system, int order, bool requiresGameplayState)
        {
            System = system;
            Order = order;
            RequiresGameplayState = requiresGameplayState;
            IsEnabled = true;
        }
    }

    /// <summary>
    /// Debug information about the system manager
    /// </summary>
    public class SystemManagerInfo
    {
        public int TotalSystems { get; set; }
        public int GameplaySystems { get; set; }
        public int AlwaysUpdateSystems { get; set; }
        public int RenderSystems { get; set; }
        public bool IsGamePlaying { get; set; }

        public override string ToString()
        {
            return $"SystemManager: {TotalSystems} total, {GameplaySystems} gameplay, {AlwaysUpdateSystems} always-update, {RenderSystems} render, Playing: {IsGamePlaying}";
        }
    }
}
