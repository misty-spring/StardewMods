namespace MistyCore.Models;

public class QuestionTileActionData
{
    public string Condition { get; set; } = "TRUE";
    public string MessageToShowIfConditionsFail { get; set; }
    public bool IsLetter { get; set; } = false;
    public string Question { get; set; }
    public string Yes { get; set; } = "[LocalizedText Strings/Lexicon:QuestionDialogue_Yes]";
    public string No { get; set; } = "[LocalizedText Strings/Lexicon:QuestionDialogue_No]";
    public List<string> TriggerActions { get; set; } = new();
}