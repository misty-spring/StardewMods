using HarmonyLib;
using ItemExtensions.Additions;
using ItemExtensions.Models.Items;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Internal;

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

            TryExtraDrops(__instance, location);
        }
        catch(Exception e)
        {
            Log($"Error: {e}", LogLevel.Error);
        }
    }

    internal static void TryExtraDrops(Train train, GameLocation location)
    {
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

        var context = new ItemQueryContext(location, who, Game1.random);
        var y = 2592;

        for (int i = 0; i < train.cars.Count; i++)
        {
            var x = (int)(train.position.X - ((i + 1) * 512));

            if (Utility.isOnScreen(new Point(x, 40), 0) == false)
                continue;

            foreach(var pair in ModEntry.TrainDrops)
            {
                var entry = pair.Value;

                var car = GetCarType(train.cars[i].carType.Value);
                var resource = GetResourceType(train.cars[i].resourceType.Value);
                
                if (car != entry.Car)
                    continue;

                if (resource != entry.Type && entry.Type != ResourceType.None)
                    continue;

                if (Sorter.GetItem(entry, context, out var item) == false)
                    continue;
                
                Game1.createItemDebris(item, new Vector2(x, y), y + 320, location);
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