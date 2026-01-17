namespace MistyCore.Models;

public class FishingOverrideData
{
    public List<string> MissingRod { get; set; }
    public List<string> MissingTackle { get; set; }
    public List<string> MissingBait { get; set; }
    public string Condition { get; set; } = "TRUE";
    public List<string> TriggerActions { get; set; } = new List<string>();
    public List<string> PossibleOverrides { get; set; } = new List<string>();
    public string Message { get; set; }
}