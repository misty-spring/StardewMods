namespace DynamicDialogues.Models;

/// <summary>
/// For queueing events.
/// </summary>
/// <see cref="StardewValley.Event"/>
internal class EventData
{
    public string Key { get; }
    public string Location { get; }
    public bool CheckPreconditions { get; } = true;
    public bool CheckSeen { get; } = true;
    public bool ResetIfUnseen { get; } = true;
    public string TriggerKey { get; }

    public EventData(EventData ev)
    {
        Key = ev.Key;
        Location = ev.Location;
        CheckSeen = ev.CheckSeen;
        CheckPreconditions = ev.CheckPreconditions;
        ResetIfUnseen = ev.ResetIfUnseen;
        TriggerKey = ev.TriggerKey;
    }

    public EventData(string which, string where, bool conditional, bool checkSeen, bool resettable, string trigger)
    {
        Key = which;
        Location = where;
        CheckSeen = checkSeen;
        CheckPreconditions = conditional;
        ResetIfUnseen = resettable;
        TriggerKey = trigger;
    }
    public EventData(string which, string where, string trigger)
    {
        Key = which;
        Location = where;
        CheckSeen = true;
        CheckPreconditions = true;
        ResetIfUnseen = true;
        TriggerKey = trigger;
    }
}
