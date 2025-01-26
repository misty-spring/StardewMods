using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

namespace IridiumScytheGinger;

public class ModEntry : Mod
{
    private static IMonitor Mon { get; set; }

    public override void Entry(IModHelper helper)
    {
        Mon = Monitor;
        
        var harmony = new Harmony(ModManifest.UniqueID);
        Mon.Log($"Applying Harmony patch \"{nameof(ModEntry)}\": postfixing SDV method \"MeleeWeapon.doSwipe\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(MeleeWeapon), nameof(MeleeWeapon.doSwipe)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(Post_doSwipe))
        );
    }

    public static void Post_doSwipe(MeleeWeapon __instance, int type, Vector2 position, int facingDirection, float swipeSpeed, Farmer f)
    {
        if (__instance.ItemId != "66")
            return;
#if DEBUG
        Mon.Log($"Current weapon is iridium scythe", LogLevel.Info);
#endif
        var xTile = (int)position.X / 64;
        var yTile = (int)position.Y / 64;
        var location = __instance.lastUser.currentLocation;

        foreach (var tile in GetAdjacentTiles(xTile, yTile))
        {
#if DEBUG
            Mon.Log("tile: "+tile, LogLevel.Debug);
#endif
            if (location.isCropAtTile((int)tile.X, (int)tile.Y) == false)
                continue;
            
            var hoeDirt = location.GetHoeDirtAtTile(new Vector2((int)tile.X, (int)tile.Y));
            if (hoeDirt?.crop is null)
                continue;
#if DEBUG
            Mon.Log("value: " + hoeDirt?.crop.whichForageCrop.Value, LogLevel.Debug);
#endif
            if (hoeDirt?.crop.whichForageCrop.Value == "2")
            {
                //create item
                var item = ItemRegistry.Create("(O)829");
                //add exp
                __instance.lastUser.gainExperience(2, 7);
                //destroy crop
                hoeDirt.destroyCrop(false);
                //randomize position
                var vector2 = Game1.random.Next(4) switch
                {
                    0 => new Vector2(-64f, 0f),
                    1 => new Vector2(64f, 0f),
                    2 => new Vector2(0f, 64f),
                    _ => new Vector2(0f, -64f),
                };
                var vector = new Vector2(tile.X * 64 + 64, tile.Y * 64 + 64);
                //add
                location.debris.Add(new Debris(item, vector, vector + vector2));
            }
        }
    }

    private static List<Vector2> GetAdjacentTiles(int xTile, int yTile)
    {
        var all = new List<Vector2>()
        {
            //center tile
            new(xTile, yTile),
            //1 tile of distance
            new(xTile - 1, yTile),
            new (xTile + 1, yTile),
            new (xTile - 1, yTile - 1),
            new (xTile - 1, yTile + 1),
            new (xTile + 1, yTile + 1),
            new (xTile + 1, yTile - 1),
            new(xTile, yTile - 1 ),
            new (xTile, yTile + 1),
            //2 tiles of distance
            //top
            new(xTile - 2, yTile - 2),
            new (xTile - 1, yTile - 2),
            new (xTile, yTile - 2),
            new (xTile + 1, yTile - 2 ),
            new (xTile + 2, yTile - 2), 
            //bottom
            new(xTile - 2, yTile + 2),
            new (xTile - 1, yTile + 2),
            new (xTile, yTile + 2),
            new (xTile + 1, yTile + 2 ),
            new (xTile + 2, yTile +  2), 
            //center
            new(xTile - 2, yTile - 1),
            new (xTile + 2, yTile - 1),
            new (xTile - 2, yTile + 1),
            new (xTile + 2, yTile + 1 ),
            new (xTile - 2, yTile), 
            new (xTile + 2, yTile), 
        };
        return all;
    }
}