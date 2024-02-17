// ReSharper disable ClassNeverInstantiated.Global

using System.Collections.Generic;

namespace DynamicDialogues.Models;

internal class HuntContext
{
    public int Timer { get; }
    public AfterSequenceBehavior OnFailure { get; }
    public AfterSequenceBehavior OnSuccess { get; }
    public bool CanPlayerRun { get; } = true;
    public List<ObjectData> Objects { get; } = new();

    public HuntContext()
    {
    }

    public HuntContext(HuntContext o)
    {
        Timer = o.Timer;
        OnFailure = o.OnFailure;
        OnSuccess = o.OnSuccess;
        CanPlayerRun = o.CanPlayerRun;
        Objects = o.Objects;
    }
}
