using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Kobold.Extensions.Combat.Data;

namespace Kobold.Extensions.Combat.Registry
{
    /// <summary>
    /// Central registry for all monster definitions.
    /// Loads from JSON and provides query API for monster lookups.
    /// </summary>
    public class MonsterRegistry
    {
        private readonly Dictionary<string, MonsterDefinition> _monstersById = new Dictionary<string, MonsterDefinition>();
        private readonly List<MonsterDefinition> _allMonsters = new List<MonsterDefinition>();
        private readonly string _contentRoot;

        /// <summary>
        /// Creates a new MonsterRegistry with the specified content root path
        /// </summary>
        /// <param name="contentRoot">Root directory for content files (default: "Content")</param>
        public MonsterRegistry(string contentRoot = "Content")
        {
            _contentRoot = contentRoot;
        }

        /// <summary>
        /// Load monster definitions from a JSON file
        /// </summary>
        /// <param name="path">Path to JSON file (relative to content root or absolute)</param>
        public void LoadMonstersFromJson(string path)
        {
            string fullPath = Path.IsPathRooted(path) ? path : Path.Combine(_contentRoot, path);
            if (!Path.HasExtension(fullPath))
                fullPath += ".json";

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Monster definition file not found: {fullPath}");

            var json = File.ReadAllText(fullPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            };

            var collection = JsonSerializer.Deserialize<MonsterDefinitionCollection>(json, options);

            if (collection?.Monsters == null)
                throw new InvalidOperationException($"Failed to load monsters from: {fullPath}");

            foreach (var monster in collection.Monsters)
            {
                RegisterMonster(monster);
            }

            Console.WriteLine($"Loaded {collection.Monsters.Count} monsters from {fullPath}");
        }

        /// <summary>
        /// Register a single monster definition
        /// </summary>
        /// <param name="monster">Monster definition to register</param>
        public void RegisterMonster(MonsterDefinition monster)
        {
            if (string.IsNullOrEmpty(monster.Id))
                throw new ArgumentException("Monster ID cannot be null or empty");

            if (_monstersById.ContainsKey(monster.Id))
                Console.WriteLine($"Warning: Overwriting existing monster definition: {monster.Id}");

            _monstersById[monster.Id] = monster;

            // Add to all monsters list if not already present
            if (!_allMonsters.Contains(monster))
            {
                _allMonsters.Add(monster);
            }
        }

        /// <summary>
        /// Get monster definition by ID
        /// </summary>
        /// <param name="monsterId">Monster ID to look up</param>
        /// <returns>Monster definition</returns>
        /// <exception cref="KeyNotFoundException">If monster not found</exception>
        public MonsterDefinition GetMonster(string monsterId)
        {
            if (_monstersById.TryGetValue(monsterId, out var monster))
                return monster;

            throw new KeyNotFoundException($"Monster not found: {monsterId}");
        }

        /// <summary>
        /// Try to get monster definition by ID
        /// </summary>
        /// <param name="monsterId">Monster ID to look up</param>
        /// <param name="monster">Output monster definition if found</param>
        /// <returns>True if monster was found, false otherwise</returns>
        public bool TryGetMonster(string monsterId, out MonsterDefinition monster)
        {
            return _monstersById.TryGetValue(monsterId, out monster);
        }

        /// <summary>
        /// Get all registered monsters
        /// </summary>
        /// <returns>Read-only collection of all monsters</returns>
        public IReadOnlyCollection<MonsterDefinition> GetAllMonsters()
        {
            return _allMonsters.AsReadOnly();
        }

        /// <summary>
        /// Get a random monster based on spawn weights
        /// </summary>
        /// <param name="random">Random number generator</param>
        /// <returns>Random monster definition</returns>
        public MonsterDefinition GetRandomMonster(Random random)
        {
            if (_allMonsters.Count == 0)
                throw new InvalidOperationException("No monsters registered");

            // Calculate total weight
            int totalWeight = _allMonsters.Sum(m => m.SpawnWeight);
            int roll = random.Next(totalWeight);

            // Select based on weighted probability
            int currentWeight = 0;
            foreach (var monster in _allMonsters)
            {
                currentWeight += monster.SpawnWeight;
                if (roll < currentWeight)
                    return monster;
            }

            // Fallback to first monster (shouldn't happen)
            return _allMonsters[0];
        }

        /// <summary>
        /// Check if a monster is registered
        /// </summary>
        /// <param name="monsterId">Monster ID to check</param>
        /// <returns>True if the monster exists in the registry</returns>
        public bool HasMonster(string monsterId)
        {
            return _monstersById.ContainsKey(monsterId);
        }

        /// <summary>
        /// Get the count of registered monsters
        /// </summary>
        public int MonsterCount => _monstersById.Count;

        /// <summary>
        /// Clear all registered monsters
        /// </summary>
        public void Clear()
        {
            _monstersById.Clear();
            _allMonsters.Clear();
        }
    }
}
