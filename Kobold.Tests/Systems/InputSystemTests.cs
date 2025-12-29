using FluentAssertions;
using Kobold.Core.Abstractions.Input;
using Kobold.Core.Components;
using Kobold.Core.Systems;
using NSubstitute;
using NUnit.Framework;
using System.Numerics;
using Tests.Helpers;

namespace Tests.Systems
{
    [TestFixture]
    public class InputSystemTests
    {
        private TestWorld _testWorld;
        private IInputManager _inputManager;
        private InputSystem _inputSystem;

        [SetUp]
        public void SetUp()
        {
            _testWorld = new TestWorld();
            _inputManager = Substitute.For<IInputManager>();
            _inputSystem = new InputSystem(_inputManager, _testWorld.World);
        }

        [TearDown]
        public void TearDown()
        {
            _testWorld.Dispose();
        }

        [Test]
        public void Update_WithUpKeyPressed_MovesPlayerUp()
        {
            // Arrange
            var player = _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new Velocity(Vector2.Zero),
                new PlayerControlled(
                    speed: 100f,
                    up: KeyCode.W,
                    down: KeyCode.S,
                    left: KeyCode.A,
                    right: KeyCode.D,
                    altUp: KeyCode.Up,
                    altDown: KeyCode.Down,
                    altLeft: KeyCode.Left,
                    altRight: KeyCode.Right
                )
            );

            _inputManager.IsKeyDown(KeyCode.W).Returns(true);

            // Act
            _inputSystem.Update(0.016f);

            // Assert
            var velocity = player.GetComponent<Velocity>(_testWorld.World);
            velocity.Value.Should().Be(new Vector2(0, -100), "pressing up should move player upward");
        }

        [Test]
        public void Update_WithDownKeyPressed_MovesPlayerDown()
        {
            // Arrange
            var player = _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new Velocity(Vector2.Zero),
                new PlayerControlled(
                    speed: 100f,
                    up: KeyCode.W,
                    down: KeyCode.S,
                    left: KeyCode.A,
                    right: KeyCode.D,
                    altUp: KeyCode.Up,
                    altDown: KeyCode.Down,
                    altLeft: KeyCode.Left,
                    altRight: KeyCode.Right
                )
            );

            _inputManager.IsKeyDown(KeyCode.S).Returns(true);

            // Act
            _inputSystem.Update(0.016f);

