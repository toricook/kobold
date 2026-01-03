namespace Kobold.Core.Abstractions.Audio
{
    /// <summary>
    /// Platform-agnostic interface for a sound effect
    /// </summary>
    public interface ISoundEffect : IDisposable
    {
        /// <summary>
        /// Play the sound effect once at the specified volume
        /// </summary>
        /// <param name="volume">Volume from 0.0 to 1.0</param>
        void Play(float volume = 1.0f);

        /// <summary>
        /// Create an instance for advanced control (looping, pitch, etc.)
        /// </summary>
        ISoundEffectInstance CreateInstance();
    }
}
