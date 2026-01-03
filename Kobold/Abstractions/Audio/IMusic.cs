namespace Kobold.Core.Abstractions.Audio
{
    /// <summary>
    /// Platform-agnostic interface for a music track
    /// </summary>
    public interface IMusic : IDisposable
    {
        /// <summary>
        /// Name of the music track
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Duration of the music track
        /// </summary>
        TimeSpan Duration { get; }
    }
}
