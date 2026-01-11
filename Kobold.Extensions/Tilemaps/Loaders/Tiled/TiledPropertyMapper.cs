using System;
using System.Collections.Generic;
using Kobold.Extensions.Tilemaps;

namespace Kobold.Extensions.Tilemaps.Loaders.Tiled
{
    /// <summary>
    /// Maps Tiled custom properties to TileProperties.
    /// This is a placeholder for future tile property mapping from TSX files.
    /// </summary>
    public class TiledPropertyMapper
    {
        /// <summary>
        /// Maps a tile type string to tile properties.
        /// </summary>
        public void MapTileType(string type, TileProperties props)
        {
            switch (type.ToLowerInvariant())
            {
                case "solid":
                case "wall":
                    props.IsSolid = true;
                    props.CollisionLayer = TileCollisionLayer.Solid;
                    break;

                case "platform":
                case "oneway":
                    props.IsSolid = true;
                    props.CollisionLayer = TileCollisionLayer.Platform;
                    break;

                case "ice":
                case "slippery":
                    props.IsSolid = true;
                    props.CollisionLayer = TileCollisionLayer.Ice;
                    break;

                case "ladder":
                    props.CollisionLayer = TileCollisionLayer.Ladder;
                    break;

                case "water":
                case "liquid":
                    props.CollisionLayer = TileCollisionLayer.Water;
                    break;

                case "trigger":
                case "sensor":
                    props.CollisionLayer = TileCollisionLayer.Trigger;
                    break;
            }
        }

        /// <summary>
        /// Creates default solid tile properties.
        /// </summary>
        public static TileProperties CreateSolidTile()
        {
            return new TileProperties
            {
                IsSolid = true,
                CollisionLayer = TileCollisionLayer.Solid
            };
        }

        /// <summary>
        /// Creates default platform tile properties.
        /// </summary>
        public static TileProperties CreatePlatformTile()
        {
            return new TileProperties
            {
                IsSolid = true,
                CollisionLayer = TileCollisionLayer.Platform
            };
        }
    }
}
