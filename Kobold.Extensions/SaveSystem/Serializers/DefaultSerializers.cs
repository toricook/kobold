using System.Numerics;
using Kobold.Core.Components;

namespace Kobold.Extensions.SaveSystem.Serializers
{
    /// <summary>
    /// Provides default serializers for common Kobold components.
    /// These are automatically registered by SaveManager to provide out-of-the-box
    /// serialization support for core components.
    /// </summary>
    public static class DefaultSerializers
    {
        /// <summary>
        /// Registers all default component serializers.
        /// </summary>
        /// <param name="registry">Component serializer registry</param>
        public static void RegisterAll(ComponentSerializerRegistry registry)
        {
            RegisterTransform(registry);
            RegisterVelocity(registry);
            RegisterPlayerControlled(registry);
        }

        /// <summary>
        /// Registers the Transform component serializer.
        /// Serializes Position, Rotation, and Scale as anonymous objects.
        /// </summary>
        private static void RegisterTransform(ComponentSerializerRegistry registry)
        {
            registry.Register<Transform>(
                serialize: (t) => new
                {
                    Position = new { X = t.Position.X, Y = t.Position.Y },
                    Rotation = t.Rotation,
                    Scale = new { X = t.Scale.X, Y = t.Scale.Y }
                },
                deserialize: (data) =>
                {
                    dynamic d = data;
                    return new Transform(
                        new Vector2((float)d.Position.X, (float)d.Position.Y),
                        (float)d.Rotation,
                        new Vector2((float)d.Scale.X, (float)d.Scale.Y)
                    );
                }
            );
        }

        /// <summary>
        /// Registers the Velocity component serializer.
        /// Serializes the velocity Vector2 as an anonymous object.
        /// </summary>
        private static void RegisterVelocity(ComponentSerializerRegistry registry)
        {
            registry.Register<Velocity>(
                serialize: (v) => new
                {
                    Value = new { X = v.Value.X, Y = v.Value.Y }
                },
                deserialize: (data) =>
                {
                    dynamic d = data;
                    return new Velocity(new Vector2((float)d.Value.X, (float)d.Value.Y));
                }
            );
        }

        /// <summary>
        /// Registers the PlayerControlled component serializer.
        /// This is a marker component with no data, so we just serialize an empty object.
        /// </summary>
        private static void RegisterPlayerControlled(ComponentSerializerRegistry registry)
        {
            registry.Register<PlayerControlled>(
                serialize: (p) => new { }, // Empty object
                deserialize: (data) => new PlayerControlled()
            );
        }

        // Additional serializers can be added here as needed:
        // - Physics
        // - BoxCollider
        // - Animation
        // - etc.
        //
        // Example for a hypothetical Health component:
        // registry.Register<Health>(
        //     serialize: (h) => new { Current = h.Current, Max = h.Max },
        //     deserialize: (data) =>
        //     {
        //         dynamic d = data;
        //         return new Health { Current = (int)d.Current, Max = (int)d.Max };
        //     }
        // );
    }
}
