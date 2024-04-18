using System.Reflection;
using HarmonyLib;
using ItemExtensions.Additions.Clumps;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace ItemExtensions.Patches;

public class TractorModPatches
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);

    internal static void Apply(Harmony harmony)
    {
        /*
        var attachments = new[] { "Axe", "Custom", "Hoe", "MeleeBlunt", "MeleeDagger", "MeleeSword", "Scythe", "Slingshot", "WateringCan" };

        foreach (var toolType in attachments)
        {
            var tractorToolMethod = AccessTools.Method($"Pathoschild.Stardew.TractorMod.Framework.Attachments.{toolType}Attachment:Apply",
                new[] { typeof(Vector2),typeof(Object),typeof(TerrainFeature),typeof(Farmer),typeof(Tool),typeof(Item),typeof(GameLocation) });

            if (tractorToolMethod is null) //if the method isn't found, return
            {
                Log($"Method not found. ({toolType}Attachment:Apply)", LogLevel.Warn);
                continue;
            }        
            Log($"Applying Harmony patch \"{nameof(TractorModPatches)}\": postfixing mod method \"Pathoschild.Stardew.TractorMod.Framework.Attachments.{toolType}Attachment.Apply\".");
        
            harmony.Patch(
                original: tractorToolMethod,
                postfix: new HarmonyMethod(typeof(TractorModPatches), nameof(Post_OnActivated))
            );
        }*/
        
        var updateAttachments = AccessTools.Method($"Pathoschild.Stardew.TractorMod.Framework.TractorManager:UpdateAttachmentEffects");

        if (updateAttachments is null) //if the method isn't found, return
        {
            Log($"Method not found. (UpdateAttachmentEffects)", LogLevel.Warn);
            return;
        }
        
        Log($"Applying Harmony patch \"{nameof(TractorModPatches)}\": postfixing mod method \"Pathoschild.Stardew.TractorMod.Framework.TractorManager:UpdateAttachmentEffects\".");
        
        harmony.Patch(
            original: updateAttachments,
            postfix: new HarmonyMethod(typeof(TractorModPatches), nameof(Post_UpdateAttachmentEffects))
        );
    }

    private static void Post_UpdateAttachmentEffects()
    {
        // get context
        var player = Game1.player;
        var location = player.currentLocation;
        var tool = player.CurrentTool;

        if (tool is null)
            return;

        if (GetRange(out var distance) == false)
            return;
        
        var grid = GetTileGrid(Game1.player.Tile, distance).ToArray();
        foreach (var tile in grid)
        {
#if DEBUG
            Log("Tile: " + tile, LogLevel.Info);
#endif   
            var obj = location.getObjectAtTile((int)tile.X, (int)tile.Y);
            if (obj is not null && ModEntry.Ores.ContainsKey(obj.ItemId))
            {
                obj.performToolAction(tool);
            }
            
            var clumps = location.resourceClumps?.Where(s => s.occupiesTile((int)tile.X, (int)tile.Y));
            
            if (clumps is null)
                return;
            
            foreach (var resource in clumps)
            {
                if(ExtensionClump.IsCustom(resource))
                    resource.performToolAction(tool, tool.UpgradeLevel, player.Tile);
            }
        }
    }

    private static bool GetRange(out int amount)
    {
        amount = -1;
        
        var tractorMod = ModEntry.Help.ModRegistry.Get("Pathoschild.TractorMod");
        var mod = (IMod)tractorMod?.GetType()?.GetProperty("Mod")?.GetValue(tractorMod);
        
        if (mod is null)
            return false;
        
        var config = mod.GetType()?.GetField("Config", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(mod);
        if (config?.GetType() is null)
            return false;
        
        var distance = config.GetType().GetProperty("Distance");
        if (distance is null)
            return false;

        try
        {
            amount = (int)distance?.GetValue(config);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static IEnumerable<Vector2> GetTileGrid(Vector2 origin, int distance)
    {
        for (int x = -distance; x <= distance; x++)
        {
            for (int y = -distance; y <= distance; y++)
                yield return new Vector2(origin.X + x, origin.Y + y);
        }
    }
    
    private static void Post_OnActivated(Vector2 tile, Object? tileObj, TerrainFeature? tileFeature, Farmer player, Tool? tool, Item? item, GameLocation location)
    {
        try
        {
            #if DEBUG
            Log($"Tool: {tool}, tileObj {tileObj?.QualifiedItemId}");
            #endif
            
            if (tool is null || tileObj is Chest)
                return;
            
            tileObj?.performToolAction(tool);
            
            if (tileFeature is ResourceClump r && Additions.Clumps.ExtensionClump.IsCustom(r))
            {
                //damage is set to 1 because we calculate actual dmg in other methods
                r.performToolAction(tool, 1, tile);
            }
        }
        catch (Exception e)
        {
            Log($"Error: {e}", LogLevel.Error);
        }
    }
}