using Arch.Core;
using Kobold.Core.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Factories
{
    /// <summary>
    /// Factory for creating common entites
    /// </summary>
    public class EntityFactory
    {
        private readonly World _world;

        public EntityFactory(World world)
        {
            _world = world;
        }

        public Entity CreateRectangle(Vector2 position, Vector2 size, Color color)
        {
            return _world.Create(
                new Transform(position),
                new RectangleRenderer(size, color),
                new BoxCollider(size)
            );
        }

        public Entity CreateMovingRectangle(Vector2 position, Vector2 size, Color color, Vector2 velocity)
        {
            return _world.Create(
                new Transform(position),
                new RectangleRenderer(size, color),
                new BoxCollider(size),
                new Velocity(velocity)
            );
        }

        public Entity CreateText(Vector2 position, string text, Color color, float fontSize = 16f)
        {
            return _world.Create(
                new Transform(position),
                new TextRenderer(text, color, fontSize)
            );
        }
    }
}
