using ItemExtensions.Additions;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace ItemExtensions.Events;

public static class Mobile
{
    public static void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsWorldReady || Game1.player.isRidingHorse())
            return;
        
        var location = Game1.player.currentLocation;
        var x = (int)Game1.player.GetToolLocation().X / Game1.tileSize;
        var y = (int)Game1.player.GetToolLocation().Y / Game1.tileSize;

        //if our front tile doesn't equal x/y
        if (x != (int)e.Cursor.Tile.X || y != (int)e.Cursor.Tile.Y)
            return;
        
        var obj = location.getObjectAtTile(x, y, true);
        var clump = GetResourceClumpAt(location, x * 64, y * 64);
        
        if (obj == null && clump == null)
            return;
        
        var isNode = obj?.ItemId != null && ModEntry.Ores.ContainsKey(obj.ItemId);
        var isClump = clump?.modData.TryGetValue(ModKeys.ClumpId, out var clumpId) == true && ModEntry.BigClumps.ContainsKey(clumpId);

        var tool = "tool";
        if (isNode)
        {
            //can't use it above because it "might not be initialized" so
            ModEntry.Ores.TryGetValue(obj.ItemId, out var nodeData);
            tool = nodeData?.Tool;
        }
        else if (isClump && clump.modData.TryGetValue(ModKeys.ClumpId, out var id))
        {
            ModEntry.BigClumps.TryGetValue(id, out var clumpData);
            tool = clumpData?.Tool;
        }
        
        var inventoryTool = GetTool(tool);

        if (inventoryTool is null)
            return;
            
        if (Game1.player.CurrentTool == inventoryTool)
            return;

        Game1.player.CurrentToolIndex = Game1.player.Items.IndexOf(inventoryTool);
        Game1.player.FireTool();
    }

    private static Tool GetTool(string requiredType)
    {
        if (string.IsNullOrWhiteSpace(requiredType))
        {
            return null;
        }

        const StringComparison ignoreCase = StringComparison.OrdinalIgnoreCase;
        
        foreach (var item in Game1.player.Items)
        {
            if (item is not Tool t)
                continue;

            var toolType = t.GetType().Name;
            
            //can be any
            if (requiredType.Equals("Any", ignoreCase))
                return t;
            
            //match
            if (toolType.Equals(requiredType, ignoreCase) || (requiredType.Equals("Weapon", ignoreCase) && toolType.Equals("MeleeWeapon")))
                return t;

            //if it's not the "anyexcept" tool
            if (!requiredType.StartsWith("AnyExcept", ignoreCase)) 
                continue;
            
            var notThisTool = requiredType.Remove(0, 9);
            if (t.GetType().Name.Equals(notThisTool) == false)
                return t;
        }

        return null;
    }

    private static ResourceClump GetResourceClumpAt(GameLocation location, int x, int y)
    {
        return location.resourceClumps.FirstOrDefault(clump => clump.getBoundingBox().Contains(x, y));
    }
}