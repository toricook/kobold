using Kobold.Extensions.Tilemaps;

namespace Kobold.Extensions.Procedural
{
    /// <summary>
    /// Generates tilemaps using cellular automata algorithms.
    /// Useful for creating organic cave-like structures and procedural dungeons.
    /// </summary>
    public class CellularAutomataGenerator
    {
        private readonly CellularAutomataConfig _config;
        private readonly Random _random;
        private bool[,] _grid;

        public CellularAutomataGenerator(CellularAutomataConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            int seed = config.Seed ?? Environment.TickCount;
            _random = new Random(seed);

            _grid = new bool[config.Width, config.Height];
        }

        /// <summary>
        /// Generates a new tilemap using cellular automata.
        /// </summary>
        /// <returns>Generated TileMap</returns>
        public TileMap Generate()
        {
            // Step 1: Initialize grid with random values
            InitializeGrid();

            // Step 2: Apply cellular automata rules for N iterations
            for (int i = 0; i < _config.Iterations; i++)
            {
                SmoothGrid();
            }

            // Step 3: Connect caves if requested
            if (_config.ConnectCaves)
            {
                ConnectRegions();
            }

            // Step 4: Convert grid to TileMap
            return ConvertToTileMap();
        }

        /// <summary>
        /// Initializes the grid with random wall/floor values.
        /// </summary>
        private void InitializeGrid()
        {
            for (int x = 0; x < _config.Width; x++)
            {
                for (int y = 0; y < _config.Height; y++)
                {
                    // Random chance to be a wall
                    _grid[x, y] = _random.NextDouble() < _config.InitialWallProbability;
                }
            }
        }

        /// <summary>
        /// Applies one iteration of cellular automata smoothing.
        /// </summary>
        private void SmoothGrid()
        {
            bool[,] newGrid = new bool[_config.Width, _config.Height];

            for (int x = 0; x < _config.Width; x++)
            {
                for (int y = 0; y < _config.Height; y++)
                {
                    int wallCount = CountNeighboringWalls(x, y);

                    // Apply cellular automata rules
                    if (_grid[x, y])
                    {
                        // Cell is currently a wall
                        // Dies if too few neighbors
                        newGrid[x, y] = wallCount > _config.DeathThreshold;
                    }
                    else
                    {
                        // Cell is currently floor
                        // Becomes wall if enough neighbors
                        newGrid[x, y] = wallCount >= _config.BirthThreshold;
                    }
                }
            }

            _grid = newGrid;
        }

