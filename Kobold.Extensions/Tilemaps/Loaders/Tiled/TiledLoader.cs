using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Kobold.Core;
using Kobold.Core.Assets;
using Kobold.Core.Services;

namespace Kobold.Extensions.Tilemaps.Loaders.Tiled
{
    /// <summary>
    /// Loader for Tiled map editor files (.tmx).
    /// Uses custom XML parsing for TMX and TSX files.
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

        private class TilesetInfo
        {
            public int FirstGid { get; set; }
            public string Name { get; set; } = "";
            public int TileWidth { get; set; }
            public int TileHeight { get; set; }
            public int Spacing { get; set; }
            public int Margin { get; set; }
            public string ImageSource { get; set; } = "";
        }

        public TilemapData Load(string path, AssetManager assetManager)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Tiled map file not found: {path}", path);

            try
            {
                Console.WriteLine($"Loading Tiled map: {Path.GetFileName(path)}");

                var mapDirectory = Path.GetDirectoryName(path) ?? "";
                var doc = XDocument.Load(path);
                var mapElement = doc.Element("map");

                if (mapElement == null)
                    throw new TilemapLoadException("Invalid TMX file: missing <map> element", path, FormatName);

                // Parse map properties
                int mapWidth = int.Parse(mapElement.Attribute("width")?.Value ?? "0");
                int mapHeight = int.Parse(mapElement.Attribute("height")?.Value ?? "0");
                int tileWidth = int.Parse(mapElement.Attribute("tilewidth")?.Value ?? "0");
                int tileHeight = int.Parse(mapElement.Attribute("tileheight")?.Value ?? "0");

                Console.WriteLine($"  Map size: {mapWidth}x{mapHeight} tiles ({tileWidth}x{tileHeight} pixels)");

                // Load external tilesets
                var tilesets = new List<TilesetInfo>();
                foreach (var tilesetElement in mapElement.Elements("tileset"))
                {
                    var tileset = LoadTileset(tilesetElement, mapDirectory);
                    if (tileset != null)
                    {
                        tilesets.Add(tileset);
                        Console.WriteLine($"  Loaded tileset: {tileset.Name} (firstgid: {tileset.FirstGid})");
                    }
                }

                if (tilesets.Count == 0)
                    throw new TilemapLoadException("No tilesets found in map", path, FormatName);

                // Count tile layers
                var tileLayers = mapElement.Elements("layer").ToList();
                int layerCount = tileLayers.Count;
                Console.WriteLine($"  Layers: {layerCount}");

                // Create TileMap
                var tileMap = new TileMap(mapWidth, mapHeight, tileWidth, tileHeight, layerCount);

                // Create tilemap data early so we can populate it
                var tilemapData = new TilemapData(tileMap, new TileSet(tileWidth, tileHeight))
                {
                    Metadata = new TilemapMetadata
                    {
                        Version = mapElement.Attribute("version")?.Value ?? "1.0",
                        Orientation = mapElement.Attribute("orientation")?.Value ?? "orthogonal",
                        RenderOrder = mapElement.Attribute("renderorder")?.Value ?? "right-down"
                    }
                };

                // Load tile layers
                for (int layerIndex = 0; layerIndex < tileLayers.Count; layerIndex++)
                {
                    LoadTileLayer(tileLayers[layerIndex], tileMap, tilemapData, layerIndex, mapWidth, mapHeight);
                }

                // Update TileSet from first tileset
                var primaryTileset = tilesets[0];
                tilemapData.TileSet = new TileSet(primaryTileset.TileWidth, primaryTileset.TileHeight,
                    primaryTileset.Spacing, primaryTileset.Margin);
                tilemapData.TileSet.TexturePath = primaryTileset.ImageSource;

                // Store all tileset metadata
                // Convert FirstGid to 0-based to match how we store tile IDs
                foreach (var tileset in tilesets)
                {
                    tilemapData.Tilesets.Add(new TilesetMetadata
                    {
                        FirstGid = tileset.FirstGid - 1, // Convert to 0-based
                        Name = tileset.Name,
                        TileWidth = tileset.TileWidth,
                        TileHeight = tileset.TileHeight,
                        Spacing = tileset.Spacing,
                        Margin = tileset.Margin,
                        ImageSource = tileset.ImageSource
                    });
                }

                // Load object layers
                LoadObjectLayers(mapElement, tilemapData);

                Console.WriteLine($"  Map loaded successfully!");
                return tilemapData;
            }
            catch (Exception ex)
            {
                throw new TilemapLoadException($"Failed to load Tiled map: {ex.Message}", path, FormatName, ex);
            }
        }

        /// <summary>
        /// Loads textures for all tilesets in the tilemap data and creates TilesetInfo objects.
        /// This method takes the TilesetMetadata from the loaded tilemap and converts them
        /// to TilesetInfo objects with loaded textures and sprite sheets.
        /// </summary>
        /// <param name="tilemapData">The tilemap data containing tileset metadata</param>
        /// <param name="assetManager">Asset manager to load textures</param>
        /// <returns>List of TilesetInfo objects with loaded textures</returns>
        public List<Tilemaps.TilesetInfo> LoadTilesetTextures(TilemapData tilemapData, AssetManager assetManager)
        {
            var tilesetInfos = new List<Tilemaps.TilesetInfo>();

            foreach (var tilesetMetadata in tilemapData.Tilesets)
            {
                try
                {
                    // Extract just the filename from the path
                    var textureName = Path.GetFileNameWithoutExtension(tilesetMetadata.ImageSource);

                    // Try to load the texture
                    Console.WriteLine($"  Loading texture: {textureName}.png for tileset '{tilesetMetadata.Name}'");
                    var texture = assetManager.LoadTexture(textureName);

                    // Create sprite sheet config for the tileset
                    var spriteSheetConfig = new SpriteSheetConfig
                    {
                        SpriteWidth = tilesetMetadata.TileWidth,
                        SpriteHeight = tilesetMetadata.TileHeight,
                        Spacing = tilesetMetadata.Spacing,
                        Margin = tilesetMetadata.Margin
                    };

                    var spriteSheet = new SpriteSheet(texture, spriteSheetConfig);

                    // Create tileset info
                    var tilesetInfo = new Tilemaps.TilesetInfo(
                        tilesetMetadata.FirstGid,
                        tilesetMetadata.Name,
                        tilesetMetadata.TileWidth,
                        tilesetMetadata.TileHeight,
                        tilesetMetadata.Spacing,
                        tilesetMetadata.Margin
                    )
                    {
                        Texture = texture,
                        SpriteSheet = spriteSheet
                    };

                    tilesetInfos.Add(tilesetInfo);
                    Console.WriteLine($"    Loaded: {texture.Width}x{texture.Height} pixels");
                }
                catch (FileNotFoundException ex)
                {
                    Console.WriteLine($"  ERROR: {ex.Message}");
                    Console.WriteLine($"  Skipping tileset '{tilesetMetadata.Name}'");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ERROR loading texture for '{tilesetMetadata.Name}': {ex.Message}");
                }
            }

            return tilesetInfos;
        }

        private TilesetInfo? LoadTileset(XElement tilesetElement, string mapDirectory)
        {
            var source = tilesetElement.Attribute("source")?.Value;
            var firstGid = int.Parse(tilesetElement.Attribute("firstgid")?.Value ?? "1");

            if (source != null)
            {
                // External tileset - load from .tsx file
                var tilesetPath = Path.Combine(mapDirectory, source);
                if (!File.Exists(tilesetPath))
                {
                    Console.WriteLine($"  Warning: External tileset not found: {tilesetPath}");
                    return null;
                }

                var tsxDoc = XDocument.Load(tilesetPath);
                var tsxTilesetElement = tsxDoc.Element("tileset");
                if (tsxTilesetElement == null)
                    return null;

                var imageElement = tsxTilesetElement.Element("image");
                if (imageElement == null)
                    return null;

                return new TilesetInfo
                {
                    FirstGid = firstGid,
                    Name = tsxTilesetElement.Attribute("name")?.Value ?? "",
                    TileWidth = int.Parse(tsxTilesetElement.Attribute("tilewidth")?.Value ?? "16"),
                    TileHeight = int.Parse(tsxTilesetElement.Attribute("tileheight")?.Value ?? "16"),
                    Spacing = int.Parse(tsxTilesetElement.Attribute("spacing")?.Value ?? "0"),
                    Margin = int.Parse(tsxTilesetElement.Attribute("margin")?.Value ?? "0"),
                    ImageSource = imageElement.Attribute("source")?.Value ?? ""
                };
            }

            return null;
        }

        private void LoadTileLayer(XElement layerElement, TileMap tileMap, TilemapData tilemapData, int layerIndex, int mapWidth, int mapHeight)
        {
            // Parse layer metadata
            var layerName = layerElement.Attribute("name")?.Value ?? "unnamed";
            var visible = bool.Parse(layerElement.Attribute("visible")?.Value ?? "true");
            var opacity = float.Parse(layerElement.Attribute("opacity")?.Value ?? "1");

            // Parse y_sort from layer properties
            bool ySort = false;
            var propertiesElement = layerElement.Element("properties");
            if (propertiesElement != null)
            {
                foreach (var propElement in propertiesElement.Elements("property"))
                {
                    var propName = propElement.Attribute("name")?.Value;
                    if (propName == "y_sort")
                    {
                        var propValue = propElement.Attribute("value")?.Value;
                        ySort = bool.Parse(propValue ?? "false");
                    }
                }
            }

            // Store layer metadata
            tilemapData.TileLayers.Add(new TileLayerMetadata
            {
                LayerIndex = layerIndex,
                Name = layerName,
                Visible = visible,
                Opacity = opacity,
                YSort = ySort
            });

            var dataElement = layerElement.Element("data");
            if (dataElement == null)
                return;

            var encoding = dataElement.Attribute("encoding")?.Value;
            if (encoding == "csv")
            {
                // Parse CSV data
                var csvData = dataElement.Value.Trim();
                var tileIds = csvData.Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.Parse(s.Trim()))
                    .ToArray();

                int index = 0;
                for (int y = 0; y < mapHeight && index < tileIds.Length; y++)
                {
                    for (int x = 0; x < mapWidth && index < tileIds.Length; x++)
                    {
                        int globalTileId = tileIds[index++];
                        // Convert global ID to local tile ID (0-based)
                        int tileId = globalTileId > 0 ? globalTileId - 1 : -1;
                        tileMap.SetTile(layerIndex, x, y, tileId);
                    }
                }

                string ySortStatus = ySort ? " [Y-SORT]" : "";
                Console.WriteLine($"    Layer {layerIndex}: {layerName} ({index} tiles){ySortStatus}");
            }
            else
            {
                Console.WriteLine($"    Warning: Unsupported encoding '{encoding}' for layer {layerIndex}");
            }
        }

        private void LoadObjectLayers(XElement mapElement, TilemapData tilemapData)
        {
            foreach (var objectGroupElement in mapElement.Elements("objectgroup"))
            {
                var tilemapObjectLayer = new TilemapObjectLayer
                {
                    Name = objectGroupElement.Attribute("name")?.Value ?? "Objects",
                    Visible = bool.Parse(objectGroupElement.Attribute("visible")?.Value ?? "true"),
                    Opacity = float.Parse(objectGroupElement.Attribute("opacity")?.Value ?? "1")
                };

                // Parse y_sort from layer properties
                var layerPropertiesElement = objectGroupElement.Element("properties");
                if (layerPropertiesElement != null)
                {
                    foreach (var propElement in layerPropertiesElement.Elements("property"))
                    {
                        var propName = propElement.Attribute("name")?.Value;
                        var propValue = propElement.Attribute("value")?.Value;

                        if (propName == "y_sort")
                        {
                            tilemapObjectLayer.YSort = bool.Parse(propValue ?? "false");
                        }
                        else
                        {
                            // Store other layer properties
                            tilemapObjectLayer.Properties[propName ?? ""] = propValue ?? "";
                        }
                    }
                }

                foreach (var objElement in objectGroupElement.Elements("object"))
                {
                    var tilemapObj = new TilemapObject
                    {
                        Id = int.Parse(objElement.Attribute("id")?.Value ?? "0"),
                        Name = objElement.Attribute("name")?.Value ?? "",
                        Type = objElement.Attribute("type")?.Value ?? "",
                        X = float.Parse(objElement.Attribute("x")?.Value ?? "0"),
                        Y = float.Parse(objElement.Attribute("y")?.Value ?? "0"),
                        Width = float.Parse(objElement.Attribute("width")?.Value ?? "0"),
                        Height = float.Parse(objElement.Attribute("height")?.Value ?? "0"),
                        Rotation = float.Parse(objElement.Attribute("rotation")?.Value ?? "0"),
                        Visible = bool.Parse(objElement.Attribute("visible")?.Value ?? "true"),
                        Shape = TilemapObjectShape.Rectangle
                    };

                    // Parse object properties (including y_sort override)
                    var objPropertiesElement = objElement.Element("properties");
                    if (objPropertiesElement != null)
                    {
                        foreach (var propElement in objPropertiesElement.Elements("property"))
                        {
                            var propName = propElement.Attribute("name")?.Value;
                            var propValue = propElement.Attribute("value")?.Value;

                            if (propName == "y_sort")
                            {
                                tilemapObj.YSort = bool.Parse(propValue ?? "false");
                            }
                            else
                            {
                                // Store other object properties
                                tilemapObj.Properties[propName ?? ""] = propValue ?? "";
                            }
                        }
                    }

                    tilemapObjectLayer.Objects.Add(tilemapObj);
                }

                string ySortStatus = tilemapObjectLayer.YSort ? " [Y-SORT]" : "";
                tilemapData.ObjectLayers.Add(tilemapObjectLayer);
                Console.WriteLine($"    Object layer: {tilemapObjectLayer.Name} ({tilemapObjectLayer.Objects.Count} objects){ySortStatus}");
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
