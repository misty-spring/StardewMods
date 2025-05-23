using System.Linq;
using SpousesIsland.Additions;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using static SpousesIsland.ModEntry;

namespace SpousesIsland.Events;

internal static class Day
{
    internal static void Log(string msg, LogLevel lv = LogLevel.Trace) => Mon.Log(msg, lv);
    internal static void OnStart(object sender, DayStartedEventArgs e)
    {
        //if rain during a random visit, don't
        if (Game1.getLocationFromName("IslandSouth").IsRainingHere() && Config.AvoidRain && !IsFromTicket)
        {
            IslandToday = false;
            return;
        }
        
        CheckPlayer();

        if (!IsFromTicket)
        {
            RandomizedInt = Game1.random.Next(1, 101);
            IslandToday = Config.CustomChance >= RandomizedInt;
        }
        
        //if no island or not unlocked, do nothing (if unlocked but no island, make devan invisible)
        if (!IslandToday || !Unlocked)
        {
#if DEBUG
            Log("Island visit won't happen today.", LogLevel.Info);
#endif
            if (!Unlocked)
                return;

            var devan = Game1.getCharacterFromName("DevanSpring");
            if (devan == null)
                return;

            devan.IsInvisible = true;
            devan.daysUntilNotInvisible = 1;
            return;
        }

        foreach (var character in ValidSpouses)
        {
            #if DEBUG
            Log($"Checking {character}...", LogLevel.Debug);
            #endif
            
            if(Status is not null && Status.Any() && !Status.ContainsKey(character))
                continue;
            
            var spouse = Game1.getCharacterFromName(character);
            if (spouse is null)
                continue;

            Schedule.Island(spouse);
        }

        if (Children?.Count == 0 || Children == null)
            return;

        if (Config.AllowChildren == false)
        {
            Schedule.Babysitter();
            return;
        }

        if (!Beds.HasAnyKidBeds() && Config.UseFurnitureBed)
        {
            Log("There's no child beds in island farmhouse. Farmer's kids won't visit.", LogLevel.Warn);
            Schedule.Babysitter();
            return;
        }

        foreach (var child in Children)
        {
            var kid = Game1.getCharacterFromName(child.Name);
            
            if (kid is null)
                continue;
            
            Schedule.Island(kid);
        }
    }
    
    internal static void OnEnd(object sender, DayEndingEventArgs e)
    {
        if (Status is not null && Status.Any())
        {
#if DEBUG
            var status = "";
            foreach (var pair in Status)
            {
                status += $"{pair.Key}: {pair.Value}\n";
            }
            Mon.Log(status, LogLevel.Debug);
#endif
            RandomizedInt = 0;
            IslandToday = true;
            IsFromTicket = true;
            
            Help.Data.WriteSaveData( $"{Id}_IslandVisit", Status);
        }
        else
        {
            //get new %
            PreviousDayRandom = RandomizedInt;
            RandomizedInt = Game1.random.Next(1, 101);
            IslandToday = Config.CustomChance >= RandomizedInt;
            IsFromTicket = false;
        }

        var hadYesterday = Config.CustomChance >= PreviousDayRandom;
        //
    }
}