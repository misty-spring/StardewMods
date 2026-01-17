using HarmonyLib;
using Microsoft.Xna.Framework;
using MistyCore.Additions.EventCommands;
using MistyCore.Models;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Triggers;

namespace MistyCore.Patches;

internal class EventPatches
{
    private static void Log(string msg, LogLevel lv = ModEntry.Level) => ModEntry.Mon.Log(msg, lv);

    internal static void Apply(Harmony harmony)
    {
        Log($"Applying Harmony patch \"{nameof(EventPatches)}\": postfixing SDV method \"Event.receiveActionPress\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(Event), nameof(Event.receiveActionPress)),
            postfix: new HarmonyMethod(typeof(EventPatches), nameof(Post_receiveActionPress))
        );

        Log($"Applying Harmony patch \"{nameof(EventPatches)}\": postfixing SDV method \"Event.festivalUpdate\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(Event), nameof(Event.festivalUpdate)),
            postfix: new HarmonyMethod(typeof(EventPatches), nameof(Post_festivalUpdate))
        );
        
        Log($"Applying Harmony patch \"{nameof(EventPatches)}\": postfixing SDV method \"Event.UpdateBeforeNextCommand\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(Event), "UpdateBeforeNextCommand"),
            postfix: new HarmonyMethod(typeof(EventPatches), nameof(Post_UpdateBeforeNextCommand))
        );

        Log($"Applying Harmony patch \"{nameof(EventPatches)}\": prefixing SDV method \"Event.endBehaviors\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(Event), nameof(Event.endBehaviors), parameters: new[] { typeof(string[]), typeof(GameLocation) }),
            prefix: new HarmonyMethod(typeof(EventPatches), nameof(Pre_endBehaviors))
        );
    }
    
    private static void Pre_endBehaviors(Event __instance, string[] args, GameLocation location)
    {
        /* format(s):
         * end lastSleepLocation
         * end warp <where> <x> <y>
         * end house
         * end farmhouse
         * end islandfarmhouse
         */

        if (args.Length < 2)
            return;

        string where;
        Vector2 pos;
        var x = 0;
        var y = 0;

        switch (ArgUtility.Get(args, 1))
        {
            case "lastSleepLocation":
                where = __instance.farmer.lastSleepLocation.Value;
                pos = __instance.farmer.mostRecentBed / 64;
                break;
            case "warp":
                where = args[2];
                if (args.Length < 5)
                {
                    Utility.getDefaultWarpLocation(where, ref x, ref y);
                    pos = new Vector2(x, y);
                }
                else
                {
                    x = int.Parse(args[3]);
                    y = int.Parse(args[4]);
                    pos = new Vector2(x, y);
                }
                break;
            case "house":
                var home = Utility.getHomeOfFarmer(__instance.farmer);
                where = home.NameOrUniqueName;
                var entry = home.getEntryLocation();
                pos = new Vector2(entry.X, entry.Y);
                break;
            case "farmhouse":
                where = "FarmHouse";
                Utility.getDefaultWarpLocation(where, ref x, ref y);
                pos = new Vector2(x, y);
                break;
            case "islandfarmhouse":
                where = "IslandFarmHouse";
                Utility.getDefaultWarpLocation(where, ref x, ref y);
                pos = new Vector2(x, y);
                break;
            default:
                return;
        }

        if (string.IsNullOrWhiteSpace(where) || pos == Vector2.Zero)
            return;

        Log($"Data: location {where}, position {pos}");

        if (where == location.Name)
        {
            __instance.LogCommandError(args, "Destiny location is the same as current one. If you want to set an end position, use `end position` instead.");
            return;
        }

        __instance.setExitLocation(where, (int)pos.X, (int)pos.Y);
        Game1.eventOver = true;
        __instance.CurrentCommand += 2;
        Game1.screenGlowHold = false;
    }

