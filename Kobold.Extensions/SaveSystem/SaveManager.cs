using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using Arch.Core;
using Kobold.Core.Events;
using Kobold.Extensions.SaveSystem.Data;
using Kobold.Extensions.SaveSystem.Events;
using Kobold.Extensions.SaveSystem.Serializers;

namespace Kobold.Extensions.SaveSystem
{
    /// <summary>
    /// Main public API for the save system.
    /// Handles save/load operations, compression, file management, and component serializer registration.
    /// </summary>
    public class SaveManager
    {
        private readonly World _world;
        private readonly EventBus _eventBus;
        private readonly ComponentSerializerRegistry _registry;
        private readonly WorldSerializer _worldSerializer;
        private readonly string _saveDirectory;
        private readonly string _metadataDirectory;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Creates a new SaveManager instance.
        /// </summary>
        /// <param name="world">ECS World to save/load</param>
        /// <param name="eventBus">Event bus for publishing save/load events</param>
        /// <param name="saveDirectory">Directory name for saves (relative to user data path)</param>
        public SaveManager(World world, EventBus eventBus, string saveDirectory = "Saves")
        {
            _world = world;
            _eventBus = eventBus;
            _registry = new ComponentSerializerRegistry();
            _worldSerializer = new WorldSerializer(world, _registry);

            // Setup save directories
            var userDataPath = GetUserDataPath();
            _saveDirectory = Path.Combine(userDataPath, saveDirectory);
            _metadataDirectory = Path.Combine(_saveDirectory, "metadata");

            Directory.CreateDirectory(_saveDirectory);
            Directory.CreateDirectory(_metadataDirectory);

            // Configure JSON serialization
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true, // Human-readable for debugging
                IncludeFields = true
            };

            // Register default serializers for common components
            DefaultSerializers.RegisterAll(_registry);
        }

        #region Public API

        /// <summary>
        /// Saves the current world state to a save slot.
        /// </summary>
        /// <param name="slotName">Save slot name (e.g., "save_1", "autosave")</param>
        /// <param name="customMetadata">Optional custom metadata to store</param>
        /// <param name="playtime">Total playtime in seconds (default: 0)</param>
        /// <returns>True if save succeeded</returns>
        public bool Save(string slotName, Dictionary<string, string>? customMetadata = null, float playtime = 0f)
        {
            try
            {
                var savePath = GetSavePath(slotName);
                var backupPath = savePath + ".backup";

                // Create backup of existing save
                if (File.Exists(savePath))
                {
                    File.Copy(savePath, backupPath, overwrite: true);
                }

                // Create save data
                var metadata = SaveMetadata.Create(slotName, playtime, customMetadata);
                var saveData = _worldSerializer.SerializeWorld(metadata);

                // Save to file with compression
                SaveToFile(savePath, saveData);

                // Save metadata separately for quick access
                SaveMetadataToFile(GetMetadataPath(slotName), metadata);

                // Verify save file
                if (VerifySaveFile(savePath))
                {
                    // Save successful, delete backup
                    if (File.Exists(backupPath))
                    {
                        File.Delete(backupPath);
                    }

                    _eventBus.Publish(new SaveCompletedEvent { SlotName = slotName });
                    return true;
                }
                else
                {
                    // Save corrupted, restore backup
                    if (File.Exists(backupPath))
                    {
                        File.Copy(backupPath, savePath, overwrite: true);
                    }

                    throw new InvalidOperationException("Save file verification failed");
                }
            }
            catch (Exception ex)
            {
                _eventBus.Publish(new SaveErrorEvent { Message = ex.Message, SlotName = slotName });
                return false;
            }
        }

        /// <summary>
        /// Loads world state from a save slot.
        /// WARNING: This destroys all existing entities!
        /// </summary>
        /// <param name="slotName">Save slot name</param>
        /// <returns>True if load succeeded</returns>
        public bool Load(string slotName)
        {
            try
            {
                var savePath = GetSavePath(slotName);

                if (!File.Exists(savePath))
                {
                    throw new FileNotFoundException($"Save file not found: {slotName}");
                }

                // Load save data
                var saveData = LoadFromFile(savePath);

                // Check version compatibility (basic check for now)
                if (string.IsNullOrEmpty(saveData.Metadata.SaveSystemVersion))
                {
                    throw new InvalidOperationException("Save file is missing version information");
                }

                // Destroy all existing entities before loading
                DestroyAllEntities();

                // Deserialize world state
                _worldSerializer.DeserializeWorld(saveData);

                _eventBus.Publish(new LoadCompletedEvent { SlotName = slotName, Metadata = saveData.Metadata });
                return true;
            }
            catch (Exception ex)
            {
                _eventBus.Publish(new SaveErrorEvent { Message = ex.Message, SlotName = slotName });
                return false;
            }
        }

