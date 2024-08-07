using DynamicDialogues.Commands;
using DynamicDialogues.Models;
using Microsoft.Xna.Framework;
using StardewValley;

namespace DynamicDialogues.Patches;

internal partial class EventPatches
{
    private static void Pre_endBehaviors(Event __instance, string[] args, GameLocation location)
    {
        /* format(s):
         * end lastSleepLocation
         * end warp <where> <x> <y>
         * end house
         * end farmhouse
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
            return;

        //for simplifying
        var e = __instance;
        var tile = new Vector2(xTile, yTile);
        const int multiplier = 64; //when drawn to scren, its multiplied by 64. or 4 * 16.
        var position = new Rectangle((int)(tile.X * multiplier), (int)(tile.Y * multiplier), 64, 64);

        Log("Current position checking: " + tile, Level);

        foreach (var prop in e.festivalProps)
        {
#if DEBUG
            var boundingRect = ModEntry.Help.Reflection.GetField<Rectangle>(prop, "boundingRect").GetValue();
            Log("Prop's boundingRect: " + boundingRect, Level);
#endif

            if (!prop.isColliding(position))
                continue;

            Log("Removing...", Level);
            //e.festivalProps.Remove(prop);
            e.removeFestivalProps(position);
            Game1.playSound("coin");
            break;
        }

        /*
        var flag = false;
        foreach (var obj in data.Objects)
        {
            //if found match
            if (xTile == obj.X && yTile == obj.Y)
            {
                flag = true;
                Log("Found prop to remove.", Level);
                break;
            }
        }
        //if none matched, return
        if(!flag)
            return;

        foreach(var prop in e.props)
        {
            if (prop.TileLocation != position)
                continue;

            Log("Removing...", Level);
            e.props.Remove(prop);
            Game1.playSound("coin");
            break;

        }*/

        //check if any props are left
#if DEBUG
        Log("Props left: " + e.festivalProps.Count, Level);
#endif
        var hasPropsLeft = e.festivalProps?.Count > 0;
        if (hasPropsLeft)
            return;

        //get data for this object hunt
        var actualId = e.playerControlSequenceID.Remove(0, 40);
        var data = Helper.GameContent.Load<Dictionary<string, HuntContext>>(@"mistyspring.dynamicdialogues\Commands\objectHunt")[actualId];

        if (data.OnSuccess != null)
        {
            var s = data.OnSuccess;
            if (!string.IsNullOrWhiteSpace(s.Mail))
            {
                if (s.ImmediateMail)
                    Game1.player.mailbox.Add(s.Mail);
                else
                    Game1.player.mailForTomorrow.Add(s.Mail);
            }
            if (!string.IsNullOrWhiteSpace(s.Flag))
            {
                Game1.player.mailReceived.Add(s.Flag);
            }
            if (s.Energy != 0)
            {
                Game1.player.Stamina += s.Energy;
            }
            if (s.Health != 0)
            {
                Game1.player.health += s.Health;
            }
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
        var actualId = e.playerControlSequenceID.Remove(0, 40);
        var data = Helper.GameContent.Load<Dictionary<string, HuntContext>>(@"mistyspring.dynamicdialogues\Commands\objectHunt")[actualId];

        //do either failure or success actions depending on outcome
        var hasPropsLeft = e.props?.Count > 0;
        if (hasPropsLeft)
        {
            e.props.Clear();
            if (data.OnFailure != null)
            {
                var f = data.OnFailure;
                if (!string.IsNullOrWhiteSpace(f.Mail))
                {
                    if (f.ImmediateMail)
                        Game1.player.mailbox.Add(f.Mail);
                    else
                        Game1.player.mailForTomorrow.Add(f.Mail);
                }
                if (!string.IsNullOrWhiteSpace(f.Flag))
                {
                    Game1.player.mailReceived.Add(f.Flag);
                }
                if (f.Energy != 0)
                {
                    Game1.player.Stamina += f.Energy;
                }
                if (f.Health != 0)
                {
                    Game1.player.health += f.Health;
                }
            }
        }
        else
        {
            if (data.OnSuccess != null)
            {
                var s = data.OnSuccess;
                if (!string.IsNullOrWhiteSpace(s.Mail))
                {
                    if (s.ImmediateMail)
                        Game1.player.mailbox.Add(s.Mail);
                    else
                        Game1.player.mailForTomorrow.Add(s.Mail);
                }
                if (!string.IsNullOrWhiteSpace(s.Flag))
                {
                    Game1.player.mailReceived.Add(s.Flag);
                }
                if (s.Energy != 0)
                {
                    Game1.player.Stamina += s.Energy;
                }
                if (s.Health != 0)
                {
                    Game1.player.health += s.Health;
                }
            }
        }

        e.EndPlayerControlSequence();
        e.CurrentCommand++;
    }
}