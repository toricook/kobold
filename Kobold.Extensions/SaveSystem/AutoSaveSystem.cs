using System;
using System.Collections.Generic;
using Arch.Core;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Events;
using Kobold.Extensions.SaveSystem.Events;

namespace Kobold.Extensions.SaveSystem
{
    /// <summary>
    /// System that automatically saves the game at regular intervals.
    /// Tracks total playtime and triggers saves via SaveManager.
    /// </summary>
    public class AutoSaveSystem : ISystem
    {
        private readonly World _world;
        private readonly SaveManager _saveManager;
        private readonly EventBus _eventBus;
        private float _timeSinceLastSave;
        private float _totalPlaytime;

        /// <summary>
        /// Whether auto-save is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Time interval between auto-saves in seconds.
        /// Default: 300 seconds (5 minutes).
        /// </summary>
        public float AutoSaveInterval { get; set; } = 300f;

        /// <summary>
        /// Name of the save slot to use for auto-saves.
        /// Default: "autosave".
        /// </summary>
        public string AutoSaveSlot { get; set; } = "autosave";

        /// <summary>
        /// Creates a new AutoSaveSystem.
        /// </summary>
        /// <param name="world">ECS World</param>
        /// <param name="saveManager">SaveManager instance</param>
        /// <param name="eventBus">EventBus for publishing events</param>
        public AutoSaveSystem(World world, SaveManager saveManager, EventBus eventBus)
        {
            _world = world;
            _saveManager = saveManager;
            _eventBus = eventBus;
        }

        /// <summary>
        /// Updates the auto-save timer and triggers saves when interval is reached.
        /// </summary>
        /// <param name="deltaTime">Time since last frame in seconds</param>
        public void Update(float deltaTime)
        {
            if (!Enabled) return;

            _totalPlaytime += deltaTime;
            _timeSinceLastSave += deltaTime;

            if (_timeSinceLastSave >= AutoSaveInterval)
            {
                PerformAutoSave();
                _timeSinceLastSave = 0f;
            }
        }

        /// <summary>
        /// Performs an auto-save operation.
        /// </summary>
        private void PerformAutoSave()
        {
            var metadata = new Dictionary<string, string>
            {
                ["playtime"] = _totalPlaytime.ToString("F2"),
                ["autosave"] = "true"
            };

            try
            {
                bool success = _saveManager.Save(AutoSaveSlot, metadata, _totalPlaytime);

                _eventBus.Publish(new AutoSaveCompletedEvent
                {
                    Success = success,
                    ErrorMessage = success ? null : "Auto-save failed (see SaveErrorEvent for details)"
                });
            }
            catch (Exception ex)
            {
                _eventBus.Publish(new AutoSaveCompletedEvent
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets the current total playtime in seconds.
        /// </summary>
        public float GetPlaytime() => _totalPlaytime;

        /// <summary>
        /// Resets the playtime counter to zero.
        /// Useful when starting a new game.
        /// </summary>
        public void ResetPlaytime()
        {
            _totalPlaytime = 0f;
            _timeSinceLastSave = 0f;
        }

        /// <summary>
        /// Sets the playtime to a specific value.
        /// Useful when loading a save file.
        /// </summary>
        /// <param name="playtime">Playtime in seconds</param>
        public void SetPlaytime(float playtime)
        {
            _totalPlaytime = playtime;
        }

        /// <summary>
        /// Forces an immediate auto-save regardless of interval.
        /// </summary>
        public void SaveNow()
        {
            PerformAutoSave();
            _timeSinceLastSave = 0f;
        }
    }
}
