// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace DynamicDialogues.Models;

/// <summary>
/// A class used for Dialogues with special commands (e.g. jump, emote, etc.)
/// </summary>
public class DialogueData
{
    public int Time { get; set; } = -1;  //time to add dialogue at, mut. exclusive w/ from-to
    public int From { get; set; } = 600; //from this hour
    public int To { get; set; } = 2600; //until this hour
    public string Location { get; set; } = "any";  //location npc has to be in

    public string Dialogue { get; set; } //the dialogue
    public bool ClearOnMove { get; set; } //if to clear dialogue on move
    public bool Override { get; set; } //if to delete previous dialogues
    public bool Immediate { get; set; } // will print dialogue right away if NPC is in location
    public bool Force { get; set; } // if Immediate, prints dialogue regardless of location
    //public bool ApplyWhenMoving { get; set; } = false;

    public bool IsBubble { get; set; } //show text overhead instead

    public string FaceDirection { get; set; } //string to change facing to
    public bool Jump { get; set; } //makes npc jump when addition is placed
    public int Shake { get; set; } = -1; //shake for x milliseconds
    public int Emote { get; set; } = -1; //emote int (if allowed)

    public AnimationData Animation { get; set; } = new(); //animation to play, if any
    public PlayerConditions Conditions { get; set; } = new();
    public string TriggerAction { get; set; }
}

