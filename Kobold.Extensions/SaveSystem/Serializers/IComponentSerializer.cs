namespace Kobold.Extensions.SaveSystem.Serializers
{
    /// <summary>
    /// Interface for component serializers.
    /// Each component type can have a custom serializer that defines how to convert
    /// the component to/from a serializable format (typically anonymous objects).
    /// </summary>
    /// <typeparam name="TComponent">Component type to serialize</typeparam>
    public interface IComponentSerializer<TComponent> where TComponent : struct
    {
        /// <summary>
        /// Serializes a component to an object that System.Text.Json can handle.
        /// Typically returns an anonymous object with the component's data.
        ///
        /// Example:
        /// return new { Position = new { X = component.Position.X, Y = component.Position.Y } };
        /// </summary>
        /// <param name="component">Component instance to serialize</param>
        /// <returns>Serializable object (usually anonymous)</returns>
        object Serialize(TComponent component);

        /// <summary>
        /// Deserializes an object back into a component.
        /// The data object is typically a dynamic object deserialized from JSON.
        ///
        /// Example:
        /// dynamic d = data;
        /// return new Transform(new Vector2(d.Position.X, d.Position.Y));
        /// </summary>
        /// <param name="data">Deserialized data object</param>
        /// <returns>Reconstructed component instance</returns>
        TComponent Deserialize(object data);
    }

    /// <summary>
    /// Simple implementation of IComponentSerializer using delegates.
    /// Allows easy registration of serializers using lambda functions.
    /// </summary>
    /// <typeparam name="TComponent">Component type</typeparam>
    public class DelegateComponentSerializer<TComponent> : IComponentSerializer<TComponent>
        where TComponent : struct
    {
        private readonly System.Func<TComponent, object> _serialize;
        private readonly System.Func<object, TComponent> _deserialize;

        public DelegateComponentSerializer(
            System.Func<TComponent, object> serialize,
            System.Func<object, TComponent> deserialize)
        {
            _serialize = serialize;
            _deserialize = deserialize;
        }

        public object Serialize(TComponent component) => _serialize(component);
        public TComponent Deserialize(object data) => _deserialize(data);
    }
}
