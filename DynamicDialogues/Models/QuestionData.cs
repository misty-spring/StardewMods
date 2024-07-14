// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace DynamicDialogues.Models;

///<summary>Questions added by player, which might lead to events or quests.</summary>
public class QuestionData
{
    public string Question { get; set; }
    public string Answer { get; set; }
    public int MaxTimesAsked { get; set; } //0 meaning forever avaiable
    public string Location { get; set; } = "any"; //if avaiable only in a specific location
    public int From { get; set; } = 610; //from this hour
    public int To { get; set; } = 2550; //until this hour
    public string QuestToStart { get; set; }
    public PlayerConditions Conditions { get; set; } = new();
    public string TriggerAction { get; set; }
}
