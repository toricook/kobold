using Kobold.Core.Abstractions.Rendering;
using Kobold.Core.Abstractions.Audio;

namespace Kobold.Core.Abstractions.Core
{
    /// <summary>
    /// Interface for loading game content (textures, sounds, etc.)
    /// </summary>
    public interface IContentLoader
    {
        /// <summary>
        /// Root directory for content files
        /// </summary>
        string ContentRoot { get; }

        /// <summary>
        /// Load a texture from a file path
        /// </summary>
        /// <param name="path">Path to the texture file (relative to content directory)</param>
        /// <returns>The loaded texture</returns>
        ITexture LoadTexture(string path);

        /// <summary>
        /// Check if a texture file exists
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>True if the file exists</returns>
        bool TextureExists(string path);

        /// <summary>
        /// Load a sound effect from a file path
        /// </summary>
        /// <param name="path">Path to the sound file (relative to content directory)</param>
        /// <returns>The loaded sound effect</returns>
        ISoundEffect LoadSoundEffect(string path);

        /// <summary>
        /// Load a music track from a file path
        /// </summary>
        /// <param name="path">Path to the music file (relative to content directory)</param>
        /// <returns>The loaded music track</returns>
        IMusic LoadMusic(string path);

        /// <summary>
        /// Check if a sound file exists
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>True if the file exists</returns>
        bool SoundExists(string path);
    }
}
