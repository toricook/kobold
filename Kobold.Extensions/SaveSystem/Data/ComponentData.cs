namespace Kobold.Extensions.SaveSystem.Data
{
    /// <summary>
    /// Represents serialized component data for saving.
    /// Contains the component type name and its serialized state.
    /// </summary>
    public class ComponentData
    {
        /// <summary>
        /// Fully qualified type name of the component (e.g., "Kobold.Core.Components.Transform")
        /// Used to reconstruct the correct component type during deserialization.
        /// </summary>
        public string TypeName { get; set; } = string.Empty;

        /// <summary>
        /// Serialized component data as an object.
        /// Typically an anonymous object created by the component serializer.
        /// System.Text.Json will serialize this to a JSON object.
        /// </summary>
        public object? Data { get; set; }
    }
}