    public static void Post_receiveActionPress(ref Event __instance, int xTile, int yTile)
    {
        if(__instance.playerControlSequenceID == null)
            return;

        if (!__instance.playerControlSequenceID.StartsWith(World.HuntSequenceId))
        {
            #if DEBUG
            Log($"Doesn't seem to be a custom object hunt. ({__instance.playerControlSequenceID})", LogLevel.Warn);
            #endif
            return;
        }

        //for simplifying
        var e = __instance;
        var tile = new Vector2(xTile, yTile);
        const int multiplier = 64; //when drawn to scren, its multiplied by 64. or 4 * 16.
        var position = new Rectangle((int)(tile.X * multiplier), (int)(tile.Y * multiplier), 64, 64);

        Log("Current position checking: " + tile);

        foreach (var prop in e.festivalProps)
        {
#if DEBUG
            var boundingRect = ModEntry.Help.Reflection.GetField<Rectangle>(prop, "boundingRect").GetValue();
            Log("Prop's boundingRect: " + boundingRect);
#endif

            if (!prop.isColliding(position))
                continue;

            Log("Removing...");
            //e.festivalProps.Remove(prop);
            e.removeFestivalProps(position);
            Game1.playSound("coin");
            Game1.player.festivalScore++;
            break;
        }

        //check if any props are left
#if DEBUG
        Log("Props left: " + e.festivalProps.Count);
#endif

        if (e.festivalProps.Count > 0)
            return;

        //get data for this object hunt
        var actualId = e.playerControlSequenceID.Remove(0, 33);
        if (ModEntry.ObjectHunt.TryGetValue(actualId, out var data) == false)
        {
            __instance.LogErrorAndHalt($"Couldn't find the given ID. ({actualId})");
            return;
        }

        if (data.OnSuccess != null)
        {
            RunSequenceActions(data.OnSuccess);
        }

        e.EndPlayerControlSequence();
        e.CurrentCommand++;
        e.festivalTimer = 0;
    }

    // ReSharper disable once UnusedParameter.Global
    public static void Post_festivalUpdate(ref Event __instance, GameTime time)
    {
        if (__instance == null)
            return;
        
        if (__instance.festivalTimer > 0)
            return;

        if(__instance.playerControlSequenceID == null)
            return;
            
        if (!__instance.playerControlSequenceID.StartsWith(World.HuntSequenceId))
            return;

        var e = __instance;

        //we remove the ID
        var actualId = e.playerControlSequenceID.Remove(0, 33);
        ModEntry.ObjectHunt.TryGetValue(actualId, out var data);

        if (data is null)
        {
            __instance.LogErrorAndHalt("Data for object hunt is null.");
            return;
        }

        //do either failure or success actions depending on outcome
        var hasPropsLeft = e.festivalProps?.Count > 0;
        if (hasPropsLeft)
        {
            e.props.Clear();
            if (data.OnFailure != null)
            {
#if DEBUG
                Log("Running OnFailure actions...");
#endif
                var f = data.OnFailure;
                RunSequenceActions(f);
            }
        }
        else
        {
            if (data.OnSuccess != null)
            {
#if DEBUG
                Log("Running OnSuccess actions...");
#endif
                var s = data.OnSuccess;
                RunSequenceActions(s);
            }
        }

        e.EndPlayerControlSequence();
        e.CurrentCommand++;
    }

    internal static void Post_UpdateBeforeNextCommand(Event __instance, GameLocation location, GameTime time)
    {
        if (string.IsNullOrWhiteSpace(__instance.playerControlSequenceID) || !__instance.playerControlSequenceID.StartsWith(World.HuntSequenceId))
        {
            return;
        }

	var actualId = __instance.playerControlSequenceID.Remove(0, 33);

        if (ModEntry.ObjectHunt.TryGetValue(actualId, out var data) == false || data.Timer <= 0)
            return;
        
        __instance.festivalUpdate(time);
    }

    /*
    public static void Post_setUpPlayerControlSequence(ref Event __instance, string id)
    {
        if (id.StartsWith(World.HuntSequenceId) == false)
            return;

        if (ModEntry.ObjectHunt.TryGetValue(id, out var data) == false)
            return;

        if (string.IsNullOrWhiteSpace(data.Host) == false)
        {
            var host = ModEntry.Help.Reflection.GetField<NPC>(__instance, "festivalHost");
            host.SetValue(__instance.getActorByName(data.Host));
        }

        if (string.IsNullOrWhiteSpace(data.HostMessageKey) == false)
        {
            var hostMessageKey = ModEntry.Help.Reflection.GetField<string>(__instance, "hostMessageKey");
            hostMessageKey.SetValue(data.HostMessageKey);
        }
    }*/

    private static void RunSequenceActions(AfterSequenceBehavior sequence)
    {
        if (!string.IsNullOrWhiteSpace(sequence.Mail))
        {
            if (sequence.ImmediateMail)
                Game1.player.mailbox.Add(sequence.Mail);
            else
                Game1.player.mailForTomorrow.Add(sequence.Mail);
        }
        if (!string.IsNullOrWhiteSpace(sequence.Flag))
        {
            Game1.player.mailReceived.Add(sequence.Flag);
        }
        if (sequence.Energy != 0)
        {
            Game1.player.Stamina += sequence.Energy;
        }
        if (sequence.Health != 0)
        {
            Game1.player.health += sequence.Health;
        }

        if (sequence.TriggerActions is not null && sequence.TriggerActions.Count > 0)
        {
            foreach (var action in sequence.TriggerActions)
            {
                TriggerActionManager.TryRunAction(action, out var error, out var exception);
                if (error != null)
                    ModEntry.Mon.Log("Error while running action: " + error, LogLevel.Error);
            }
        }
    }
}