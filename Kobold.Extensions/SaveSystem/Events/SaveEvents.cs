using Kobold.Core.Events;

namespace Kobold.Extensions.SaveSystem.Events
{
    /// <summary>
    /// Event published when a save operation completes successfully.
    /// </summary>
    public class SaveCompletedEvent : IEvent
    {
        /// <summary>
        /// Name of the save slot that was saved.
        /// </summary>
        public string SlotName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Event published when a load operation completes successfully.
    /// </summary>
    public class LoadCompletedEvent : IEvent
    {
        /// <summary>
        /// Name of the save slot that was loaded.
        /// </summary>
        public string SlotName { get; set; } = string.Empty;

        /// <summary>
        /// Metadata from the loaded save file.
        /// </summary>
        public SaveMetadata? Metadata { get; set; }
    }

    /// <summary>
    /// Event published when an auto-save operation completes.
    /// </summary>
    public class AutoSaveCompletedEvent : IEvent
    {
        /// <summary>
        /// Whether the auto-save succeeded.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Optional error message if auto-save failed.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Event published when a save or load operation encounters an error.
    /// </summary>
    public class SaveErrorEvent : IEvent
    {
        /// <summary>
        /// Error message describing what went wrong.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Name of the save slot involved (if applicable).
        /// </summary>
        public string SlotName { get; set; } = string.Empty;
    }
}
