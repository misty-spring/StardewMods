namespace MistyCore.Models;

public class ItemAdditionData
{
    public string Condition { get; set; } = "TRUE";
    public string FlagToSet { get; set; }
    public string PlaySound { get; set; }
    public List<AddedItemData> Items { get; set; } = new();
    public List<TileChangeData> TileChanges { get; set; } = new();
    public List<TileChangeData> TileRemovals { get; set; } = new();
    public List<string> TriggerActions { get; set; } = new List<string>();
    public string MessageToShowIfConditionsFail { get; set; }
    public bool IsLetter { get; set; }= false;
    public string MessageToShowIfAlreadyReceived { get; set; }
    public bool IsLetterReceived { get; set; }= false;
}