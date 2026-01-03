using Kobold.Core.Abstractions.Core;
using Kobold.Core.Abstractions.Rendering;
using Kobold.Core.Abstractions.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
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

        public ISoundEffect LoadSoundEffect(string path)
        {
            string fullPath = path;
            if (!Path.IsPathRooted(path))
            {
                fullPath = Path.Combine(_contentRoot, path);
            }

            // Add .wav extension if no extension provided
            if (!Path.HasExtension(fullPath))
            {
                fullPath += ".wav";
            }

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Sound effect file not found: {fullPath}");
            }

            using (var fileStream = File.OpenRead(fullPath))
            {
                var soundEffect = SoundEffect.FromStream(fileStream);
                return new MonoGameSoundEffect(soundEffect);
            }
        }

        public IMusic LoadMusic(string path)
        {
            string fullPath = path;
            if (!Path.IsPathRooted(path))
            {
                fullPath = Path.Combine(_contentRoot, path);
            }

            // Add .ogg extension if no extension provided
            if (!Path.HasExtension(fullPath))
            {
                fullPath += ".ogg";
            }

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Music file not found: {fullPath}");
            }

            // MonoGame's Song.FromUri requires a URI
            var uri = new Uri(Path.GetFullPath(fullPath));
            var song = Song.FromUri(Path.GetFileNameWithoutExtension(fullPath), uri);
            return new MonoGameMusic(song);
        }

        public bool SoundExists(string path)
        {
            string fullPath = path;
            if (!Path.IsPathRooted(path))
            {
                fullPath = Path.Combine(_contentRoot, path);
            }

            // Check for both .wav and .ogg extensions
            if (!Path.HasExtension(fullPath))
            {
                return File.Exists(fullPath + ".wav") || File.Exists(fullPath + ".ogg");
            }

            return File.Exists(fullPath);
        }
    }
}
