using Arch.Core;
using Kobold.Core.Events;
using Kobold.Core.Systems;
using Asteroids.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kobold.Core.Abstractions.Engine;
using Kobold.Core.Components;

namespace Asteroids.Systems
{
    /// <summary>
    /// Handles scoring, high scores, and score display updates
    /// </summary>
    public class ScoringSystem : ISystem, IEventHandler<AsteroidDestroyedEvent>, IEventHandler<GameRestartEvent>
    {
        private readonly World _world;
        private readonly EventBus _eventBus;
        private readonly ScoringConfig _config;

        private int _currentScore = 0;
        private int _highScore = 0;
        private Entity _scoreDisplayEntity = Entity.Null;
        private Entity _highScoreDisplayEntity = Entity.Null;

        public ScoringSystem(World world, EventBus eventBus, ScoringConfig config = null)
        {
            _world = world;
            _eventBus = eventBus;
            _config = config ?? new ScoringConfig();

            // Subscribe to events
            _eventBus.Subscribe<AsteroidDestroyedEvent>(this);
            _eventBus.Subscribe<GameRestartEvent>(this);

            // Load high score (in a real game, this would be from a file/registry)
            LoadHighScore();
        }

        public void Update(float deltaTime)
        {
            // This system is mostly event-driven, but we could add
            // time-based scoring bonuses here if desired
        }

        public void Handle(AsteroidDestroyedEvent eventData)
        {
            // Add base score for the asteroid
            int scoreToAdd = eventData.ScoreValue;

            // Apply any multipliers or bonuses
            scoreToAdd = ApplyScoreMultipliers(scoreToAdd, eventData);

            // Update score
            AddScore(scoreToAdd);

            // Publish score changed event
            _eventBus.Publish(new ScoreChangedEvent(_currentScore, scoreToAdd, eventData.Position));
        }

        public void Handle(GameRestartEvent eventData)
        {
            ResetScore();
        }

        private int ApplyScoreMultipliers(int baseScore, AsteroidDestroyedEvent eventData)
        {
            int finalScore = baseScore;

            // Size-based multiplier (smaller asteroids worth more)
            switch (eventData.Size)
            {
                case Components.AsteroidSize.Large:
                    finalScore = (int)(baseScore * _config.LargeAsteroidMultiplier);
                    break;
                case Components.AsteroidSize.Medium:
                    finalScore = (int)(baseScore * _config.MediumAsteroidMultiplier);
                    break;
                case Components.AsteroidSize.Small:
                    finalScore = (int)(baseScore * _config.SmallAsteroidMultiplier);
                    break;
            }

            // Could add wave multiplier, combo multiplier, etc. here
            // finalScore = (int)(finalScore * GetWaveMultiplier());

            return finalScore;
        }

        private void AddScore(int points)
        {
            _currentScore += points;

            // Check for new high score
            if (_currentScore > _highScore)
            {
                _highScore = _currentScore;
                _eventBus.Publish(new HighScoreEvent(_highScore, _currentScore - _highScore));
                SaveHighScore();
            }

            // Update score display
            UpdateScoreDisplay();
        }

        private void ResetScore()
        {
            _currentScore = 0;
            UpdateScoreDisplay();
        }

        private void UpdateScoreDisplay()
        {
            // Find and update score display entity
            if (_scoreDisplayEntity == Entity.Null || !_world.IsAlive(_scoreDisplayEntity))
            {
                FindScoreDisplayEntities();
            }

            // Update current score display
            if (_scoreDisplayEntity != Entity.Null && _world.IsAlive(_scoreDisplayEntity))
            {
                if (_world.Has<TextRenderer>(_scoreDisplayEntity))
                {
                    ref var textRenderer = ref _world.Get<TextRenderer>(_scoreDisplayEntity);
                    textRenderer.Text = $"SCORE: {_currentScore}";
                }
            }

            // Update high score display
            if (_highScoreDisplayEntity != Entity.Null && _world.IsAlive(_highScoreDisplayEntity))
            {
                if (_world.Has<TextRenderer>(_highScoreDisplayEntity))
                {
                    ref var textRenderer = ref _world.Get<TextRenderer>(_highScoreDisplayEntity);
                    textRenderer.Text = $"HIGH: {_highScore}";
                }
            }
        }

        private void FindScoreDisplayEntities()
        {
            // Find entities with TextRenderer that contain score text
            var textQuery = new QueryDescription().WithAll<TextRenderer>();

            _world.Query(in textQuery, (Entity entity, ref TextRenderer textRenderer) =>
            {
                if (textRenderer.Text.StartsWith("SCORE:"))
                {
                    _scoreDisplayEntity = entity;
                }
                else if (textRenderer.Text.StartsWith("HIGH:"))
                {
                    _highScoreDisplayEntity = entity;
                }
            });
        }

        private void LoadHighScore()
        {
            // In a real game, load from PlayerPrefs, file, or registry
            // For now, just use a default value
            _highScore = _config.DefaultHighScore;
        }

        private void SaveHighScore()
        {
            // In a real game, save to PlayerPrefs, file, or registry
            // For now, just keep it in memory
            Console.WriteLine($"New High Score: {_highScore}");
        }

        /// <summary>
        /// Get current scoring information
        /// </summary>
        public ScoreInfo GetScoreInfo()
        {
            return new ScoreInfo
            {
                CurrentScore = _currentScore,
                HighScore = _highScore,
                IsNewHighScore = _currentScore == _highScore && _currentScore > 0
            };
        }

        /// <summary>
        /// Manually add score (for testing or special events)
        /// </summary>
        public void AddBonusScore(int points, string reason = "Bonus")
        {
            AddScore(points);
            _eventBus.Publish(new BonusScoreEvent(points, reason));
        }

        /// <summary>
        /// Set the entities that display score information
        /// </summary>
        public void SetScoreDisplayEntities(Entity scoreEntity, Entity highScoreEntity = default)
        {
            _scoreDisplayEntity = scoreEntity;
            _highScoreDisplayEntity = highScoreEntity;
            UpdateScoreDisplay();
        }
    }

    /// <summary>
    /// Configuration for scoring behavior
    /// </summary>
    public class ScoringConfig
    {
        // Score multipliers by asteroid size
        public float LargeAsteroidMultiplier { get; set; } = 1.0f;
        public float MediumAsteroidMultiplier { get; set; } = 1.0f;
        public float SmallAsteroidMultiplier { get; set; } = 1.0f;

        // Bonus scoring
        public int WaveClearBonus { get; set; } = 1000;
        public float WaveMultiplier { get; set; } = 1.1f; // 10% increase per wave

        // High score settings
        public int DefaultHighScore { get; set; } = 10000;
        public bool ShowHighScoreEffects { get; set; } = true;
    }

    /// <summary>
    /// Information about current scoring state
    /// </summary>
    public struct ScoreInfo
    {
        public int CurrentScore;
        public int HighScore;
        public bool IsNewHighScore;

        public override string ToString()
        {
            return $"Score: {CurrentScore:N0}, High: {HighScore:N0}, New High: {IsNewHighScore}";
        }
    }
}