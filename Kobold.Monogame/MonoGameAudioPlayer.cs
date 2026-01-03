using Kobold.Core.Abstractions.Audio;
using Microsoft.Xna.Framework.Media;

namespace Kobold.Monogame
{
    /// <summary>
    /// MonoGame implementation of IAudioPlayer using MediaPlayer
    /// </summary>
    public class MonoGameAudioPlayer : IAudioPlayer
    {
        private IMusic? _currentMusic;

        public IMusic? CurrentMusic => _currentMusic;

        public float MusicVolume
        {
            get => MediaPlayer.Volume;
            set => MediaPlayer.Volume = Math.Clamp(value, 0.0f, 1.0f);
        }

        public bool IsPlaying => MediaPlayer.State == MediaState.Playing;

        public void PlayMusic(IMusic music, bool loop = true)
        {
            if (music is MonoGameMusic mgMusic)
            {
                _currentMusic = music;
                MediaPlayer.IsRepeating = loop;
                MediaPlayer.Play(mgMusic.InternalSong);
            }
        }

        public void StopMusic()
        {
            MediaPlayer.Stop();
            _currentMusic = null;
        }

        public void PauseMusic()
        {
            if (MediaPlayer.State == MediaState.Playing)
            {
                MediaPlayer.Pause();
            }
        }

        public void ResumeMusic()
        {
            if (MediaPlayer.State == MediaState.Paused)
            {
                MediaPlayer.Resume();
            }
        }
    }
}
