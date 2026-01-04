using Kobold.Extensions.Gameplay.Components;
using Kobold.Core.Components;
using Kobold.Core.Systems;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Tests.Helpers;
using FluentAssertions;
using Kobold.Extensions.Destruction.Systems;
using Kobold.Extensions.Destruction.Components;
using Kobold.Extensions.Gameplay.Components;
namespace Tests.Systems
{
    [TestFixture]
    public class DestructionSystemTests
    {
        private TestWorld _testWorld;
        private DestructionSystem _destructionSystem;

        [SetUp]
        public void SetUp()
        {
            _testWorld = new TestWorld();
            _destructionSystem = new DestructionSystem(_testWorld.World, _testWorld.EventBus);
        }

        [TearDown]
        public void TearDown()
        {
            _testWorld.Dispose();
        }

        [Test]
        public void Update_WithExpiredLifetime_DestroysEntity()
        {
            // Arrange
            var entity = _testWorld.World.Create(
                new Transform(Vector2.Zero),
                new Lifetime(1.0f) // 1 second lifetime
            );

            // Act - simulate 1.5 seconds
            _destructionSystem.Update(deltaTime: 1.5f);

            // Assert
            entity.ShouldBeDestroyed(_testWorld.World, "lifetime expired");
        }

        [Test]
        public void Update_WithPendingDestruction_DestroysEntity()
        {
            // Arrange
            var entity = _testWorld.World.Create(
                new Transform(Vector2.Zero)
            );

            DestructionSystem.MarkForDestruction(_testWorld.World, entity, DestructionReason.Manual);

            // Act
            _destructionSystem.Update(deltaTime: 0.016f);

            // Assert
            entity.ShouldBeDestroyed(_testWorld.World, "marked for destruction");
        }

        [Test]
        public void Update_WithDelayedDestruction_WaitsForDelay()
        {
            // Arrange
            var entity = _testWorld.World.Create(
                new Transform(Vector2.Zero)
            );

            DestructionSystem.MarkForDestruction(
                _testWorld.World,
                entity,
                DestructionReason.Manual,
                delay: 1.0f // 1 second delay
            );

            // Act - first frame
            _destructionSystem.Update(deltaTime: 0.5f);

            // Assert - should still be alive
            entity.ShouldBeAlive(_testWorld.World, "delay not expired");

            // Act - second frame
            _destructionSystem.Update(deltaTime: 0.6f);

            // Assert - now destroyed
            entity.ShouldBeDestroyed(_testWorld.World, "delay expired");
        }

        [Test]
        public void Update_PublishesEntityDestroyedEvent()
        {
            // Arrange
            var entity = _testWorld.World.Create(
                new Transform(new Vector2(100, 200)),
                new Lifetime(0.5f)
            );

            // Act
            _destructionSystem.Update(deltaTime: 1.0f);

            // Assert
            var destroyedEvent = _testWorld.GetLastEvent<EntityDestroyedEvent>();
            destroyedEvent.Should().NotBeNull();
            destroyedEvent.Entity.Should().Be(entity);
            destroyedEvent.Reason.Should().Be(DestructionReason.Lifetime);
            destroyedEvent.Position.Should().Be(new Vector2(100, 200));
        }
    }
}
