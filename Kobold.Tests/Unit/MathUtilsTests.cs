using FluentAssertions;
using Kobold.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Unit
{
    [TestFixture]
    public class MathUtilsTests
    {
        [TestCase(0f, 0f, ExpectedResult = 0f)]
        [TestCase(MathF.PI, MathF.PI, ExpectedResult = 0f)]
        [TestCase(0f, MathF.PI * 2, ExpectedResult = 0f)]
        [TestCase(0f, MathF.PI, ExpectedResult = MathF.PI)]
        [TestCase(MathF.PI, 0f, ExpectedResult = -MathF.PI)]
        [TestCase(0.1f, MathF.PI + 0.1f, ExpectedResult = -MathF.PI)]
        public float AngleDifference_ReturnsShortestPath(float from, float to)
        {
            return MathUtils.AngleDifference(from, to);
        }

        [Test]
        public void WrapAngle_KeepsAngleInRange()
        {
            // Arrange
            var angles = new[] { -4 * MathF.PI, -MathF.PI, 0f, MathF.PI, 4 * MathF.PI };

            // Act & Assert
            foreach (var angle in angles)
            {
                var wrapped = MathUtils.WrapAngle(angle);
                wrapped.Should().BeGreaterOrEqualTo(-MathF.PI);
                wrapped.Should().BeLessOrEqualTo(MathF.PI);
            }
        }

        [Test]
        public void MoveTowards_StopsAtTarget()
        {
            // Arrange
            var current = 5f;
            var target = 10f;
            var maxDelta = 100f; // More than distance

            // Act
            var result = MathUtils.MoveTowards(current, target, maxDelta);

            // Assert
            result.Should().Be(target);
        }
    }
}
