// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace DynamicDialogues.Models;

///<summary>Conditions for a dialogue to be added.</summary>
public class PlayerConditions
{
    // null means 'any'
    public string Hat { get; set; }
    public string Rings { get; set; }
    public string Shirt { get; set; }
    public string Pants { get; set; }
    public string Boots { get; set; }
    public string Inventory { get; set; }
    public string GameStateQuery { get; set; } = "TRUE"; //see https://stardewvalleywiki.com/Modding:Migrate_to_Stardew_Valley_1.6#Game_state_queries 

    public PlayerConditions()
    {}
}