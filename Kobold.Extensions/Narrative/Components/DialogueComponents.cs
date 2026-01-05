using Arch.Core;
using System.Collections.Generic;
using Kobold.Core.Narrative;

namespace Kobold.Extensions.Narrative.Components
{
    /// <summary>
    /// Component that stores story flags for branching dialogue and narrative state.
    /// Typically attached to the player entity.
    /// Flags can be any value type (bool, int, string, etc.) and are used for:
    /// - Tracking player choices and decisions
    /// - Controlling which dialogue options are available
    /// - Determining quest progression
    /// - Storing narrative state
    /// </summary>
    /// <example>
    /// // Set a flag when player makes a choice
    /// ref var flags = ref world.Get&lt;StoryFlags&gt;(playerEntity);
    /// flags.Flags["met_guard"] = true;
    /// flags.Flags["player_reputation"] = 10;
    ///
    /// // Check a flag in dialogue conditions
    /// if (flags.Flags.TryGetValue("quest_started", out var value) &amp;&amp; value is true)
    /// {
    ///     // Show quest-related dialogue
    /// }
    /// </example>
    public struct StoryFlags
    {
        /// <summary>
        /// Dictionary of story flags, where keys are flag names and values can be any type.
        /// Commonly used types: bool (yes/no), int (counters), string (states).
        /// </summary>
        public Dictionary<string, object> Flags;

        public StoryFlags()
        {
            Flags = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Component that tracks the current active dialogue conversation.
    /// Created when dialogue starts and destroyed when dialogue ends.
    /// Only one dialogue can be active at a time - there should only be one entity with this component.
    /// </summary>
    /// <remarks>
    /// This component is managed by the DialogueSystem and should not be created manually.
    /// The system creates it when dialogue starts and removes it when dialogue ends.
    /// </remarks>
    public struct DialogueState
    {
        /// <summary>
        /// The unique ID of the currently active dialogue conversation.
        /// </summary>
        public string DialogueId;

        /// <summary>
        /// The ID of the current node being displayed in the dialogue.
        /// </summary>
        public string CurrentNodeId;

        /// <summary>
        /// Reference to the loaded dialogue data containing all nodes and choices.
        /// </summary>
        public DialogueData Data;

        /// <summary>
        /// Optional reference to the entity that initiated the dialogue (e.g., the NPC being talked to).
        /// Can be Entity.Null if the dialogue doesn't have a specific speaker entity.
        /// </summary>
        public Entity Speaker;

        public DialogueState(string dialogueId, string currentNodeId, DialogueData data, Entity speaker)
        {
            DialogueId = dialogueId;
            CurrentNodeId = currentNodeId;
            Data = data;
            Speaker = speaker;
        }
    }

    /// <summary>
    /// Component that marks an entity as capable of starting a dialogue conversation.
    /// Commonly attached to NPCs, interactive objects, or trigger zones.
    /// When the player interacts with or enters this entity, the dialogue system will start the conversation.
    /// </summary>
    /// <example>
    /// // Create an NPC that starts a conversation when the player talks to them
    /// var npc = world.Create&lt;Transform, DialogueTrigger&gt;();
    /// world.Set(npc, new DialogueTrigger
    /// {
    ///     DialogueId = "guard_conversation",
    ///     RequireInteraction = true  // Player must press a button to talk
    /// });
    ///
    /// // Create a zone that starts dialogue automatically when entered
    /// var trigger = world.Create&lt;Transform, DialogueTrigger, Trigger&gt;();
    /// world.Set(trigger, new DialogueTrigger
    /// {
    ///     DialogueId = "area_narration",
    ///     RequireInteraction = false  // Starts automatically
    /// });
    /// </example>
    public struct DialogueTrigger
    {
        /// <summary>
        /// The unique ID of the dialogue to start when this trigger is activated.
        /// Must match a dialogue file loaded by the AssetManager.
        /// </summary>
        public string DialogueId;

        /// <summary>
        /// If true, the player must press an interact button to start the dialogue.
        /// If false, dialogue starts automatically when conditions are met (e.g., entering a zone).
        /// </summary>
        public bool RequireInteraction;

        public DialogueTrigger(string dialogueId, bool requireInteraction = true)
        {
            DialogueId = dialogueId;
            RequireInteraction = requireInteraction;
        }
    }
}
