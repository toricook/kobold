using Arch.Core;
using Kobold.Core.Components;
using Kobold.Core.Events;
using Kobold.Core.Systems;
using Pong.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pong.Systems
{
    public class ScoreSystem : ISystem, IEventHandler<PlayerScoredEvent>
    {
        private readonly World _world;
        private readonly EventBus _eventBus;
        private Score _currentScore;
        private static readonly int _winningScore = 10;

        public ScoreSystem(World world, EventBus eventBus)
        {
            _world = world;
            _eventBus = eventBus;
            _currentScore = new Score();
            eventBus.Subscribe<PlayerScoredEvent>(this);
        }

        public void Handle(PlayerScoredEvent eventData)
        {
            if (eventData.PlayerId == 1)
            {
                _currentScore = new Score(_currentScore.PlayerScore + 1, _currentScore.AIScore);
            }
            else if (eventData.PlayerId == 2)
            {
                _currentScore = new Score(_currentScore.PlayerScore, _currentScore.AIScore + 1);
            }

            UpdateScoreDisplay();
        }

        private void UpdateScoreDisplay()
        {
            var scoreQuery = new QueryDescription().WithAll<TextRenderer>().WithNone<Paddle, Ball>();

            _world.Query(in scoreQuery, (ref TextRenderer textRenderer) =>
            {
                textRenderer.Text = $"{_currentScore.PlayerScore} - {_currentScore.AIScore}";
            });
        }

        private void CheckForGameOver()
        {
            if (_currentScore.PlayerScore >= _winningScore)
            {
                _eventBus.Publish(new GameOverEvent(1, _currentScore.PlayerScore, _currentScore.AIScore));
            }
            else if (_currentScore.AIScore >= _winningScore)
            {
                _eventBus.Publish(new GameOverEvent(2, _currentScore.AIScore, _currentScore.PlayerScore));
            }
        }

        public Score GetCurrentScore() => _currentScore;

        public void ResetScore()
        {
            _currentScore = new Score();
            UpdateScoreDisplay();
        }

        public void Update(float deltaTime)
        {
            // pass
        }
    }
}
