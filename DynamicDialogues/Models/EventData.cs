// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace DynamicDialogues.Models;

/// <summary>
/// For queueing events.
/// </summary>
/// <see cref="StardewValley.Event"/>
public class EventData
{
    public string Key { get; set; }
    public string Location { get; set; }
    public bool CheckPreconditions { get; set; } = true;
    public bool CheckSeen { get; set; } = true;
    public bool ResetIfUnseen { get; set; } = true;
    public string TriggerKey { get; set; }

    public EventData(string which, string where, bool conditional, bool checkSeen, bool resettable, string trigger)
    {
        Key = which;
        Location = where;
        CheckSeen = checkSeen;
        CheckPreconditions = conditional;
        ResetIfUnseen = resettable;
        TriggerKey = trigger;
    }
}
