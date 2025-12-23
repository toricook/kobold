using FluentAssertions;
using Kobold.Core.Components;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Unit
{
    [TestFixture]
    public class CollisionDetectionTests
    {
        [Test]
        public void BoxCollider_Overlaps_DetectsCollision()
        {
            // Arrange
            var box1 = new BoxCollider(new Vector2(20, 20));
            var pos1 = new Vector2(0, 0);

            var box2 = new BoxCollider(new Vector2(20, 20));
            var pos2 = new Vector2(10, 10); // Overlapping

            // Act
            var collides = box1.Overlaps(pos1, box2, pos2);

            // Assert
            collides.Should().BeTrue();
        }

        [Test]
        public void BoxCollider_NoOverlap_NoCollision()
        {
            // Arrange
            var box1 = new BoxCollider(new Vector2(20, 20));
            var pos1 = new Vector2(0, 0);

            var box2 = new BoxCollider(new Vector2(20, 20));
            var pos2 = new Vector2(100, 100); // Far apart

            // Act
            var collides = box1.Overlaps(pos1, box2, pos2);

            // Assert
            collides.Should().BeFalse();
        }

        [Test]
        public void BoxCollider_EdgeTouching_DetectsCollision()
        {
            // Arrange
            var box1 = new BoxCollider(new Vector2(20, 20));
            var pos1 = new Vector2(0, 0);

            var box2 = new BoxCollider(new Vector2(20, 20));
            var pos2 = new Vector2(20, 0); // Exactly touching

            // Act
            var collides = box1.Overlaps(pos1, box2, pos2);

            // Assert
            collides.Should().BeTrue("boxes touching should collide");
        }
    }
}
