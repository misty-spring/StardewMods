﻿using System.Linq;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using static SpousesIsland.Additions.Dialogues;

namespace SpousesIsland.Patches;

internal static class NpcPatches
{
    private static string Translate(string msg) => ModEntry.Translate(msg);
    private static void Log(string msg, LogLevel lv = LogLevel.Trace) => ModEntry.Mon.Log(msg, lv);

    internal static void Apply(Harmony harmony)
    {
        Log($"Applying Harmony patch \"{nameof(Patches)}\": prefixing SDV method \"NPC.tryToReceiveActiveObject(Farmer who)\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(NPC), nameof(NPC.tryToReceiveActiveObject)),
            prefix: new HarmonyMethod(typeof(NpcPatches), nameof(Pre_tryToReceiveActiveObject))
            );
        /*
        Log($"Applying Harmony patch \"{nameof(Patches)}\": prefixing SDV method \"NPC.warpToPathControllerDestination()\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(NPC), nameof(NPC.warpToPathControllerDestination)),
            prefix: new HarmonyMethod(typeof(NpcPatches), nameof(Post_warpToPathControllerDestination))
        );*/
    }

    private static void Post_warpToPathControllerDestination(NPC __instance)
    {
        if(!ModEntry.IslandToday)
            return;
        
        //if not in invited list
        if(ModEntry.ValidSpouses.Contains(__instance.Name) == false)
            return;
        
        if (__instance.currentLocation.Name.Equals(__instance.queuedSchedulePaths[^1].targetLocationName))
        {
            return;
        }
#if DEBUG
        ModEntry.Mon.Log($"NPC current location is {__instance.currentLocation.Name}, but the end destination is {__instance.queuedSchedulePaths[^1].targetLocationName}. Skipping", LogLevel.Debug);
#endif
        Game1.delayedActions.Add(new DelayedAction(500, IncludeSpeed));
        return;

        void IncludeSpeed()
        {
            __instance.addedSpeed += 0.3f;
        }
    }

    /// <summary>
    /// Tries to get our specific mod data.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private static bool TryGetModData(Object obj)
    {
        if (obj == null)
            return false;

        var noData = obj.modData is not { Length: > 0 };
        
        if (noData)
            return false;

        //if (!obj.modData.TryGetValue($"{ModEntry.Id}_IslandTicket", out _))
        //    return false;

        return obj.modData.TryGetValue($"{ModEntry.Id}_Days", out _);
    }

    /// <summary>
    /// If the item received is ours, runs custom actions
    /// </summary>
    /// <param name="__instance">NPC receiving.</param>
    /// <param name="who">Player.</param>
    /// <param name="__result">OG result.</param>
    /// <param name="probe">If just checking for an action.</param>
    /// <returns>Whether the OG method should be run.</returns>
    private static bool Pre_tryToReceiveActiveObject(NPC __instance, Farmer who, bool __result, bool probe = false)
    {
        var obj = who.ActiveObject;
        if (obj == null)
        {
            return true;
        }

        if (!TryGetModData(obj))
            return true;

        //if just checking for an action
        if (probe)
        {
            __result = true;
            return false;
        }

        who.Halt();
        who.faceGeneralDirection(__instance.getStandingPosition(), 0, opposite: false, useTileCalculations: false);

        if (ModEntry.IslandToday && __instance.Name != "Willy")
        {
            //tell player
            string alreadyongoing = Translate("AlreadyOngoing.Visit");
            Game1.addHUDMessage(new HUDMessage(alreadyongoing, HUDMessage.newQuest_type));
        }
        //if festival tomorrow
        else if (Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.season))
        {
            //tell player theres festival tmrw
            var festivalnotice = Game1.parseText(Translate("FestivalTomorrow"));
            Game1.drawDialogueBox(festivalnotice);
        }
        //if not, call method that handles NPC's reaction (+etc).
        else
        {
            TriggerTicket(__instance, who, obj.Name.Contains("Day"));
        }

