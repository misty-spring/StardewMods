// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace DynamicDialogues.Models;

///<summary>Notifications sent to game HUD.</summary>
public class NotificationData
{
    public int Time { get; set; } = -1; //time to show at
    public string Location { get; set; } = "any"; //the location to show at
    public string Message { get; set; } //msg to display
    public string Sound { get; set; } //sound to make
    //(Maybe?) string Icon { get; set; } = "; //icon
    //public int FadeOut { get; set; } = -1; //fadeout is auto set by game

    public bool IsBox { get; set; } //if box instead
    public PlayerConditions Conditions { get; set; } = new();
    public string TriggerAction { get; set; }
}
