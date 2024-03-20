using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ItemExtensions.Additions;
using ItemExtensions.Events;
using ItemExtensions.Models;
using ItemExtensions.Models.Internal;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Locations;
using StardewValley.Internal;
using Object = StardewValley.Object;

namespace ItemExtensions.Patches;

public class GameLocationPatches
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);
    internal static void Apply(Harmony harmony)
    {
        Log($"Applying Harmony patch \"{nameof(GameLocationPatches)}\": postfixing SDV method \"GameLocation.spawnObjects\".");
        
        harmony.Patch(
            original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.spawnObjects)),
            postfix: new HarmonyMethod(typeof(GameLocationPatches), nameof(Post_spawnObjects))
        );
        
        Log($"Applying Harmony patch \"{nameof(GameLocationPatches)}\": transpiling SDV method \"GameLocation.spawnObjects\".");
        
        harmony.Patch(
            original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.spawnObjects)),
            transpiler: new HarmonyMethod(typeof(GameLocationPatches), nameof(Transpiler))
        );
    }

    /// <summary>
    /// Checks if any spawned object is a custom resource. If so, applies data.
    /// </summary>
    /// <param name="__instance"></param>
    private static void Post_spawnObjects(GameLocation __instance)
    {
        Log($"Checking spawns made at {__instance.DisplayName ?? __instance.NameOrUniqueName}");
        
        foreach (var item in __instance.Objects.Values)
        {
            if(!ModEntry.Ores.TryGetValue(item.ItemId, out var resource))
                continue;
            
            if(resource is null || resource == new ResourceData())
                continue;
            
            Log($"Setting spawn data for {item.DisplayName}");
            
            World.SetSpawnData(item, resource);
        }
    }
    
    /// <summary>
    /// Edits <see cref="GameLocation.spawnObjects"/>:
    /// Before trying to create a forage, this checks if it's a clump. If so, spawns and breaks (sub)loop.
    /// </summary>
    /// <param name="instructions">Original code.</param>
    /// <param name="il"></param>
    /// <returns>Edited code.</returns>
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        //new one
        var codes = new List<CodeInstruction>(instructions);
        var instructionsToInsert = new List<CodeInstruction>();

        var index = codes.FindIndex(ci => ci.opcode == OpCodes.Callvirt && ci.operand is MethodInfo { Name: "get_Chance"});
        Log($"index: {index}", LogLevel.Info);
        
        var redirectTo = codes.Find(ci => codes.IndexOf(ci) == index + 3);//new[]{typeof(ISpawnItemData), typeof(ItemQueryContext), typeof(bool), typeof(HashSet<string>), typeof(Func<string, string>), typeof(Item), typeof(Action<string, string>)})};
        //&& (ConstructorInfo)ci.operand == AccessTools.Method(typeof(ItemQueryResolver), nameof(ItemQueryResolver.TryResolveRandomItem), new[]{typeof(ISpawnItemData), typeof(ItemQueryContext), typeof(bool), typeof(HashSet<string>), typeof(Func<string, string>), typeof(Item), typeof(Action<string, string>)}));
        var breakAt = codes.Find(ci => ci.opcode == OpCodes.Ldloc_S && ((LocalBuilder)ci.operand).LocalIndex == 11);
        
        //add label for brfalse
        var brfalseLabel = il.DefineLabel();
        redirectTo.labels ??= new List<Label>();
        redirectTo.labels.Add(brfalseLabel);
        
        //add label for br_S
        var brSLabel = il.DefineLabel();
        breakAt.labels ??= new List<Label>();
        breakAt.labels.Add(brSLabel);
        
        if (index <= -1) 
            return codes.AsEnumerable();
        
        #if DEBUG
        Log($"INDEXED \nname: {codes[index].opcode.Name}, type: {codes[index].opcode.OpCodeType}, operandtype: {codes[index].opcode.OperandType}, \npop: {codes[index].opcode.StackBehaviourPop}, push: {codes[index].opcode.StackBehaviourPush}, \nvalue: {codes[index].opcode.Value}, flowcontrol: {codes[index].opcode.FlowControl}, operand: {codes[index].operand}");
        Log($"REDIRECT \nname: {redirectTo.opcode.Name}, type: {redirectTo.opcode.OpCodeType}, operandtype: {redirectTo.opcode.OperandType}, \npop: {redirectTo.opcode.StackBehaviourPop}, push: {redirectTo.opcode.StackBehaviourPush}, \nvalue: {redirectTo.opcode.Value}, flowcontrol: {redirectTo.opcode.FlowControl}, operand: {redirectTo.operand}");
        Log($"BREAK \nname: {breakAt.opcode.Name}, type: {breakAt.opcode.OpCodeType}, operandtype: {breakAt.opcode.OperandType}, \npop: {breakAt.opcode.StackBehaviourPop}, push: {breakAt.opcode.StackBehaviourPush}, \nvalue: {breakAt.opcode.Value}, flowcontrol: {breakAt.opcode.FlowControl}, operand: {breakAt.operand}");
        #endif
        
        /* if (TryCustomClump(forage, context, vector2))
         * {
         *      ++this.numberOfSpawnedObjectsOnMap;
         *      break;
         * }
         */
        
        //arguments
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_S, 13)); //spawndata arg
        //instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SpawnForageData), "forage"))); //load forage
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_S, 10)); //context arg
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_S, 16)); //position arg
        
        //call my code w/ prev args
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GameLocationPatches), nameof(CheckIfCustomClump))));
        /*
        // ??? get result ?
        //instructionsToInsert.Add(new CodeInstruction(OpCodes.Stloc_S, 25));
        //instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_S, 25));*/

        //tell where to go if false
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Brfalse, brfalseLabel)); 
        
        // ?
        //instructionsToInsert.Add(new CodeInstruction(OpCodes.Nop));
        
        //if true: +spawnobj
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg, 0)); 
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg, 0)); 
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldfld, typeof(GameLocation).GetRuntimeField("numberOfSpawnedObjectsOnMap")));
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldc_I4_1));
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Add));
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Stfld, typeof(GameLocation).GetRuntimeField("numberOfSpawnedObjectsOnMap")));
        
        //break
        
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Br_S, brSLabel));

        Log($"codes count: {codes.Count}, insert count: {instructionsToInsert.Count}");
        Log("Inserting method");
        codes.InsertRange(index + 3, instructionsToInsert);
        return codes.AsEnumerable();
    }

    /// <summary>
    /// Checks if a spawn is a clump, and tries to place if so.
    /// </summary>
    /// <param name="forage">The spawn data.</param>
    /// <param name="context">Location context.</param>
    /// <param name="vector2">Position to spawn at.</param>
    /// <returns>Whether the spawn was a clump.</returns>
    public static bool CheckIfCustomClump(SpawnForageData forage, ItemQueryContext context, Vector2 vector2)
    {
        //var log = ModEntry.Help.Reflection.GetField<IGameLogger>(typeof(Game1), "log").GetValue();
        //Log("Called transpiled code");

        if (forage is null)
        {
            Log("parameter forage can't be null.");
            return false;
        }
        
        if (context is null)
        {
            Log("Context can't be null. Skipping");
            return false;
        }
        
        var random = context.Random ?? Game1.random;
        var randomItemId = forage.RandomItemId;

        //check spawnData validity
        var isOurs = forage.Id.StartsWith("Clump ItemExtension", StringComparison.OrdinalIgnoreCase);
        
        if (!isOurs)
            return false;

        var validItemId = ModEntry.BigClumps.ContainsKey(forage.ItemId);
        var isAnyRandomAClump = false;
        
        List<string> randomsThatAreItem = new();
        List<string> randomsThatAreClump = new();

        //if a random Id exists
        if (randomItemId != null && randomItemId.Any())
        {
            foreach (var randomId in randomItemId)
            {
                //if parsed id NOT in big clumps, assume item
                if (ModEntry.BigClumps.ContainsKey(randomId) == false)
                {
                    randomsThatAreItem.Add(randomId);
                    continue;
                }

                randomsThatAreClump.Add(randomId);
                isAnyRandomAClump = true;
            }
            
            //if no random is clump
            if (!isAnyRandomAClump)
            {
                //if main is clump
                if (validItemId)
                {
                    TryPlaceCustomClump(forage.ItemId, context, vector2);
                    return true;
                }

                //else
                Log($"None of the item IDs seem to be a custom clump. No spawn will be made. ({context.Location.NameOrUniqueName} @ {forage.Id})", LogLevel.Warn);
                return false;
            }
        }
        else if (randomItemId is null)
        {
            #if DEBUG
            Log("No random items listed. Attempting to spawn by ItemId");
            #endif
            TryPlaceCustomClump(forage.ItemId, context, vector2);
            return true;
        }

        if (!isAnyRandomAClump)
        {
            if (!validItemId)
            {
                Log($"None of the item IDs seem to be a custom clump. No spawn will be made. ({context.Location?.NameOrUniqueName} @ {forage.Id})", LogLevel.Warn);
                return false;
            }
        }
        
        //if id isn't valid clump, add to random items
        if (!string.IsNullOrWhiteSpace(forage.ItemId))
        {
            if (!validItemId)
                randomsThatAreItem.Add(forage.ItemId);
            else
                randomsThatAreClump.Add(forage.ItemId);
        }

        var all = new List<string>();
        all.AddRange(randomsThatAreItem);
        all.AddRange(randomsThatAreClump);

        var chosen = random.ChooseFrom(all);
        var placeItem = randomsThatAreItem.Contains(chosen);

        if (placeItem)
        {
            var firstOrDefault = ItemQueryResolver.TryResolve(
                chosen, 
                context, 
                perItemCondition: forage.PerItemCondition, 
                maxItems: forage.MaxItems, 
                // ReSharper disable once AccessToModifiedClosure
                logError: (query, error) => { Log($"Location '{context.Location.NameOrUniqueName}' failed parsing item query '{query}' for forage '{chosen}': {error}"); }
                ).FirstOrDefault();
            
            //return true because the list already has items that might fail
            if (firstOrDefault is not null)
            {
                //create
                var asItem = ItemQueryResolver.ApplyItemFields(firstOrDefault.Item, forage, context) as Item;
                
                // 1 out of [clumps count]
                // e.g, if you have 4 possible clumps, 20% chance
                if (asItem is Object o)
                {
                    o.IsSpawnedObject = true;
                    if (context.Location.dropObject(o, vector2 * 64f, Game1.viewport, true))
                    {
                        ++context.Location.numberOfSpawnedObjectsOnMap;
                        return true;
                    }
                }
            }
        }

        //if an item was meant to be placed but couldn't (for x reason), choose clump
        if (placeItem)
        {
            chosen = Game1.random.ChooseFrom(randomsThatAreClump);
        }
        
        TryPlaceCustomClump(chosen, context, vector2);

        return true;
    }

    /// <summary>
    /// Attempts to place a custom clump.
    /// </summary>
    /// <param name="clumpId">Id to get data from.</param>
    /// <param name="context">Query context.</param>
    /// <param name="position">Position to place at.</param>
    private static void TryPlaceCustomClump(string clumpId, ItemQueryContext context, Vector2 position)
    {
        #if DEBUG
        Log("Placing clump...");
        #endif
        
        if(ModEntry.BigClumps.TryGetValue(clumpId, out var data) == false)
        {
            return;
        }

        var clump = new ExtensionClump(clumpId, data, position);
        var cf = context.Location.GetData().CustomFields;

        try
        {
            if (cf is not null)
            {
                var hasRect = cf.TryGetValue(ModKeys.ClumpSpawnRect, out var rawRect);
                var avoidOverlap = cf.TryGetValue(ModKeys.AvoidOverlap, out var overlap) && bool.Parse(overlap);

                if (hasRect)
                {
                    var newPosition = CheckPosition(context, position, rawRect, avoidOverlap);
                    clump.Tile = newPosition;
                }
                else if(avoidOverlap && context.Location.IsTileOccupiedBy(position))
                {
                    var newPosition = NearestOpenTile(context.Location, position);
                    clump.Tile = newPosition;
                }
            }
            
            context.Location.resourceClumps.Add(clump);
        }
        catch (Exception ex)
        {
            Log($"Error: {ex}", LogLevel.Error);
        }
    }

    /// <summary>
    /// Fixes position for spawn.
    /// This might be heavy on resources, so it's recommended to just use FTM instead.
    /// </summary>
    /// <param name="context">Spawn context.</param>
    /// <param name="position">Current position.</param>
    /// <param name="rawRect">Spawn zone, unparsed.</param>
    /// <param name="avoidOverlap">If to avoid placing on a tile with content.</param>
    /// <returns></returns>
    private static Vector2 CheckPosition(ItemQueryContext context, Vector2 position, string rawRect, bool avoidOverlap)
    {
        var result = position;
        
        //can either be "x y w h" for single one, or for multiple "\"x y w h\" \"x y w h\""
        var split = ArgUtility.SplitBySpaceQuoteAware(rawRect);
        var rects = new List<Rectangle>();
        //if multiple, parse each. otherwise parse single one
        if (split[0].Contains(' '))
        {
            foreach (var raw in split)
            {
                var args = ArgUtility.SplitBySpace(raw);
                rects.Add(new Rectangle(int.Parse(args[0]), int.Parse(args[1]), int.Parse(args[2]), int.Parse(args[3])));
            }
        }
        else
        {
            rects.Add(new Rectangle(int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2]), int.Parse(split[3])));
        }

        //if point isn't in allowed rect, set to a random point in any
        if (rects.Any(r => r.Contains(position))) 
            return result;
        
        var random = context.Random ?? Game1.random;
        var randomRect = random.ChooseFrom(rects);

        if (!avoidOverlap)
        {
            result = new Vector2(
                random.Next(randomRect.X, randomRect.X + randomRect.Width),
                random.Next(randomRect.Y, randomRect.Y + randomRect.Height));
        }
        else
        {
            for (var i = 0; i < 30; i++)
            {
                result = new Vector2(
                    random.Next(randomRect.X, randomRect.X + randomRect.Width),
                    random.Next(randomRect.Y, randomRect.Y + randomRect.Height));

                if (context.Location.IsTileOccupiedBy(position) == false)
                    break;
            }
        }

        return result;
    }

    /// <summary>
    /// Finds the nearest open tile.
    /// </summary>
    /// <param name="location">Location to use for checks.</param>
    /// <param name="target">Initial position.</param>
    /// <returns>A tile that is unoccupied.</returns>
    internal static Vector2 NearestOpenTile(GameLocation location, Vector2 target)
    {
        var position = new Vector2();
        for (var i = 1; i < 30; i++)
        {
            var toLeft = new Vector2(target.X - i, target.Y);
            if (!location.IsTileOccupiedBy(toLeft))
            {
                position = toLeft;
                break;
            }
            
            var toRight = new Vector2(target.X + i, target.Y);
            if (!location.IsTileOccupiedBy(toRight))
            {
                position = toRight;
                break;
            }
            
            var toUp = new Vector2(target.X, target.Y - i);
            if (!location.IsTileOccupiedBy(toUp))
            {
                position = toUp;
                break;
            }
            
            var toDown = new Vector2(target.X, target.Y + i);
            if (!location.IsTileOccupiedBy(toDown))
            {
                position = toDown;
                break;
            }

            var upperLeft= new Vector2(target.X - i, target.Y - 1);
            if (!location.IsTileOccupiedBy(upperLeft))
            {
                position = upperLeft;
                break;
            }
            
            var lowerLeft= new Vector2(target.X - i, target.Y + 1);
            if (!location.IsTileOccupiedBy(lowerLeft))
            {
                position = lowerLeft;
                break;
            }
            
            var upperRight= new Vector2(target.X + i, target.Y - 1);
            if (!location.IsTileOccupiedBy(upperRight))
            {
                position = upperRight;
                break;
            }
            
            var lowerRight= new Vector2(target.X + i, target.Y + 1);
            if (!location.IsTileOccupiedBy(lowerRight))
            {
                position = lowerRight;
                break;
            }
        }

        return position;
    }
}