using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ItemExtensions.Additions.Clumps;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace ItemExtensions.Patches.Mods;

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
        
        Log($"Applying Harmony patch \"{nameof(TractorModPatches)}\": transpiling mod method \"Pathoschild.Stardew.TractorMod.Framework.Attachments.SeedAttachment:Apply\".");
        
        harmony.Patch(
            original: AccessTools.Method($"Pathoschild.Stardew.TractorMod.Framework.Attachments.SeedAttachment:Apply"),
            transpiler: new HarmonyMethod(typeof(TractorModPatches), nameof(SeedAttachment_Transpiler))
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
        for (var x = -distance; x <= distance; x++)
        {
            for (var y = -distance; y <= distance; y++)
                yield return new Vector2(origin.X + x, origin.Y + y);
        }
    }
    
    /// <summary>
    /// Edits <see cref="GameLocation.spawnObjects"/>:
    /// Before trying to create a forage, this checks if it's a clump. If so, spawns and breaks (sub)loop.
    /// </summary>
    /// <param name="instructions">Original code.</param>
    /// <param name="il"></param>
    /// <returns>Edited code.</returns>
    private static IEnumerable<CodeInstruction> SeedAttachment_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        //new one
        var codes = new List<CodeInstruction>(instructions);

        var index = codes.FindIndex(ci => ci.opcode == OpCodes.Callvirt && ci.operand is MethodInfo { Name: "get_ItemId"});
#if DEBUG
        Log($"index: {index}, total {codes.Count}", LogLevel.Info);
#endif
        
        /* this replaces `item.ItemId` with `CropPatches.GetSeedForTractor(item)`
         * for that, we insert the method right on get_ItemId
         * callvirt will put the value on evaluation stack, so no need to worry
         */

        var getSeed = new CodeInstruction(OpCodes.Call,
            AccessTools.Method(typeof(TractorModPatches), nameof(GetSeedForTractor)));
        
        Log("Replacing method");
        codes[index] = getSeed;
        
        return codes.AsEnumerable();
    }

    internal static string GetSeedForTractor(Item item)
    {
#if DEBUG
        Log("Successfully called GetSeedForTractor");
#endif
        if (item is Object o)
        {
            //use obj location, otherwise player location, as final fallback game1 location
            var location = o.Location ?? Game1.player.currentLocation;
            location ??= Game1.currentLocation;
            
            return CropPatches.ResolveSeedId(o.ItemId, location);
        }

        return item.ItemId;
    }
}