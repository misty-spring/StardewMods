using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

namespace KrobusSleeps;

public class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        var harmony = new Harmony(ModManifest.UniqueID);
        Monitor.Log($"Applying Harmony patch \"{nameof(ModEntry)}\": postfixing SDV method \"FarmHouse.getSpouseBedSpot\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.getSpouseBedSpot)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(Post_getSpouseBedSpot))
        );
    }

    public static void Post_getSpouseBedSpot(string spouseName, ref Point __result)
    {
        if (spouseName != "Krobus")
            return;

        //get farmer house's bed
        var farmHouse = Utility.getHomeOfFarmer(Game1.player);
        var bed = farmHouse.GetBed(BedFurniture.BedType.Double);

        //if it doesn't exist
        if (bed is null)
        {
            bed = farmHouse.GetBed(BedFurniture.BedType.Single);
            
            if (bed is null)
                return;
        }
        
        Point bed_spot = bed.GetBedSpot();
        if (bed.bedType == BedFurniture.BedType.Double)
        {
            bed_spot.X++;
        }
        __result = bed_spot;
    }
}