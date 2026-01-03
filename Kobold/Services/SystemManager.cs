using Arch.Core;
using Kobold.Core.Abstractions.Engine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kobold.Core.Services
{
    /// <summary>
    /// System execution order constants for deterministic behavior
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
    /// Predefined system groups for common game state scenarios.
    /// Systems can belong to multiple groups using flags.
    /// </summary>
    [Flags]
    public enum SystemGroup
    {
        None = 0,
        /// <summary>Always runs regardless of game state</summary>
        Always = 1 << 0,
        /// <summary>Runs during active gameplay</summary>
        Playing = 1 << 1,
        /// <summary>Runs when game is paused</summary>
        Paused = 1 << 2,
        /// <summary>Runs in menus</summary>
        Menu = 1 << 3,
        /// <summary>Runs during loading</summary>
        Loading = 1 << 4,
        /// <summary>Runs when game is over</summary>
        GameOver = 1 << 5,

        /// <summary>Convenience: Runs during all gameplay states (Playing, Paused, GameOver)</summary>
        AnyGameplay = Playing | Paused | GameOver,
        /// <summary>Convenience: Runs in all states</summary>
        All = Always | Playing | Paused | Menu | Loading | GameOver
    }

    /// <summary>
    /// Delegate for handling system errors during execution
    /// </summary>
    /// <param name="system">The system that threw the exception</param>
    /// <param name="exception">The exception that was thrown</param>
    /// <param name="context">Context about what was happening (e.g., "Update", "Render")</param>
    public delegate void SystemErrorHandler(ISystem system, Exception exception, string context);

    /// <summary>
    /// Enhanced system manager with ordering, groups, error handling, and optimized lookups.
    /// Manages the lifecycle and execution of ECS systems.
    /// </summary>
    public class SystemManager
    {
        private readonly List<SystemInfo> _systems = new();
        private readonly List<IRenderSystem> _renderSystems = new();
        private readonly Dictionary<Type, List<ISystem>> _systemsByType = new();

        private bool _needsSort = false;
        private SystemErrorHandler _errorHandler;

        /// <summary>
        /// Gets or sets the error handler for system exceptions.
        /// If null, exceptions will propagate and crash the game loop.
        /// </summary>
        public SystemErrorHandler ErrorHandler
        {
            get => _errorHandler;
            set => _errorHandler = value;
        }

        /// <summary>
        /// Gets the total number of registered update systems
        /// </summary>
        public int SystemCount => _systems.Count;

        /// <summary>
        /// Gets the total number of registered render systems
        /// </summary>
        public int RenderSystemCount => _renderSystems.Count;

        /// <summary>
        /// Creates a new SystemManager with optional error handling
        /// </summary>
        /// <param name="errorHandler">Optional error handler for system exceptions</param>
        public SystemManager(SystemErrorHandler errorHandler = null)
        {
            _errorHandler = errorHandler;
        }

        /// <summary>
        /// Add a system with ordering and group membership.
        /// Systems are not sorted until the first Update/Render call for performance.
        /// </summary>
        /// <param name="system">The system to add</param>
        /// <param name="order">Execution order (lower executes first)</param>
        /// <param name="groups">Which groups this system belongs to (defaults to Always)</param>
        public void AddSystem(ISystem system, int order = SystemUpdateOrder.GAME_LOGIC, SystemGroup groups = SystemGroup.Always)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));

            var systemInfo = new SystemInfo(system, order, groups);
            _systems.Add(systemInfo);
            _needsSort = true;

            // Add to type lookup cache
            var type = system.GetType();
            if (!_systemsByType.ContainsKey(type))
                _systemsByType[type] = new List<ISystem>();
            _systemsByType[type].Add(system);

            // Also cache by interfaces
            foreach (var interfaceType in type.GetInterfaces().Where(t => typeof(ISystem).IsAssignableFrom(t)))
            {
                if (!_systemsByType.ContainsKey(interfaceType))
                    _systemsByType[interfaceType] = new List<ISystem>();
                _systemsByType[interfaceType].Add(system);
            }
        }

        /// <summary>
        /// Add a system with ordering (backward compatibility overload).
        /// Legacy method that uses the old boolean parameter for gameplay state requirement.
        /// </summary>
        /// <param name="system">The system to add</param>
        /// <param name="order">Execution order (lower executes first)</param>
        /// <param name="requiresGameplayState">If true, system runs during Playing group; if false, runs Always</param>
        [Obsolete("Use AddSystem(ISystem, int, SystemGroup) instead. This overload is for backward compatibility.")]
        public void AddSystem(ISystem system, int order, bool requiresGameplayState)
        {
            var groups = requiresGameplayState ? SystemGroup.Playing : SystemGroup.Always;
            AddSystem(system, order, groups);
        }

        /// <summary>
        /// Add a render system (these always run during the render phase)
        /// </summary>
        public void AddRenderSystem(IRenderSystem renderSystem)
        {
            if (renderSystem == null)
                throw new ArgumentNullException(nameof(renderSystem));

            _renderSystems.Add(renderSystem);
        }

        /// <summary>
        /// Add a hybrid system that implements both ISystem and IRenderSystem
        /// </summary>
        public void AddHybridSystem(ISystem system, int order = SystemUpdateOrder.GAME_LOGIC, SystemGroup groups = SystemGroup.Always)
        {
            AddSystem(system, order, groups);

            if (system is IRenderSystem renderSystem)
            {
                AddRenderSystem(renderSystem);
            }
        }

        /// <summary>
        /// Add a hybrid system (backward compatibility overload)
        /// </summary>
        [Obsolete("Use AddHybridSystem(ISystem, int, SystemGroup) instead. This overload is for backward compatibility.")]
        public void AddHybridSystem(ISystem system, int order, bool requiresGameplayState)
        {
            var groups = requiresGameplayState ? SystemGroup.Playing : SystemGroup.Always;
            AddHybridSystem(system, order, groups);
        }

        /// <summary>
        /// Remove a system from all tracking
        /// </summary>
        public void RemoveSystem(ISystem system)
        {
            if (system == null)
                return;

            _systems.RemoveAll(s => s.System == system);

            // Remove from type cache
            var type = system.GetType();
            if (_systemsByType.TryGetValue(type, out var systems))
            {
                systems.Remove(system);
                if (systems.Count == 0)
                    _systemsByType.Remove(type);
            }

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
            if (renderSystem == null)
                return;

            _renderSystems.Remove(renderSystem);
        }

        /// <summary>
        /// Update all systems that belong to the specified group(s), in order.
        /// Only enabled systems will be updated.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last update</param>
        /// <param name="activeGroups">Which groups should be updated (defaults to Always)</param>
        public void UpdateAll(float deltaTime, SystemGroup activeGroups = SystemGroup.Always)
        {
            EnsureSorted();

            foreach (var systemInfo in _systems)
            {
                // Skip if system is disabled
                if (!systemInfo.IsEnabled)
                    continue;

                // Skip if system doesn't belong to any of the active groups
                if ((systemInfo.Groups & activeGroups) == 0)
                    continue;

                try
                {
                    systemInfo.System.Update(deltaTime);
                }
                catch (Exception ex)
                {
                    HandleSystemError(systemInfo.System, ex, "Update");
                }
            }
        }

        /// <summary>
        /// Render all render systems in registration order
        /// </summary>
        public void RenderAll()
        {
            foreach (var renderSystem in _renderSystems)
            {
                try
                {
                    renderSystem.Render();
                }
                catch (Exception ex)
                {
                    HandleSystemError(renderSystem as ISystem, ex, "Render");
                }
            }
        }

        /// <summary>
        /// Get a specific system by type. Uses O(1) dictionary lookup.
        /// Returns the first system of the specified type, or null if not found.
        /// </summary>
        public T GetSystem<T>() where T : class
        {
            var type = typeof(T);
            if (_systemsByType.TryGetValue(type, out var systems) && systems.Count > 0)
            {
                return systems[0] as T;
            }

            // Also check render systems if T is IRenderSystem
            if (typeof(IRenderSystem).IsAssignableFrom(type))
            {
                foreach (var renderSystem in _renderSystems)
                {
                    if (renderSystem is T typedSystem)
                        return typedSystem;
                }
            }

            return null;
        }

        /// <summary>
        /// Get all systems of a specific type. Uses O(1) dictionary lookup.
        /// </summary>
        public IEnumerable<T> GetSystems<T>() where T : class
        {
            var result = new List<T>();
            var type = typeof(T);

            if (_systemsByType.TryGetValue(type, out var systems))
            {
                foreach (var system in systems)
                {
                    if (system is T typedSystem)
                        result.Add(typedSystem);
                }
            }

            // Also check render systems if T is IRenderSystem
            if (typeof(IRenderSystem).IsAssignableFrom(type))
            {
                foreach (var renderSystem in _renderSystems)
                {
                    if (renderSystem is T typedSystem && !result.Contains(typedSystem))
                        result.Add(typedSystem);
                }
            }

            return result;
        }

        /// <summary>
        /// Enable or disable a system. Disabled systems will not be updated.
        /// </summary>
        public void SetSystemEnabled(ISystem system, bool enabled)
        {
            var systemInfo = _systems.FirstOrDefault(s => s.System == system);
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
            var systemInfo = _systems.FirstOrDefault(s => s.System == system);
            return systemInfo?.IsEnabled ?? false;
        }

        /// <summary>
        /// Clear all systems
        /// </summary>
        public void ClearSystems()
        {
            _systems.Clear();
            _renderSystems.Clear();
            _systemsByType.Clear();
            _needsSort = false;
        }

        /// <summary>
        /// Get detailed information about the current state of the system manager
        /// </summary>
        public SystemManagerInfo GetSystemInfo()
        {
            var info = new SystemManagerInfo
            {
                TotalSystems = _systems.Count,
                RenderSystems = _renderSystems.Count,
                EnabledSystems = _systems.Count(s => s.IsEnabled),
                DisabledSystems = _systems.Count(s => !s.IsEnabled),
                HasErrorHandler = _errorHandler != null
            };

            // Count systems by group
            foreach (var systemInfo in _systems)
            {
                if ((systemInfo.Groups & SystemGroup.Always) != 0)
                    info.AlwaysUpdateSystems++;
                if ((systemInfo.Groups & SystemGroup.Playing) != 0)
                    info.PlayingSystems++;
                if ((systemInfo.Groups & SystemGroup.Paused) != 0)
                    info.PausedSystems++;
                if ((systemInfo.Groups & SystemGroup.Menu) != 0)
                    info.MenuSystems++;
            }

            return info;
        }

        /// <summary>
        /// Sort systems by execution order if needed (lazy sorting for performance)
        /// </summary>
        private void EnsureSorted()
        {
            if (_needsSort)
            {
                _systems.Sort((a, b) => a.Order.CompareTo(b.Order));
                _needsSort = false;
            }
        }

        /// <summary>
        /// Handle a system error by delegating to the error handler or re-throwing
        /// </summary>
        private void HandleSystemError(ISystem system, Exception exception, string context)
        {
            if (_errorHandler != null)
            {
                _errorHandler(system, exception, context);
            }
            else
            {
                // No error handler - re-throw the exception
                throw new SystemExecutionException($"System {system?.GetType().Name} threw an exception during {context}", exception);
            }
        }
    }

    /// <summary>
    /// Information about a registered system
    /// </summary>
    public class SystemInfo
    {
        public ISystem System { get; }
        public int Order { get; }
        public SystemGroup Groups { get; }
        public bool IsEnabled { get; set; }

        public SystemInfo(ISystem system, int order, SystemGroup groups)
        {
            System = system ?? throw new ArgumentNullException(nameof(system));
            Order = order;
            Groups = groups;
            IsEnabled = true;
        }
    }

    /// <summary>
    /// Detailed information about the system manager state for debugging
    /// </summary>
    public class SystemManagerInfo
    {
        public int TotalSystems { get; set; }
        public int RenderSystems { get; set; }
        public int EnabledSystems { get; set; }
        public int DisabledSystems { get; set; }
        public int AlwaysUpdateSystems { get; set; }
        public int PlayingSystems { get; set; }
        public int PausedSystems { get; set; }
        public int MenuSystems { get; set; }
        public bool HasErrorHandler { get; set; }

        public override string ToString()
        {
            return $"SystemManager: {TotalSystems} total ({EnabledSystems} enabled, {DisabledSystems} disabled), " +
                   $"{RenderSystems} render, ErrorHandler: {HasErrorHandler}";
        }
    }

    /// <summary>
    /// Exception thrown when a system encounters an error during execution
    /// </summary>
    public class SystemExecutionException : Exception
    {
        public SystemExecutionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
