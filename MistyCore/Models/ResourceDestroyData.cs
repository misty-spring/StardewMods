namespace MistyCore.Models;

public class ResourceDestroyData
{
    public string Condition { get; set; } = "TRUE";
    public float Chance { get; set; } = 1f;
    public List<string> TriggerActions { get; set; } = new();
    public string PlayEvent  { get; set; } = null;
    public bool CheckEventSeen { get; set; } = true;
}