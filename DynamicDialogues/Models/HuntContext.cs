// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace DynamicDialogues.Models;

public class HuntContext
{
    public int Timer { get; set; }
    public AfterSequenceBehavior OnFailure { get; set; }
    public AfterSequenceBehavior OnSuccess { get; set; }
    public bool CanPlayerRun { get; set; } = true;
    public List<ObjectData> Objects { get; set; } = new();
}
