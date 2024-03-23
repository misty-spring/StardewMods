using ItemExtensions.Additions.Clumps;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace ItemExtensions.Additions;

public class Debugging
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);
    public static void Tester(string arg1, string[] arg2)
    {
        if (arg2 is null || arg2.Any() == false)
        {
            Log("Must have at least 1 argument.", LogLevel.Warn);
            return;
        }

        if (!Context.IsWorldReady)
        {
            Log("Must load a save.", LogLevel.Warn);
            return;
        }

        const string testPack = "mistyspring.testobj";
        
        if (ModEntry.Help.ModRegistry.Get(testPack) is null)
        {
            Log("The test pack was not found.", LogLevel.Error);
            return;
        }
            
        switch (arg2[0])
        {
            case "ore":
            case "clump":
                var pos = new Vector2(Game1.player.Tile.X + 2, Game1.player.Tile.Y);
                var clump = ExtensionClump.Create($"{testPack}_TestClump", pos);
                Log($"Adding clump ID {testPack}_TestClump at {pos}...", LogLevel.Info);
                Game1.player.currentLocation.resourceClumps.Add(clump);
                break;
            case "jelly":
                Game1.player.eatObject(new StardewValley.Object($"{testPack}_Jelly",1));
                break;
            case "eat":
                Game1.player.eatObject(new StardewValley.Object($"{testPack}_trash",1));
                break;
            case "sip":
            case "drink":
                Game1.player.eatObject(new StardewValley.Object("614",1));
                break;
            case "list":
                Log($"Possible commands: eat, clump, drink, jelly.", LogLevel.Info);
                break;
            default:
                Log($"Command {arg2[0]} not recognized.", LogLevel.Warn);
                break;
        }
    }

    public static void Fix(string arg1, string[] arg2)
    {
        if (!Context.IsWorldReady)
        {
            Log("Must load a save.", LogLevel.Warn);
            return;
        }
        
        List<ResourceClump> clumps = new();
        
        Utility.ForEachLocation(CheckForCustomClumps, true, true);

        foreach (var resource in clumps)
        {
            //give default values of a stone
            resource.textureName.Set("Maps/springobjects");
            resource.parentSheetIndex.Set(672);
            resource.loadSprite();
        }

        return;

        bool CheckForCustomClumps(GameLocation arg)
        {
            foreach (var resource in arg.resourceClumps)
            {
                //if not custom
                if(resource.modData.ContainsKey(ModKeys.ClumpId) == false)
                    continue;
                    
                clumps.Add(resource);
            }

            return true;
        }
    }

    public static void Dump(string arg1, string[] arg2)
    {
        if (arg2 is null || arg2.Any() == false)
        {
            Log("Must have at least 1 argument.", LogLevel.Warn);
            return;
        }

        if (!Context.IsWorldReady)
        {
            Log("No save has been loaded yet. This may cause issues", LogLevel.Warn);
        }

        var helper = ModEntry.Help;
        
        switch (arg2[0])
        {
            case "ore":
            case "ores":
                helper.Data.WriteJsonFile("dump/Ores.json", ModEntry.Ores);
                break;
            case "clump":
            case "clumps":
                helper.Data.WriteJsonFile("dump/ResourceClumps.json", ModEntry.BigClumps);
                break;
            case "eat":
            case "sip":
            case "drink":
                helper.Data.WriteJsonFile("dump/Animations.json", ModEntry.EatingAnimations);
                break;
            case "seed":
            case "seeds":
                helper.Data.WriteJsonFile("dump/Seeds.json", ModEntry.Seeds);
                break;
            case "menu":
            case "menuactions":
            case "actions":
                helper.Data.WriteJsonFile("dump/MenuActions.json", ModEntry.MenuActions);
                break;
            case "shop": 
            case "shops":
                helper.Data.WriteJsonFile("dump/Shops.json", GetAllShopExtras());
                break;
            default:
                Log($"Command {arg2[0]} not recognized.", LogLevel.Warn);
                return;
        }
        
        Log("File dumped in mod folder.", LogLevel.Info);
    }

    private static Dictionary<string, List<ISalable>> GetAllShopExtras()
    {
        throw new NotImplementedException();
    }
}