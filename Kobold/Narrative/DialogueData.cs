using System.Collections.Generic;

namespace Kobold.Core.Narrative
{
    /// <summary>
    /// Represents a complete dialogue conversation with multiple nodes.
    /// Loaded from JSON files and used by the DialogueSystem.
    /// </summary>
    public class DialogueData
    {
        /// <summary>
        /// Unique identifier for this dialogue conversation.
        /// Used to reference and load the dialogue from code.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Dictionary of dialogue nodes indexed by their node ID.
        /// The dialogue system navigates between nodes using these IDs.
        /// </summary>
        public Dictionary<string, DialogueNode> Nodes { get; set; } = new Dictionary<string, DialogueNode>();
    }

    /// <summary>
    /// Represents a single node in a dialogue conversation.
    /// Contains the text to display and choices available to the player.
    /// </summary>
    public class DialogueNode
    {
        /// <summary>
        /// Name of the character speaking this line.
        /// Can be null for narration or system messages.
        /// </summary>
        public string? Speaker { get; set; }

        /// <summary>
        /// The text content of this dialogue node.
        /// This is what will be displayed to the player.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// List of choices available to the player at this node.
        /// Empty list means end of conversation.
        /// </summary>
        public List<DialogueChoice> Choices { get; set; } = new List<DialogueChoice>();
    }

    /// <summary>
    /// Represents a dialogue choice that the player can select.
    /// Includes the choice text, next node, and optional conditions/effects.
    /// </summary>
    public class DialogueChoice
    {
        /// <summary>
        /// The text displayed for this choice option.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the next dialogue node to show when this choice is selected.
        /// Null, empty, or "end" means the conversation will end.
        /// </summary>
        public string? Next { get; set; }

        /// <summary>
        /// Optional condition that must be met for this choice to be available.
        /// If null, the choice is always available.
        /// </summary>
        public DialogueCondition? Condition { get; set; }

        /// <summary>
        /// Optional flags to set when this choice is selected.
        /// Used to track player decisions and affect future dialogue/gameplay.
        /// </summary>
        public Dictionary<string, object>? SetFlags { get; set; }
    }

    /// <summary>
    /// Represents a condition that must be met for a dialogue choice to be available.
    /// Conditions check against story flags stored on the player entity.
    /// </summary>
    public class DialogueCondition
    {
        /// <summary>
        /// The name of the story flag to check.
        /// </summary>
        public string Flag { get; set; } = string.Empty;

        /// <summary>
        /// The value the flag must have for the condition to pass.
        /// Comparison is done using object.Equals().
        /// </summary>
        public object? Value { get; set; }
    }
}
