using StardewValley;

namespace FarmVisitors
{
    // ReSharper disable once InconsistentNaming
    internal class DupeNPC : NPC
    {
        public ScheduleData CustomData { get; set; }
        public bool IsOutside { get; set; }
        public int ControllerTime { get; set; }
        public int DurationSoFar { get; set; }
        public int TimeOfArrival { get; set; }
        public bool CustomVisiting { get; set; }
        
        internal DupeNPC(NPC who)
        {
            Name = who.Name;
            displayName = who.displayName;
            Sprite = who.Sprite;
            Age = who.Age;
            CurrentDialogue = null;
            ignoreScheduleToday = true;
            temporaryController = null;
            currentLocation = Utility.getHomeOfFarmer(Game1.player);
            Position = Utility.getHomeOfFarmer(Game1.player).getEntryLocation().ToVector2();
            Schedule = null;
            goingToDoEndOfRouteAnimation.Value = false;
            Portrait = who.Portrait;
            Manners = who.Manners;
            SocialAnxiety = who.SocialAnxiety;
            Optimism = who.Optimism;
            Gender = who.Gender;
            Breather = who.Breather;
        }
        internal DupeNPC(NPC who, ScheduleData s)
        {
            CustomData = s;
            Name = who.Name;
            displayName = who.displayName;
            Sprite = who.Sprite;
            Age = who.Age;
            CurrentDialogue = null;
            ignoreScheduleToday = true;
            temporaryController = null;
            currentLocation = Utility.getHomeOfFarmer(Game1.player);
            Position = Utility.getHomeOfFarmer(Game1.player).getEntryLocation().ToVector2();
            Schedule = null;
            goingToDoEndOfRouteAnimation.Value = false;
            Portrait = who.Portrait;
            Manners = who.Manners;
            SocialAnxiety = who.SocialAnxiety;
            Optimism = who.Optimism;
            Gender = who.Gender;
            Breather = who.Breather;
        }

    }
}