        /// <summary>
        /// Counts the number of wall neighbors around a cell (Moore neighborhood - 8 surrounding cells).
        /// </summary>
        private int CountNeighboringWalls(int x, int y)
        {
            int count = 0;

            for (int nx = x - 1; nx <= x + 1; nx++)
            {
                for (int ny = y - 1; ny <= y + 1; ny++)
                {
                    // Skip the center cell
                    if (nx == x && ny == y)
                        continue;

                    // Check if neighbor is in bounds
                    if (nx < 0 || nx >= _config.Width || ny < 0 || ny >= _config.Height)
                    {
                        // Out of bounds - treat as wall if EdgeIsWall is true
                        if (_config.EdgeIsWall)
                            count++;
                    }
                    else
                    {
                        // In bounds - count if it's a wall
                        if (_grid[nx, ny])
                            count++;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Converts the boolean grid to a TileMap.
        /// </summary>
        private TileMap ConvertToTileMap()
        {
            var tileMap = new TileMap(
                _config.Width,
                _config.Height,
                _config.TileWidth,
                _config.TileHeight,
                layerCount: 1
            );

            for (int x = 0; x < _config.Width; x++)
            {
                for (int y = 0; y < _config.Height; y++)
                {
                    int tileId = _grid[x, y] ? _config.WallTileId : _config.FloorTileId;
                    tileMap.SetTile(0, x, y, tileId);
                }
            }

            return tileMap;
        }

        /// <summary>
        /// Gets the current state of the grid (for debugging or visualization).
        /// </summary>
        /// <returns>Copy of the current grid state</returns>
        public bool[,] GetGrid()
        {
            var copy = new bool[_config.Width, _config.Height];
            Array.Copy(_grid, copy, _grid.Length);
            return copy;
        }

        #region Cave Connectivity

        /// <summary>
        /// Represents a cave region (connected floor tiles).
        /// </summary>
        private class CaveRegion
        {
            public List<(int x, int y)> Tiles { get; } = new List<(int x, int y)>();
            public (int x, int y) Centroid { get; set; }

            public void CalculateCentroid()
            {
                if (Tiles.Count == 0)
                {
                    Centroid = (0, 0);
                    return;
                }

                long sumX = 0;
                long sumY = 0;
                foreach (var tile in Tiles)
                {
                    sumX += tile.x;
                    sumY += tile.y;
                }
                Centroid = ((int)(sumX / Tiles.Count), (int)(sumY / Tiles.Count));
            }
        }

        /// <summary>
        /// Connects all cave regions using flood fill + Minimum Spanning Tree.
        /// Based on best practices from procedural dungeon generation literature.
        /// </summary>
        private void ConnectRegions()
        {
            // Step 1: Identify all cave regions using flood fill
            var regions = IdentifyCaveRegions();

            if (regions.Count <= 1)
                return; // Already connected or no caves

            // Step 2: Remove small regions
            RemoveSmallRegions(regions);

            if (regions.Count <= 1)
                return;

            // Step 3: Calculate centroids for each region
            foreach (var region in regions)
            {
                region.CalculateCentroid();
            }

            // Step 4: Connect regions using Minimum Spanning Tree
            ConnectRegionsWithMST(regions);
        }

        /// <summary>
        /// Identifies separate cave regions using flood fill algorithm.
        /// </summary>
        private List<CaveRegion> IdentifyCaveRegions()
        {
            var regions = new List<CaveRegion>();
            var visited = new bool[_config.Width, _config.Height];

            for (int x = 0; x < _config.Width; x++)
            {
                for (int y = 0; y < _config.Height; y++)
                {
                    // Find unvisited floor tiles
                    if (!visited[x, y] && !_grid[x, y])
                    {
                        var region = new CaveRegion();
                        FloodFill(x, y, visited, region);
                        regions.Add(region);
                    }
                }
            }

            return regions;
        }

        /// <summary>
        /// Flood fill algorithm to identify connected floor tiles.
        /// </summary>
        private void FloodFill(int startX, int startY, bool[,] visited, CaveRegion region)
        {
            var queue = new Queue<(int x, int y)>();
            queue.Enqueue((startX, startY));
            visited[startX, startY] = true;

            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();
                region.Tiles.Add((x, y));

                // Check 4-directional neighbors
                var neighbors = new[] { (x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1) };
                foreach (var (nx, ny) in neighbors)
                {
                    if (nx >= 0 && nx < _config.Width && ny >= 0 && ny < _config.Height &&
                        !visited[nx, ny] && !_grid[nx, ny])
                    {
                        visited[nx, ny] = true;
                        queue.Enqueue((nx, ny));
                    }
                }
            }
        }

        /// <summary>
        /// Removes regions smaller than the minimum size by filling them with walls.
        /// </summary>
        private void RemoveSmallRegions(List<CaveRegion> regions)
        {
            var toRemove = new List<CaveRegion>();

            foreach (var region in regions)
            {
                if (region.Tiles.Count < _config.MinCaveSize)
                {
                    // Fill small region with walls
                    foreach (var (x, y) in region.Tiles)
                    {
                        _grid[x, y] = true;
                    }
                    toRemove.Add(region);
                }
            }

            foreach (var region in toRemove)
            {
                regions.Remove(region);
            }
        }

        /// <summary>
        /// Connects cave regions using Minimum Spanning Tree (Prim's algorithm).
        /// This ensures all caves are reachable with minimal total corridor length.
        /// </summary>
        private void ConnectRegionsWithMST(List<CaveRegion> regions)
        {
            var connected = new HashSet<int> { 0 }; // Start with first region
            var unconnected = new HashSet<int>();
            for (int i = 1; i < regions.Count; i++)
                unconnected.Add(i);

            // Prim's algorithm: repeatedly find the shortest edge between connected and unconnected sets
            while (unconnected.Count > 0)
            {
                int bestFrom = -1;
                int bestTo = -1;
                double bestDistance = double.MaxValue;

                // Find closest pair between connected and unconnected regions
                foreach (var connectedIdx in connected)
                {
                    foreach (var unconnectedIdx in unconnected)
                    {
                        var dist = Distance(regions[connectedIdx].Centroid, regions[unconnectedIdx].Centroid);
                        if (dist < bestDistance)
                        {
                            bestDistance = dist;
                            bestFrom = connectedIdx;
                            bestTo = unconnectedIdx;
                        }
                    }
                }

                // Create corridor between the two regions
                if (bestFrom != -1 && bestTo != -1)
                {
                    CreateCorridor(regions[bestFrom].Centroid, regions[bestTo].Centroid);
                    connected.Add(bestTo);
                    unconnected.Remove(bestTo);
                }
            }
        }

        /// <summary>
        /// Calculates Euclidean distance between two points.
        /// </summary>
        private double Distance((int x, int y) a, (int x, int y) b)
        {
            int dx = a.x - b.x;
            int dy = a.y - b.y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Creates a corridor between two points using straight lines (L-shaped path).
        /// Randomly chooses between horizontal-first or vertical-first.
        /// </summary>
        private void CreateCorridor((int x, int y) start, (int x, int y) end)
        {
            // Randomly choose between horizontal-first or vertical-first
            if (_random.Next(2) == 0)
            {
                // Horizontal then vertical
                CarveHorizontalLine(start.x, end.x, start.y);
                CarveVerticalLine(start.y, end.y, end.x);
            }
            else
            {
                // Vertical then horizontal
                CarveVerticalLine(start.y, end.y, start.x);
                CarveHorizontalLine(start.x, end.x, end.y);
            }
        }

        /// <summary>
        /// Carves a horizontal corridor with some width for better connectivity.
        /// </summary>
        private void CarveHorizontalLine(int x1, int x2, int y)
        {
            int minX = Math.Min(x1, x2);
            int maxX = Math.Max(x1, x2);

            for (int x = minX; x <= maxX; x++)
            {
                for (int dy = -1; dy <= 1; dy++) // Width of 3
                {
                    int ny = y + dy;
                    if (ny >= 0 && ny < _config.Height && x >= 0 && x < _config.Width)
                    {
                        _grid[x, ny] = false; // Clear wall to create floor
                    }
                }
            }
        }

        /// <summary>
        /// Carves a vertical corridor with some width for better connectivity.
        /// </summary>
        private void CarveVerticalLine(int y1, int y2, int x)
        {
            int minY = Math.Min(y1, y2);
            int maxY = Math.Max(y1, y2);

            for (int y = minY; y <= maxY; y++)
            {
                for (int dx = -1; dx <= 1; dx++) // Width of 3
                {
                    int nx = x + dx;
                    if (nx >= 0 && nx < _config.Width && y >= 0 && y < _config.Height)
                    {
                        _grid[nx, y] = false; // Clear wall to create floor
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Generates a tilemap and tileset together.
        /// The tileset will have wall and floor tiles configured with appropriate properties.
        /// </summary>
        /// <param name="wallIsSolid">Whether wall tiles should have collision</param>
        /// <returns>Tuple of (TileMap, TileSet)</returns>
        public (TileMap tileMap, TileSet tileSet) GenerateWithTileSet(bool wallIsSolid = true)
        {
            var tileMap = Generate();

            var tileSet = new TileSet(_config.TileWidth, _config.TileHeight);

            // Configure floor tile (non-solid)
            tileSet.SetTileProperties(_config.FloorTileId, new TileProperties
            {
                IsSolid = false,
                CollisionLayer = TileCollisionLayer.None
            });

            // Configure wall tile (solid if specified)
            tileSet.SetTileProperties(_config.WallTileId, new TileProperties
            {
                IsSolid = wallIsSolid,
                CollisionLayer = wallIsSolid ? TileCollisionLayer.Solid : TileCollisionLayer.None
            });

            return (tileMap, tileSet);
        }
    }
}
