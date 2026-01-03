namespace Kobold.Core.Abstractions.Audio
{
    /// <summary>
    /// Platform-agnostic interface for music playback control
    /// Manages background music playback (typically one track at a time)
    /// </summary>
    public interface IAudioPlayer
    {
        /// <summary>
        /// Currently playing music, or null if none
        /// </summary>
        IMusic? CurrentMusic { get; }

        /// <summary>
        /// Global music volume (0.0 to 1.0)
        /// </summary>
        float MusicVolume { get; set; }

        /// <summary>
        /// Is music currently playing
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        /// Play a music track
        /// </summary>
        /// <param name="music">The music track to play</param>
        /// <param name="loop">Whether to loop the music</param>
        void PlayMusic(IMusic music, bool loop = true);

        /// <summary>
        /// Stop the currently playing music
        /// </summary>
        void StopMusic();

        /// <summary>
        /// Pause the currently playing music
        /// </summary>
        void PauseMusic();

        /// <summary>
        /// Resume paused music
        /// </summary>
        void ResumeMusic();
    }
}
