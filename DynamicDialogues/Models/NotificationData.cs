// ReSharper disable ClassNeverInstantiated.Global
namespace DynamicDialogues.Models;

///<summary>Notifications sent to game HUD.</summary>
internal class NotificationData
{
    public int Time { get; } = -1; //time to show at
    public string Location { get; } = "any"; //the location to show at
    public string Message { get; } //msg to display
    public string Sound { get; } //sound to make
    //(Maybe?) string Icon { get; set; } = "; //icon
    //public int FadeOut { get; set; } = -1; //fadeout is auto set by game

    public bool IsBox { get; } //if box instead
    public PlayerConditions Conditions { get; } = new();
    public string TriggerAction { get; }

    public NotificationData()
    {
    }

    public NotificationData(NotificationData rn)
    {
        Time = rn.Time;
        Location = rn.Location;
        
        Conditions = rn.Conditions;
        TriggerAction = rn.TriggerAction;
        
        Message = rn.Message;
        Sound = rn.Sound;

        IsBox = rn.IsBox;
    }
}
