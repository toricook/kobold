using Arch.Core;
using FluentAssertions;
using Kobold.Core.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Helpers
{
    public static class EntityAssertions
    {
        public static void ShouldHaveComponent<T>(this Entity entity, World world, string because = "")
            where T : struct
        {
            world.Has<T>(entity).Should().BeTrue(because);
        }

        public static void ShouldNotHaveComponent<T>(this Entity entity, World world, string because = "")
            where T : struct
        {
            world.Has<T>(entity).Should().BeFalse(because);
        }

        public static void ShouldBeAlive(this Entity entity, World world, string because = "")
        {
            world.IsAlive(entity).Should().BeTrue(because);
        }

        public static void ShouldBeDestroyed(this Entity entity, World world, string because = "")
        {
            world.IsAlive(entity).Should().BeFalse(because);
        }

        public static T GetComponent<T>(this Entity entity, World world) where T : struct
        {
            return world.Get<T>(entity);
        }

        public static void ShouldBeAt(this Entity entity, World world, Vector2 expectedPosition,
                                       float tolerance = 0.01f)
        {
            var transform = world.Get<Transform>(entity);
            transform.Position.X.Should().BeApproximately(expectedPosition.X, tolerance);
            transform.Position.Y.Should().BeApproximately(expectedPosition.Y, tolerance);
        }
    }
}