            // Assert
            var velocity = player.GetComponent<Velocity>(_testWorld.World);
            velocity.Value.Should().Be(new Vector2(0, 100), "pressing down should move player downward");
        }

        [Test]
        public void Update_WithLeftKeyPressed_MovesPlayerLeft()
        {
            // Arrange
            var player = _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new Velocity(Vector2.Zero),
                new PlayerControlled(
                    speed: 100f,
                    up: KeyCode.W,
                    down: KeyCode.S,
                    left: KeyCode.A,
                    right: KeyCode.D,
                    altUp: KeyCode.Up,
                    altDown: KeyCode.Down,
                    altLeft: KeyCode.Left,
                    altRight: KeyCode.Right
                )
            );

            _inputManager.IsKeyDown(KeyCode.A).Returns(true);

            // Act
            _inputSystem.Update(0.016f);

            // Assert
            var velocity = player.GetComponent<Velocity>(_testWorld.World);
            velocity.Value.Should().Be(new Vector2(-100, 0), "pressing left should move player leftward");
        }

        [Test]
        public void Update_WithRightKeyPressed_MovesPlayerRight()
        {
            // Arrange
            var player = _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new Velocity(Vector2.Zero),
                new PlayerControlled(
                    speed: 100f,
                    up: KeyCode.W,
                    down: KeyCode.S,
                    left: KeyCode.A,
                    right: KeyCode.D,
                    altUp: KeyCode.Up,
                    altDown: KeyCode.Down,
                    altLeft: KeyCode.Left,
                    altRight: KeyCode.Right
                )
            );

            _inputManager.IsKeyDown(KeyCode.D).Returns(true);

            // Act
            _inputSystem.Update(0.016f);

            // Assert
            var velocity = player.GetComponent<Velocity>(_testWorld.World);
            velocity.Value.Should().Be(new Vector2(100, 0), "pressing right should move player rightward");
        }

        [Test]
        public void Update_WithNoKeysPressed_StopsPlayer()
        {
            // Arrange
            var player = _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new Velocity(new Vector2(50, 50)), // Initially moving
                new PlayerControlled(
                    speed: 100f,
                    up: KeyCode.W,
                    down: KeyCode.S,
                    left: KeyCode.A,
                    right: KeyCode.D,
                    altUp: KeyCode.Up,
                    altDown: KeyCode.Down,
                    altLeft: KeyCode.Left,
                    altRight: KeyCode.Right
                )
            );

            // No keys pressed (all return false by default)

            // Act
            _inputSystem.Update(0.016f);

            // Assert
            var velocity = player.GetComponent<Velocity>(_testWorld.World);
            velocity.Value.Should().Be(Vector2.Zero, "no keys pressed should stop movement");
        }

        [Test]
        public void Update_WithDiagonalMovement_CombinesVelocities()
        {
            // Arrange
            var player = _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new Velocity(Vector2.Zero),
                new PlayerControlled(
                    speed: 100f,
                    up: KeyCode.W,
                    down: KeyCode.S,
                    left: KeyCode.A,
                    right: KeyCode.D,
                    altUp: KeyCode.Up,
                    altDown: KeyCode.Down,
                    altLeft: KeyCode.Left,
                    altRight: KeyCode.Right
                )
            );

            _inputManager.IsKeyDown(KeyCode.W).Returns(true); // Up
            _inputManager.IsKeyDown(KeyCode.D).Returns(true); // Right

            // Act
            _inputSystem.Update(0.016f);

            // Assert
            var velocity = player.GetComponent<Velocity>(_testWorld.World);
            velocity.Value.Should().Be(new Vector2(100, -100), "diagonal movement should combine velocities");
        }

        [Test]
        public void Update_WithAlternateKeys_MovesPlayer()
        {
            // Arrange
            var player = _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new Velocity(Vector2.Zero),
                new PlayerControlled(
                    speed: 100f,
                    up: KeyCode.W,
                    down: KeyCode.S,
                    left: KeyCode.A,
                    right: KeyCode.D,
                    altUp: KeyCode.Up,
                    altDown: KeyCode.Down,
                    altLeft: KeyCode.Left,
                    altRight: KeyCode.Right
                )
            );

            _inputManager.IsKeyDown(KeyCode.Up).Returns(true); // Alternate up key

            // Act
            _inputSystem.Update(0.016f);

            // Assert
            var velocity = player.GetComponent<Velocity>(_testWorld.World);
            velocity.Value.Should().Be(new Vector2(0, -100), "alternate keys should work");
        }

        [Test]
        public void Update_WithVerticalOnly_IgnoresHorizontalInput()
        {
            // Arrange
            var player = _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new Velocity(Vector2.Zero),
                new PlayerControlled(
                    speed: 100f,
                    verticalOnly: true,
                    up: KeyCode.W,
                    down: KeyCode.S,
                    left: KeyCode.A,
                    right: KeyCode.D,
                    altUp: KeyCode.Up,
                    altDown: KeyCode.Down,
                    altLeft: KeyCode.Left,
                    altRight: KeyCode.Right
                )
            );

            _inputManager.IsKeyDown(KeyCode.W).Returns(true); // Up
            _inputManager.IsKeyDown(KeyCode.D).Returns(true); // Right (should be ignored)

            // Act
            _inputSystem.Update(0.016f);

            // Assert
            var velocity = player.GetComponent<Velocity>(_testWorld.World);
            velocity.Value.Should().Be(new Vector2(0, -100), "vertical only should ignore horizontal input");
        }

        [Test]
        public void Update_WithHorizontalOnly_IgnoresVerticalInput()
        {
            // Arrange
            var player = _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new Velocity(Vector2.Zero),
                new PlayerControlled(
                    speed: 100f,
                    horizontalOnly: true,
                    up: KeyCode.W,
                    down: KeyCode.S,
                    left: KeyCode.A,
                    right: KeyCode.D,
                    altUp: KeyCode.Up,
                    altDown: KeyCode.Down,
                    altLeft: KeyCode.Left,
                    altRight: KeyCode.Right
                )
            );

            _inputManager.IsKeyDown(KeyCode.W).Returns(true); // Up (should be ignored)
            _inputManager.IsKeyDown(KeyCode.D).Returns(true); // Right

            // Act
            _inputSystem.Update(0.016f);

            // Assert
            var velocity = player.GetComponent<Velocity>(_testWorld.World);
            velocity.Value.Should().Be(new Vector2(100, 0), "horizontal only should ignore vertical input");
        }

        [Test]
        public void Update_WithMultiplePlayers_HandlesEachIndependently()
        {
            // Arrange
            var player1 = _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new Velocity(Vector2.Zero),
                new PlayerControlled(
                    speed: 100f,
                    up: KeyCode.W,
                    down: KeyCode.S,
                    left: KeyCode.A,
                    right: KeyCode.D,
                    altUp: KeyCode.P, // Use P to avoid conflicts
                    altDown: KeyCode.P,
                    altLeft: KeyCode.P,
                    altRight: KeyCode.P
                )
            );

            var player2 = _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new Velocity(Vector2.Zero),
                new PlayerControlled(
                    speed: 200f,
                    up: KeyCode.Up,
                    down: KeyCode.Down,
                    left: KeyCode.Left,
                    right: KeyCode.Right,
                    altUp: KeyCode.P, // Use P to avoid conflicts
                    altDown: KeyCode.P,
                    altLeft: KeyCode.P,
                    altRight: KeyCode.P
                )
            );

            _inputManager.IsKeyDown(KeyCode.W).Returns(true); // Player 1 up
            _inputManager.IsKeyDown(KeyCode.Right).Returns(true); // Player 2 right

            // Act
            _inputSystem.Update(0.016f);

            // Assert
            var velocity1 = player1.GetComponent<Velocity>(_testWorld.World);
            var velocity2 = player2.GetComponent<Velocity>(_testWorld.World);

            velocity1.Value.Should().Be(new Vector2(0, -100), "player 1 moves at its speed");
            velocity2.Value.Should().Be(new Vector2(200, 0), "player 2 moves at its speed");
        }
    }
}