        __result = true;
        return false;
    }

    /// <summary>
    /// Depending on the NPC, causes actions.
    /// </summary>
    /// <param name="receiver"></param>
    /// <param name="giver"></param>
    /// <param name="isDayTicket"></param>
    private static void TriggerTicket(NPC receiver, Farmer giver, bool isDayTicket)
    {
        var npcdata = giver.friendshipData[receiver.Name];

        if (npcdata.IsMarried() || npcdata.IsRoommate())
        {
            //var scheduledWeek = isDayTicket && giver.mailReceived.Contains("VisitTicket_week");
            //var scheduledDay = !isDayTicket && giver.mailReceived.Contains("VisitTicket_day");

            //if already invited
            if (ModEntry.Status.Any() && ModEntry.Status.ContainsKey(receiver.Name))
            {
                //tell player about it
                var alreadyinvited = string.Format(Translate("AlreadyInvited"), receiver.displayName);
                Game1.addHUDMessage(new HUDMessage(alreadyinvited, HUDMessage.error_type));
            }
            else
            {
                giver.ActiveObject.modData.TryGetValue($"{ModEntry.Id}_Days", out var daysRaw);
                var days = int.Parse(daysRaw);
                
                Draw(receiver, GetInviteDialogue(receiver));

                //user will always have data in Status (created during SaveLoadedBasicInfo).
                //so there's no worry about possible nulls
                ModEntry.Status.Add(receiver.Name, (days, days == 1));
                giver.mailReceived.Add(days == 1 ? "VisitTicket_day" : "VisitTicket_week");
                giver.reduceActiveItemByOne();
            }
        }
        else if (receiver.Name == "Willy" && (giver.currentLocation.Name == "Beach" || giver.currentLocation.Name == "FishShop"))
        {
            var willytext = Translate("Willy.IslandTicket");
            Draw(receiver, willytext);

            var yn = giver.currentLocation.createYesNoResponses();

            giver.currentLocation.createQuestionDialogue(
                question: Translate("IslandVisit.Question"),
                answerChoices: yn,
                afterDialogueBehavior: HandleIslandEvent);
        }
        else
        {
            //send rejection dialogue like the pendant one
            Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", receiver.displayName)));
        }
    }

    internal static void HandleIslandEvent(Farmer who, string whichAnswer)
    {
        if (whichAnswer == "No")
        {
            string rejected = Translate("IslandVisit.Rejected");
            Game1.drawObjectDialogue(rejected);
            return;
        }

        var willy = Utility.fuzzyCharacterSearch("Willy");
        willy.jump();
        who.reduceActiveItemByOne();

        if (Game1.random.NextDouble() < 0.2)
        {
            var aboveHead = "Strings\\Locations:BoatTunnel_willyText_random" + Game1.random.Next(2);
            willy.showTextAboveHead(aboveHead);

            Game1.pauseThenDoFunction(1500, TravelToIsland);
        }
        else
            TravelToIsland();

    }

    private static void TravelToIsland()
    {
        Game1.stats.Increment("boatRidesToIsland");
        Game1.fadeScreenToBlack();
        Game1.warpFarmer("IslandSouth", 21, 43, 0);
    }

    /// <summary>
    /// Get the dialogue for a NPC, depending on name and personality.
    /// </summary>
    /// <param name="who"></param>
    /// <returns>The NPC's reply to being invited (to the island).</returns>
    private static string GetInviteDialogue(NPC who)
    {
        var vanilla = who.Name switch
        {

            "Abigail" => true,
            "Alex" => true,
            "Elliott" => true,
            "Emily" => true,
            "Haley" => true,
            "Harvey" => true,
            "Krobus" => true,
            "Leah" => true,
            "Maru" => true,
            "Penny" => true,
            "Sam" => true,
            "Sebastian" => true,
            "Shane" => true,
            "Claire" => true,
            "Lance" => true,
            "Olivia" => true,
            "Sophia" => true,
            "Victor" => true,
            "Wizard" => true,
            _ => false,
        };

        if (vanilla)
        {
            return Translate($"Invite_{who.Name}");
        }
        else
        {
            var r = Game1.random.Next(1, 4);
            return Translate($"Invite_generic_{who.Optimism}_{r}"); //1 polite, 2 rude, 0 normal?
        }
    }
}