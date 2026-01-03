using Kobold.Core.Abstractions.Audio;
using Microsoft.Xna.Framework.Media;

namespace Kobold.Monogame
{
    /// <summary>
    /// MonoGame implementation of IMusic
    /// </summary>
    public class MonoGameMusic : IMusic
    {
        public Song InternalSong { get; }
        private bool _disposed = false;

        public MonoGameMusic(Song song)
        {
            InternalSong = song;
        }

        public string Name => InternalSong.Name;
        public TimeSpan Duration => InternalSong.Duration;

        public void Dispose()
        {
            if (!_disposed)
            {
                InternalSong?.Dispose();
                _disposed = true;
            }
        }
    }
}
