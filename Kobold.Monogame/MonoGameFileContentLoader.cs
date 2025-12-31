using Kobold.Core.Abstractions.Core;
using Kobold.Core.Abstractions.Rendering;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace Kobold.Monogame
{
    /// <summary>
    /// MonoGame implementation of content loading that loads raw files directly from the file system.
    /// Loads PNG textures at runtime using Texture2D.FromStream, bypassing the MonoGame Content Pipeline.
    /// </summary>
    public class MonoGameFileContentLoader : IContentLoader
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly string _contentRoot;

        public string ContentRoot => _contentRoot;

        public MonoGameFileContentLoader(GraphicsDevice graphicsDevice, string contentRoot = "Content")
        {
            _graphicsDevice = graphicsDevice;
            _contentRoot = contentRoot;
        }

        public ITexture LoadTexture(string path)
        {
            // Support both relative and absolute paths
            string fullPath = path;
            if (!Path.IsPathRooted(path))
            {
                fullPath = Path.Combine(_contentRoot, path);
            }

            // Add .png extension if no extension provided
            if (!Path.HasExtension(fullPath))
            {
                fullPath += ".png";
            }

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Texture file not found: {fullPath}");
            }

            // Load texture from file
            using (var fileStream = File.OpenRead(fullPath))
            {

                var texture = Texture2D.FromStream(_graphicsDevice, fileStream);
                return new MonoGameTexture(texture);
            }
        }

        public bool TextureExists(string path)
        {
            string fullPath = path;
            if (!Path.IsPathRooted(path))
            {
                fullPath = Path.Combine(_contentRoot, path);
            }

            if (!Path.HasExtension(fullPath))
            {
                fullPath += ".png";
            }

            return File.Exists(fullPath);
        }
    }
}
