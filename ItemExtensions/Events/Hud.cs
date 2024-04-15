using ItemExtensions.Patches;
using StardewModdingAPI.Events;
using StardewValley;

namespace ItemExtensions.Events;

internal static class Hud
{
    internal static void Rendered(object sender, RenderedHudEventArgs e)
    {
        if(InventoryPatches.AnimationQueue is null)
            return;

        InventoryPatches.AnimationQueue.draw(e.SpriteBatch);
            
        if (InventoryPatches.AnimationQueue.currentNumberOfLoops < InventoryPatches.AnimationQueue.totalNumberOfLoops)
            return;

        ModEntry.Mon.Log("Clearing animation...");
        InventoryPatches.AnimationQueue = null;
    }
}