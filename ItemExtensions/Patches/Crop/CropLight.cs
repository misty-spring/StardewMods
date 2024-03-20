using ItemExtensions.Additions;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace ItemExtensions.Patches;

internal partial class CropPatches
{
    internal static void Post_harvest()
    {}
    
    internal static void Post_newDay()
    {}

    internal static void Post_plant(HoeDirt __instance, string itemId, Farmer who, bool isFertilizer, bool __result)
    {
        //if didn't plant
        if (__result == false)
            return;

        var hasColor = Game1.cropData[itemId].CustomFields.TryGetValue(ModKeys.LightColor, out var rawColor);
        var hasSize = Game1.cropData[itemId].CustomFields.TryGetValue(ModKeys.LightColor, out var rawSize);

        var r = __instance.crop;
        //try get light data
        if (r.NetFields.AddField().TryGetValue(ModKeys.LightSize, out var sizeRaw) == false ||
            r.modData.TryGetValue(ModKeys.LightColor, out var rgb) == false ||
            r.modData.TryGetValue(ModKeys.LightTransparency, out var transRaw) == false)
        {
#if DEBUG
            Log($"Data for {id} light not found. (onAddedToLocation)", LogLevel.Trace);
#endif
            return;
        }

        if (float.TryParse(sizeRaw, out var size) == false)
        {
            Log($"Couldn't parse light size for clump Id {id} ({sizeRaw})", LogLevel.Debug);
            return;
        }

        if (float.TryParse(transRaw, out var trans) == false)
        {
            Log($"Couldn't parse transparency for clump Id {id} ({sizeRaw})", LogLevel.Debug);
            return;
        }

        //parse
        Color color;
        if (rgb.Contains(' ') == false)
        {
            color = Utility.StringToColor(rgb) ?? Color.White;
        }
        else
        {
            var rgbs = ArgUtility.SplitBySpace(rgb);
            var parsed = rgbs.Select(int.Parse).ToList();
            color = new Color(parsed[0], parsed[1], parsed[2]);
        }

        color *= trans;

        //set
        var fixedPosition = new Vector2(tile.X + r.width.Value / 2, tile.Y * r.height.Value / 2);
        var lightSource = new LightSource(4, fixedPosition, size, color);

        r.modData.Add(ModKeys.LightId, $"{lightSource.Identifier}");
    }
}