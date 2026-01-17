using System.Text;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;
using Object = StardewValley.Object;

namespace MistyCore.Additions;

public static class TriggerActions
{
    
    /// <summary>
    /// Adds experience to the given skill.
    /// </summary>
    /// <param name="args">Arguments.</param>
    /// <param name="context">Trigger context.</param>
    /// <param name="error">Error, if any.</param>
    /// <returns>Whether the action was run.</returns>
    public static bool AddExp(string[] args, TriggerActionContext context, out string error)
    {
        #if DEBUG
        var fullString = new StringBuilder();
        foreach (var s in args)
            fullString.Append(' ' + s);

        ModEntry.Mon.Log($"Args: {fullString}", LogLevel.Debug);
        #endif
        
        //command <who> <skill> <amt>
        
        var p = !ArgUtility.TryGet(args, 1, out var playerKey, out error);
        var t = !ArgUtility.TryGet(args, 2, out var skillRaw, out error);
        var i = !ArgUtility.TryGetOptionalInt(args, 3, out var amount, out error, defaultValue: 50);
        var invalid = p || t || i;

        if (invalid || string.IsNullOrWhiteSpace(skillRaw))
            return false;

        int skill;
        if (int.TryParse(skillRaw, out var parsedSkill))
            skill = parsedSkill;
        else
        {
            skill = skillRaw.ToLower() switch {
                "farming" => 0,
                "fishing" => 1,
                "foraging" => 2,
                "mining" => 3,
                "combat" => 4,
                "luck" => 5,
                _ => 0
            };
        }

#if DEBUG
        ModEntry.Mon.Log($"Values:\namount = {amount}\nwho = {playerKey}, \nskill = {skill}({skillRaw})", LogLevel.Debug);
#endif
        var success = GameStateQuery.Helpers.WithPlayer(Game1.player, playerKey, target =>
        {
            try
            {
                target.gainExperience(skill, amount);
            }
            catch (Exception e)
            {
                ModEntry.Mon.Log("Error:" + e);
                return false;
            }
            return true;
        });
        
        return success;
    }

    public static bool AddItemHoldUp(string[] args, TriggerActionContext context, out string error)
    {
        //command item [recipe] [quantity] [quality]
        //required
        var noItemId = !ArgUtility.TryGet(args, 1, out var qualifiedItemId, out error);
        //optional
        ArgUtility.TryGetOptionalBool(args, 2, out var isRecipe, out error, defaultValue: false);
        ArgUtility.TryGetOptionalInt(args, 3, out var quantity, out error, defaultValue: 1);
        ArgUtility.TryGetOptionalInt(args, 4, out var quality, out error, defaultValue: 0);
        
        if (noItemId || string.IsNullOrWhiteSpace(qualifiedItemId))
            return false;

        var itemId = qualifiedItemId.Remove(0, 3);
        
        var item = isRecipe switch
        {
            true => new Object(itemId, quantity, true, -1, quality),
            _ => ItemRegistry.Create(qualifiedItemId, quantity, quality)
        };

        Game1.player.addItemByMenuIfNecessaryElseHoldUp(item);
        
        return true;
    }
}