// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MistyCore.Models;

public class HuntContext
{
    public int Timer { get; set; }
    //public string Host { get; set; }
    //public string HostMessageKey { get; set; }
    public AfterSequenceBehavior OnFailure { get; set; }
    public AfterSequenceBehavior OnSuccess { get; set; }
    public bool CanPlayerRun { get; set; } = true;
    public List<HuntingObjectData> Objects { get; set; } = new();
}
