using System;

namespace Kobold.Core.Events
{
    /// <summary>
    /// Base class for all events in the Kobold event system.
    /// Provides common functionality like automatic timestamping.
    /// </summary>
    /// <remarks>
    /// Inherit from this class when creating custom events to automatically get timestamp tracking.
    /// If you don't need timestamps or want to implement IEvent directly, you can skip this base class.
    ///
    /// Example:
    /// <code>
    /// public class ScoreChangedEvent : BaseEvent
    /// {
    ///     public int OldScore { get; }
    ///     public int NewScore { get; }
    ///     public int Delta => NewScore - OldScore;
    ///
    ///     public ScoreChangedEvent(int oldScore, int newScore)
    ///     {
    ///         OldScore = oldScore;
    ///         NewScore = newScore;
    ///     }
    /// }
    /// </code>
    /// </remarks>
    /// <seealso cref="IEvent"/>
    /// <seealso cref="EventBus"/>
    public abstract class BaseEvent : IEvent
    {
        /// <summary>
        /// The UTC timestamp when this event was created.
        /// Useful for event ordering, debugging, and analytics.
        /// </summary>
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }
}
