using HarmonyLib;
using ItemExtensions.Models;
using ItemExtensions.Models.Contained;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System.Security.AccessControl;
using Object = StardewValley.Object;

namespace ItemExtensions.Patches;

public partial class TrainPatches
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);
    
    internal static void Apply(Harmony harmony)
    {
        Log($"Applying Harmony patch \"{nameof(TrainPatches)}\": prefixing SDV method \"Train.Update(GameTime, GameLocation)\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(Train), nameof(Train.Update)),
            prefix: new HarmonyMethod(typeof(TrainPatches), nameof(Post_Update))
        );
    }

    internal static void Post_Update(Train __instance, GameTime time, GameLocation location)
    {
        try
        {
            //only do this check once per second
            if(time.ElapsedGameTime.TotalMilliseconds % 2000 != 0)
                return;

            TryExtraDrops(__instance);
        }
        catch(Exception e)
        {
            Log($"Error: {e}", LogLevel.Error);
        }
    }

    internal static void TryExtraDrops(Train train)
    {

        var location = Game1.MasterPlayer.currentLocation;
        var who = Game1.MasterPlayer;

        /*
         * car type:
         * 0 plain
         * 1 coal
         * 2 passenger
         * 3 engine
          
         * resource type:
         * coal = 0
         * metal = 1
         * wood = 2
         * compartments = 3
         * grass = 4
         * hay = 5
         * bricks = 6
         * rocks = 7
         * packages = 8
         * presents = 9
         */
        
        for (int i = 0; i < train.cars.Count; i++)
        {
            var x = (int)(train.position.X - (float)((i + 1) * 512))/64;
            if (Utility.isOnScreen(new Point(x, 40)) == false)
                continue;

            foreach(var pair in ModEntry.TrainDrops)
            {
                var entry = pair.Value;

                var car = GetCarType(train.cars[i].carType);
                var resource = GetResource(train.cars[i].resourceType);
                
                if (car != entry.CarType)
                    continue;

                if (resource != entry.Resource && entry.Resource != ResourceType.None)
                    continue;

                if(entry.Chance < Game1.random.NextDouble())
                    continue;
                        
                if (string.IsNullOrWhiteSpace(entry.Condition) && GameStateQuery.CheckConditions(entry.Condition, location, who) == false)
                    continue;
                
                var item = ItemQueryResolver.TryResolve(entry.ItemId, context, entry.Filter, entry.PerItemCondition, avoidRepeat: entry.AvoidRepeat);
                
                var id = item.FirstOrDefault()?.Item.QualifiedItemId;
                        
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                Game1.createObjectDebris(id, x, 42, 2);
            }
        }
    }

    private static CarType GetCarType(int which)
    {
        return which switch {
            1 => CarType.Resource,
            2 => CarType.Passenger,
            3 => CarType.Engine,
            _ => CarType.Plain
        };
    }

    private static ResourceType GetResourceType(int which)
    {
        return which switch {
            0 => ResourceType.Coal,
            1 => ResourceType.Metal,
            2 => ResourceType.Wood,
            3 => ResourceType.Compartments,
            4 => ResourceType.Grass,
            5 => ResourceType.Hay,
            6 => ResourceType.Bricks,
            7 => ResourceType.Rocks,
            8 => ResourceType.Packages,
            9 => ResourceType.Presents,
            _ => ResourceType.None
        };
    }
}