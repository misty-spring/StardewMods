using GingerIslandStart.Additions;
using ItemExtensions.Models;
using StardewModdingAPI.Events;
using StardewValley;

namespace GingerIslandStart.Events;

public static class Save
{
    private  static bool JustCreated { get; set; }
    
    /// <summary>
    /// Actions done when save is created.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <see cref="StardewValley.Objects.Chest.dumpContents"/>
    internal static void Created(object sender, SaveCreatedEventArgs e)
    {
        if (!ModEntry.EnabledOption)
            return;

        JustCreated = true;
        IslandChanges.ChangeGiftLocation();
    }

    internal static void Loaded(object sender, SaveLoadedEventArgs e)
    {
        if (JustCreated)
        {
            Game1.player.mailReceived.Add(ModEntry.Id);
            Game1.delayedActions.Add(new DelayedAction(300, PlayIntro));
            ModEntry.NeedsWarp = false;
        }
        else
            Location.CheckWarpChanges();
    }

    private static void PlayIntro()
    {
        Game1.PlayEvent("GingerIslandStart_AltIntro", false);
        Game1.delayedActions.Add(new DelayedAction(1000, () =>
        {
            Game1.CurrentEvent.onEventFinished += Location.WarpToIsland;
        }));
        ModEntry.NeedsWarp = false;
    }
}