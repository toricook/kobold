using Kobold.Core.Abstractions.Rendering;

namespace Kobold.Core.Abstractions.Core
{
    /// <summary>
    /// Interface for loading game content (textures, sounds, etc.)
    /// </summary>
    public interface IContentLoader
    {
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
    }
}