        /// <summary>
        /// Deletes a save slot.
        /// </summary>
        public bool DeleteSave(string slotName)
        {
            try
            {
                var savePath = GetSavePath(slotName);
                var metadataPath = GetMetadataPath(slotName);

                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }

                if (File.Exists(metadataPath))
                {
                    File.Delete(metadataPath);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if a save slot exists.
        /// </summary>
        public bool SaveExists(string slotName)
        {
            return File.Exists(GetSavePath(slotName));
        }

        /// <summary>
        /// Gets metadata for a save slot without loading the full save.
        /// </summary>
        public SaveMetadata? GetSaveMetadata(string slotName)
        {
            try
            {
                var metadataPath = GetMetadataPath(slotName);

                if (File.Exists(metadataPath))
                {
                    var json = File.ReadAllText(metadataPath);
                    return JsonSerializer.Deserialize<SaveMetadata>(json, _jsonOptions);
                }
            }
            catch
            {
                // If metadata file doesn't exist or is corrupted, try loading from main save
                try
                {
                    var saveData = LoadFromFile(GetSavePath(slotName));
                    return saveData.Metadata;
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets metadata for all save slots.
        /// </summary>
        public IEnumerable<SaveMetadata> GetAllSaveMetadata()
        {
            var saves = new List<SaveMetadata>();

            foreach (var file in Directory.GetFiles(_saveDirectory, "*.sav"))
            {
                var slotName = Path.GetFileNameWithoutExtension(file);
                var metadata = GetSaveMetadata(slotName);

                if (metadata != null)
                {
                    saves.Add(metadata);
                }
            }

            return saves;
        }

        #endregion

        #region Serializer Registration

        /// <summary>
        /// Registers a component serializer.
        /// </summary>
        public void RegisterSerializer<TComponent>(IComponentSerializer<TComponent> serializer)
            where TComponent : struct
        {
            _registry.Register(serializer);
        }

        /// <summary>
        /// Registers a component serializer using delegate functions.
        /// </summary>
        public void RegisterSerializer<TComponent>(
            Func<TComponent, object> serialize,
            Func<object, TComponent> deserialize)
            where TComponent : struct
        {
            _registry.Register(serialize, deserialize);
        }

        #endregion

        #region Private Methods

        private void SaveToFile(string path, SaveFileData data)
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var bytes = Encoding.UTF8.GetBytes(json);

            using var fileStream = File.Create(path);
            using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
            gzipStream.Write(bytes, 0, bytes.Length);
        }

        private SaveFileData LoadFromFile(string path)
        {
            using var fileStream = File.OpenRead(path);
            using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
            using var memoryStream = new MemoryStream();

            gzipStream.CopyTo(memoryStream);
            var json = Encoding.UTF8.GetString(memoryStream.ToArray());

            return JsonSerializer.Deserialize<SaveFileData>(json, _jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize save file");
        }

        private void SaveMetadataToFile(string path, SaveMetadata metadata)
        {
            var json = JsonSerializer.Serialize(metadata, _jsonOptions);
            File.WriteAllText(path, json);
        }

        private bool VerifySaveFile(string path)
        {
            try
            {
                var data = LoadFromFile(path);
                return data != null && data.Metadata != null && data.Entities != null;
            }
            catch
            {
                return false;
            }
        }

        private void DestroyAllEntities()
        {
            var query = new QueryDescription();
            var entitiesToDestroy = new List<Entity>();

            _world.Query(in query, (Entity entity) =>
            {
                entitiesToDestroy.Add(entity);
            });

            foreach (var entity in entitiesToDestroy)
            {
                _world.Destroy(entity);
            }
        }

        private string GetSavePath(string slotName) => Path.Combine(_saveDirectory, $"{slotName}.sav");
        private string GetMetadataPath(string slotName) => Path.Combine(_metadataDirectory, $"{slotName}.meta.json");

        private string GetUserDataPath()
        {
            // Use standard user data paths per platform
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "KoboldGame"); // Games can customize this
        }

        #endregion
    }
}
