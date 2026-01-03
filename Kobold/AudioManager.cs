using Kobold.Core.Abstractions.Audio;

namespace Kobold.Core
{
    /// <summary>
    /// Manages audio playback for sound effects and music
    /// Provides a simplified API on top of the asset management system
    /// </summary>
    public class AudioManager
    {
        private readonly AssetManager _assetManager;
        private readonly IAudioPlayer _audioPlayer;
        private float _masterVolume = 1.0f;
        private float _soundEffectVolume = 1.0f;

        public AudioManager(AssetManager assetManager, IAudioPlayer audioPlayer)
        {
            _assetManager = assetManager;
            _audioPlayer = audioPlayer;
        }

        // ===== VOLUME CONTROL =====

        /// <summary>
        /// Master volume for all audio (0.0 to 1.0)
        /// </summary>
        public float MasterVolume
        {
            get => _masterVolume;
            set => _masterVolume = Math.Clamp(value, 0.0f, 1.0f);
        }

        /// <summary>
        /// Volume multiplier for sound effects (0.0 to 1.0)
        /// </summary>
        public float SoundEffectVolume
        {
            get => _soundEffectVolume;
            set => _soundEffectVolume = Math.Clamp(value, 0.0f, 1.0f);
        }

        /// <summary>
        /// Music volume (0.0 to 1.0)
        /// </summary>
        public float MusicVolume
        {
            get => _audioPlayer.MusicVolume;
            set => _audioPlayer.MusicVolume = Math.Clamp(value, 0.0f, 1.0f);
        }

        // ===== SOUND EFFECTS =====

        /// <summary>
        /// Play a sound effect once
        /// </summary>
        /// <param name="path">Path to the sound file</param>
        /// <param name="volume">Volume for this playback (0.0 to 1.0)</param>
        public void PlaySound(string path, float volume = 1.0f)
        {
            var sound = _assetManager.LoadSoundEffect(path);
            var effectiveVolume = MasterVolume * SoundEffectVolume * volume;
            sound.Play(Math.Clamp(effectiveVolume, 0.0f, 1.0f));
        }

        /// <summary>
        /// Create a controllable instance of a sound effect for advanced control
        /// </summary>
        /// <param name="path">Path to the sound file</param>
        /// <returns>A controllable sound instance</returns>
        public ISoundEffectInstance CreateSoundInstance(string path)
        {
            var sound = _assetManager.LoadSoundEffect(path);
            return sound.CreateInstance();
        }

        // ===== MUSIC =====

        /// <summary>
        /// Play background music
        /// </summary>
        /// <param name="path">Path to the music file</param>
        /// <param name="loop">Whether to loop the music</param>
        public void PlayMusic(string path, bool loop = true)
        {
            var music = _assetManager.LoadMusic(path);
            _audioPlayer.PlayMusic(music, loop);
        }

        /// <summary>
        /// Stop the currently playing music
        /// </summary>
        public void StopMusic()
        {
            _audioPlayer.StopMusic();
        }

        /// <summary>
        /// Pause the currently playing music
        /// </summary>
        public void PauseMusic()
        {
            _audioPlayer.PauseMusic();
        }

        /// <summary>
        /// Resume paused music
        /// </summary>
        public void ResumeMusic()
        {
            _audioPlayer.ResumeMusic();
        }

        /// <summary>
        /// Check if music is currently playing
        /// </summary>
        public bool IsMusicPlaying => _audioPlayer.IsPlaying;
    }
}
