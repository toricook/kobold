using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kobold.Core;

namespace Kobold.Extensions.Tilemaps.Loaders.Tiled
{
    /// <summary>
    /// Loader for Tiled map editor files (.tmx).
    /// Uses DotTiled library for parsing Tiled maps.
    ///
    /// NOTE: This is an initial implementation of the Tiled loader.
    /// The DotTiled library API may require adjustments based on testing with actual .tmx files.
    /// Some features like object layers and advanced tile properties may need refinement.
    /// </summary>
    public class TiledLoader : ITilemapLoader
    {
        private readonly TiledPropertyMapper _propertyMapper;

        public TiledLoader(TiledPropertyMapper? propertyMapper = null)
        {
            _propertyMapper = propertyMapper ?? new TiledPropertyMapper();
        }

        public string SupportedExtension => ".tmx";
        public string FormatName => "Tiled";

        public TilemapData Load(string path, AssetManager assetManager)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Tiled map file not found: {path}", path);

            // TODO: Complete DotTiled integration
            // The DotTiled 0.2.0 API needs to be verified with actual .tmx files
            // Possible loading methods to try:
            // - DotTiled.MapReader.ReadMap(path)
            // - DotTiled.Serialization.TmxLoader.Load(path)
            // - Or use the correct static method from DotTiled documentation

            throw new NotImplementedException(
                "TiledLoader implementation needs completion. " +
                "The DotTiled API for loading maps needs to be determined through testing with actual .tmx files. " +
                "Please refer to DotTiled documentation at https://dcronqvist.github.io/DotTiled/");
        }

        private TilemapData CreateBasicTilemapData(DotTiled.Map map, string mapPath, AssetManager assetManager)
        {
            // Create a basic tilemap - this implementation focuses on getting it to compile
            // and will need refinement based on actual DotTiled API testing

            var width = (int)map.Width;
            var height = (int)map.Height;
            var tileWidth = (int)map.TileWidth;
            var tileHeight = (int)map.TileHeight;

            // Find tile layers
            var tileLayers = map.Layers.Where(l => l is DotTiled.TileLayer).Cast<DotTiled.TileLayer>().ToList();
            if (tileLayers.Count == 0)
                throw new TilemapLoadException("No tile layers found in map", mapPath, FormatName);

            // Create TileMap
            var tileMap = new TileMap(width, height, tileWidth, tileHeight, tileLayers.Count);

            // Fill layers
            for (int i = 0; i < tileLayers.Count; i++)
            {
                FillLayer(tileMap, tileLayers[i], i);
            }

            // Create TileSet
            var tileSet = CreateTileSet(map);

            // Create result
            var tilemapData = new TilemapData(tileMap, tileSet)
            {
                Metadata = new TilemapMetadata
                {
                    Version = map.Version ?? "1.0",
                    Orientation = map.Orientation.ToString(),
                    RenderOrder = map.RenderOrder.ToString()
                }
            };

            // Extract object layers (simplified)
            ExtractObjectLayers(map, tilemapData);

            // Try to load textures
            TryLoadTextures(map, mapPath, assetManager, tilemapData);

            return tilemapData;
        }

        private void FillLayer(TileMap tileMap, DotTiled.TileLayer layer, int layerIndex)
        {
            // Check if layer has data
            if (!layer.Data.HasValue)
                return;

            var data = layer.Data.Value;

            // Access GlobalTileIDs - handle Optional wrapper
            if (!data.GlobalTileIDs.HasValue)
                return;

            var tileIds = data.GlobalTileIDs.Value;

            for (int y = 0; y < tileMap.Height; y++)
            {
                for (int x = 0; x < tileMap.Width; x++)
                {
                    int index = y * tileMap.Width + x;
                    if (index < tileIds.Length)
                    {
                        uint globalId = tileIds[index];
                        int tileId = globalId > 0 ? (int)globalId - 1 : -1;
                        tileMap.SetTile(layerIndex, x, y, tileId);
                    }
                }
            }
        }

        private TileSet CreateTileSet(DotTiled.Map map)
        {
            if (map.Tilesets == null || map.Tilesets.Count == 0)
                throw new TilemapLoadException("No tilesets found", "", FormatName);

            var tileset = map.Tilesets[0];
            var tileSet = new TileSet(
                (int)tileset.TileWidth,
                (int)tileset.TileHeight,
                (int)tileset.Spacing,
                (int)tileset.Margin);

            // Set texture path if available
            if (tileset.Image.HasValue && tileset.Image.Value.Source != null)
            {
                tileSet.TexturePath = tileset.Image.Value.Source;
            }

            // Map tile properties using property mapper
            if (tileset.Tiles != null)
            {
                foreach (var tile in tileset.Tiles)
                {
                    var props = _propertyMapper.MapProperties(tile);
                    tileSet.SetTileProperties((int)tile.ID, props);
                }
            }

            return tileSet;
        }

        private void ExtractObjectLayers(DotTiled.Map map, TilemapData tilemapData)
        {
            var objectLayers = map.Layers.Where(l => l is DotTiled.ObjectLayer).Cast<DotTiled.ObjectLayer>();

            foreach (var layer in objectLayers)
            {
                var tilemapObjectLayer = new TilemapObjectLayer
                {
                    Name = layer.Name ?? "Objects",
                    Visible = layer.Visible,
                    Opacity = layer.Opacity
                };

                // Add objects if present
                if (layer.Objects != null && layer.Objects.Count > 0)
                {
                    foreach (var obj in layer.Objects)
                    {
                        var tilemapObj = new TilemapObject
                        {
                            Id = obj.ID.HasValue ? (int)obj.ID.Value : 0,
                            Name = obj.Name ?? string.Empty,
                            Type = obj.Type ?? string.Empty,
                            X = obj.X,
                            Y = obj.Y,
                            Width = obj.Width,
                            Height = obj.Height,
                            Rotation = obj.Rotation,
                            Visible = obj.Visible,
                            Shape = TilemapObjectShape.Rectangle
                        };

                        tilemapObjectLayer.Objects.Add(tilemapObj);
                    }
                }

                tilemapData.ObjectLayers.Add(tilemapObjectLayer);
            }
        }

        private void TryLoadTextures(DotTiled.Map map, string mapPath, AssetManager assetManager, TilemapData tilemapData)
        {
            var mapDirectory = Path.GetDirectoryName(mapPath) ?? "";

            foreach (var tileset in map.Tilesets)
            {
                if (!tileset.Image.HasValue || tileset.Image.Value.Source == null)
                    continue;

                try
                {
                    var texturePath = tileset.Image.Value.Source;
                    var assetName = Path.GetFileNameWithoutExtension(texturePath);

                    var texture = assetManager.GetTexture(assetName);
                    if (texture != null)
                    {
                        tilemapData.LoadedTextures[tileset.Name ?? "default"] = texture;
                    }
                    else
                    {
                        // Store required asset for user to load
                        if (!Path.IsPathRooted(texturePath))
                        {
                            texturePath = Path.Combine(mapDirectory, texturePath);
                        }
                        tilemapData.RequiredAssets.Add(texturePath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to process texture for tileset '{tileset.Name}': {ex.Message}");
                }
            }
        }
    }

    public class TilemapLoadException : Exception
    {
        public string FilePath { get; }
        public string LoaderFormat { get; }

        public TilemapLoadException(string message, string filePath, string format)
            : base(message)
        {
            FilePath = filePath;
            LoaderFormat = format;
        }

        public TilemapLoadException(string message, string filePath, string format, Exception innerException)
            : base(message, innerException)
        {
            FilePath = filePath;
            LoaderFormat = format;
        }
    }
}
