using ItemExtensions.Additions;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Internal;
using StardewValley.Menus;

namespace ItemExtensions.Events;

public static class Menu
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);
    public static void Changed(object sender, MenuChangedEventArgs e)
    {
        
#if DEBUG
        Log("Opening treasure menu...");
#endif

        if (e.NewMenu is not ItemGrabMenu rewards || rewards.source != ItemGrabMenu.source_fishingChest)
        {
#if DEBUG
            Log($"Won't open the menu. menu is itemgrab? {Game1.activeClickableMenu is ItemGrabMenu}. type is {Game1.activeClickableMenu}");
            
            if(e.NewMenu is ItemGrabMenu menu)
                Log($"source {menu.source}");
#endif
            return;
        }

        var context = new ItemQueryContext(Game1.player.currentLocation, Game1.player, Game1.random);

        foreach (var (entry, data) in ModEntry.Treasure)
        {
#if DEBUG
            Log($"Checking entry {entry}...");
#endif
            if (Sorter.GetItem(data, context, out var item) == false)
                continue;

            if(rewards.ItemsToGrabMenu.tryToAddItem(item) is not null)
                Log($"Added treasure reward from entry {entry} ({item.QualifiedItemId})");
            else
                Log("Couldn't add item.");
        }
    }
}