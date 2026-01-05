using Arch.Core;
using System.Collections.Generic;
using Kobold.Core.Events;
using Kobold.Core.Narrative;

namespace Kobold.Extensions.Narrative.Events
{
    /// <summary>
    /// Event published when a dialogue conversation begins.
    /// The game should pause and display the dialogue UI when this event is received.
    /// </summary>
    public class DialogueStartedEvent : BaseEvent
    {
        /// <summary>
        /// The unique ID of the dialogue conversation that has started.
        /// </summary>
        public string DialogueId { get; set; } = string.Empty;

        /// <summary>
        /// The entity that initiated the dialogue (e.g., NPC being talked to).
        /// Can be Entity.Null if there's no specific speaker entity.
        /// </summary>
        public Entity Speaker { get; set; }

        public DialogueStartedEvent() { }

        public DialogueStartedEvent(string dialogueId, Entity speaker)
        {
            DialogueId = dialogueId;
            Speaker = speaker;
        }
    }

    /// <summary>
    /// Event published when a new dialogue node is displayed.
    /// UI systems should update the displayed text and choices when this event is received.
    /// </summary>
    public class DialogueNodeEvent : BaseEvent
    {
        /// <summary>
        /// Name of the character speaking this line.
        /// Null for narration or system messages.
        /// </summary>
        public string? Speaker { get; set; }

        /// <summary>
        /// The text content to display to the player.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// List of choices available to the player.
        /// This list has already been filtered based on conditions - all choices are valid.
        /// Empty list means the conversation will end when the player advances.
        /// </summary>
        public List<DialogueChoice> AvailableChoices { get; set; } = new List<DialogueChoice>();

        public DialogueNodeEvent() { }

        public DialogueNodeEvent(string? speaker, string text, List<DialogueChoice> availableChoices)
        {
            Speaker = speaker;
            Text = text;
            AvailableChoices = availableChoices;
        }
    }

    /// <summary>
    /// Event published when the player selects a dialogue choice.
    /// The DialogueSystem listens for this event to advance the conversation.
    /// </summary>
    public class DialogueChoiceSelectedEvent : BaseEvent
    {
        /// <summary>
        /// The index of the choice that was selected (0-based).
        /// Corresponds to the index in the AvailableChoices list from DialogueNodeEvent.
        /// </summary>
        public int ChoiceIndex { get; set; }

        /// <summary>
        /// The ID of the next node to navigate to.
        /// Null or "end" means the conversation will end.
        /// </summary>
        public string? NextNodeId { get; set; }

        public DialogueChoiceSelectedEvent() { }

        public DialogueChoiceSelectedEvent(int choiceIndex, string? nextNodeId)
        {
            ChoiceIndex = choiceIndex;
            NextNodeId = nextNodeId;
        }
    }

    /// <summary>
    /// Event published when a dialogue conversation ends.
    /// The game should resume and hide the dialogue UI when this event is received.
    /// </summary>
    public class DialogueEndedEvent : BaseEvent
    {
        /// <summary>
        /// The unique ID of the dialogue conversation that has ended.
        /// </summary>
        public string DialogueId { get; set; } = string.Empty;

        public DialogueEndedEvent() { }

        public DialogueEndedEvent(string dialogueId)
        {
            DialogueId = dialogueId;
        }
    }
}
