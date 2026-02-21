using System.Text;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace MistyCore.Patches;

public partial class GameLocationPatches
{
    private static void Post_getFish(GameLocation __instance, float millisecondsAfterNibble, string bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, ref Item __result, string locationName = null)
    {
        //if not rod, run og method
        if (who.CurrentTool is not FishingRod rod)
            return;
        
        var location = __instance.Name;
        
#if DEBUG
        Log("Getting fish data for location: " + location);
#endif

        if (ModEntry.FishingOverrides.TryGetValue(location, out var data) == false)
        {
#if DEBUG
            Log("Couldn't find fish overrides for location " + location);
#endif
            return;
        }

        if (GameStateQuery.CheckConditions(data.Condition, __instance, who) == false)
        {
            ModEntry.Mon.Log("Conditions don't apply for overriding fishing location: " + location, LogLevel.Debug);
            return;
        }

        var tackle = rod.GetTackle() ?? new List<Object>();
        var realBait = bait ?? "none";
        var hasFishingRequirements = false;
        if (data.MissingBait.Any())
        {
            hasFishingRequirements = data.MissingBait.Contains(realBait);
        }
        if (data.MissingTackle.Any())
        {
            hasFishingRequirements = hasFishingRequirements || HasTackle(tackle, data.MissingTackle);
        }
        if (data.MissingRod.Any()) //data.MissingRod is not null &&
        {
            hasFishingRequirements = hasFishingRequirements || data.MissingRod.Contains(rod.ItemId);
        }

        if (hasFishingRequirements)
        {
#if DEBUG
            Log("Player has fishing requirements.");
#endif
            return;
        }

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