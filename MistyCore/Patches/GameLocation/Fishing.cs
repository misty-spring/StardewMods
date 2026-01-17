using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace MistyCore.Patches;

public partial class GameLocationPatches
{
    private static void Post_getFish(float millisecondsAfterNibble, string bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, ref Item __result, string locationName = null)
    {
#if DEBUG
        Log("Getting fish data for location: " + who.currentLocation.Name);
#endif
        //if not rod, run og method
        if (who.CurrentTool is not FishingRod rod)
            return;
        
        if (ModEntry.FishingOverrides.TryGetValue(who.currentLocation.Name, out var data) == false)
            return;

        if (GameStateQuery.CheckConditions(data.Condition, who.currentLocation, who) == false)
            return;

        var tackle = rod.GetTackle() ?? new List<Object>();
        var realBait = bait ?? "none";
        var hasFishingRequirements = false;

        if (data.MissingBait is not null && data.MissingBait.Any())
        {
            hasFishingRequirements = data.MissingBait.Contains(realBait);
        }
        if (data.MissingTackle is not null && data.MissingTackle.Any())
        {
            hasFishingRequirements = hasFishingRequirements || HasTackle(tackle, data.MissingTackle);
        }
        if (data.MissingRod is not null && data.MissingRod.Any())
        {
            hasFishingRequirements = hasFishingRequirements || data.MissingRod.Contains(rod.ItemId);
        }

        if (hasFishingRequirements)
            return;

        var possibleOverrides = data.PossibleOverrides;
        var item = Game1.random.ChooseFrom(possibleOverrides);

#if DEBUG
        Log("Chosen item: " + item);
#endif
        __result = ItemRegistry.Create(item);

        if (string.IsNullOrWhiteSpace(data.Message) == false)
        {
            var text = TokenParser.ParseText(data.Message);
            Game1.pauseThenMessage(600, text);
        }
    }

    private static bool HasTackle(List<Object> tackle, List<string> requiredTackle)
    {
        if (tackle is null || tackle?.Count <= 0)
            return false;

        foreach (var obj in tackle)
        {
            if(obj is null)
                continue;

            foreach (var value in requiredTackle)
            {
                if(obj.ItemId.Equals(value))
                    return true;
            }
        }
        return false;
    }
}