namespace Kobold.Core.Abstractions.Audio
{
    /// <summary>
    /// Represents the playback state of a sound
    /// </summary>
    public enum SoundState
    {
        /// <summary>Sound is currently playing</summary>
        Playing,
        /// <summary>Sound is paused and can be resumed</summary>
        Paused,
        /// <summary>Sound is stopped</summary>
        Stopped
    }

    /// <summary>
    /// Platform-agnostic interface for a controllable sound effect instance
    /// </summary>
    public interface ISoundEffectInstance : IDisposable
    {
        /// <summary>
        /// Volume of this sound instance (0.0 to 1.0)
        /// </summary>
        float Volume { get; set; }

        /// <summary>
        /// Whether this sound instance should loop
        /// </summary>
        bool IsLooped { get; set; }

        /// <summary>
        /// Current playback state
        /// </summary>
        SoundState State { get; }

        /// <summary>
        /// Play the sound from the beginning
        /// </summary>
        void Play();

        /// <summary>
        /// Pause playback
        /// </summary>
        void Pause();

        /// <summary>
        /// Resume playback from where it was paused
        /// </summary>
        void Resume();

        /// <summary>
        /// Stop playback and reset to the beginning
        /// </summary>
        void Stop();
    }
}
