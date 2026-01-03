using Kobold.Core.Abstractions.Audio;
using Microsoft.Xna.Framework.Audio;

namespace Kobold.Monogame
{
    /// <summary>
    /// MonoGame implementation of ISoundEffect
    /// </summary>
    public class MonoGameSoundEffect : ISoundEffect
    {
        public SoundEffect InternalSoundEffect { get; }
        private bool _disposed = false;

        public MonoGameSoundEffect(SoundEffect soundEffect)
        {
            InternalSoundEffect = soundEffect;
        }

        public void Play(float volume = 1.0f)
        {
            InternalSoundEffect.Play(Math.Clamp(volume, 0.0f, 1.0f), 0.0f, 0.0f);
        }

        public ISoundEffectInstance CreateInstance()
        {
            var instance = InternalSoundEffect.CreateInstance();
            return new MonoGameSoundEffectInstance(instance);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                InternalSoundEffect?.Dispose();
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// MonoGame implementation of ISoundEffectInstance
    /// </summary>
    public class MonoGameSoundEffectInstance : ISoundEffectInstance
    {
        private readonly SoundEffectInstance _instance;
        private bool _disposed = false;

        public MonoGameSoundEffectInstance(SoundEffectInstance instance)
        {
            _instance = instance;
        }

        public float Volume
        {
            get => _instance.Volume;
            set => _instance.Volume = Math.Clamp(value, 0.0f, 1.0f);
        }

        public bool IsLooped
        {
            get => _instance.IsLooped;
            set => _instance.IsLooped = value;
        }

        public Kobold.Core.Abstractions.Audio.SoundState State => _instance.State switch
        {
            Microsoft.Xna.Framework.Audio.SoundState.Playing => Kobold.Core.Abstractions.Audio.SoundState.Playing,
            Microsoft.Xna.Framework.Audio.SoundState.Paused => Kobold.Core.Abstractions.Audio.SoundState.Paused,
            Microsoft.Xna.Framework.Audio.SoundState.Stopped => Kobold.Core.Abstractions.Audio.SoundState.Stopped,
            _ => Kobold.Core.Abstractions.Audio.SoundState.Stopped
        };

        public void Play() => _instance.Play();
        public void Pause() => _instance.Pause();
        public void Resume() => _instance.Resume();
        public void Stop() => _instance.Stop();

        public void Dispose()
        {
            if (!_disposed)
            {
                _instance?.Dispose();
                _disposed = true;
            }
        }
    }
}
