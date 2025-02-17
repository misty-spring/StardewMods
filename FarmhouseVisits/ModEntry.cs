using FarmhouseVisitsMP.APIs;
using FarmhouseVisitsMP.ModContent;
using FarmhouseVisitsMP.Models;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Pathfinding;
using Multiplayer = FarmhouseVisitsMP.ModContent.Multiplayer;

// ReSharper disable InconsistentNaming

namespace FarmhouseVisitsMP;

// ReSharper disable once ClassNeverInstantiated.Global
public class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        helper.Events.GameLoop.GameLaunched += GameLaunched;
        helper.Events.GameLoop.SaveLoaded += Events.SaveLoaded;
        helper.Events.Multiplayer.PeerConnected += Multiplayer.OnPeerConnected;

        helper.Events.GameLoop.DayStarted += DayStarted;
        helper.Events.GameLoop.TimeChanged += OnTimeChange;
        helper.Events.GameLoop.DayEnding += Events.DayEnding;

        helper.Events.Player.Warped += Events.PlayerWarp;

        helper.Events.GameLoop.ReturnedToTitle += Events.TitleReturn;
        helper.Events.Content.AssetRequested += Events.AssetRequest;
        helper.Events.Content.AssetsInvalidated += Events.AssetInvalidated;

        Config = Helper.ReadConfig<ModConfig>();

        Help = Helper;
        Logger = Monitor.Log;
        TL = Helper.Translation;
    }

    private void GameLaunched(object sender, GameLaunchedEventArgs e)
    {
        Content.ClearValues();
        Content.CleanTemp();
#if DEBUG
        Log("Game launched", LogLevel.Info);
#endif
        if (Config.Debug)
        {
            Monitor.Log("Debug has been turned on. This will change configuration for testing purposes.", LogLevel.Warn);

            Monitor.Log("Chance set to 100 (% every 10 min)");
            Config.CustomChance = 100;
            Monitor.Log("Starting hour will be 630.");
            Config.StartingHours = 630;
        }

        var allowedStringVals = new[]
        {
            "VanillaOnly",
            "VanillaAndMod",
            "None"
        };

        // get Generic Mod Config Menu's API (if it's installed)
        var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (configMenu is null)
            return;

        var hasNonDestructive = Help.ModRegistry.IsLoaded("IamSaulC.NonDestructiveNPCs");

        #region config
        // register mod
        configMenu.Register(
            mod: ModManifest,
            reset: () => Config = new ModConfig(),
            save: () => Helper.WriteConfig(Config)
        );

        // basic config options
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.CustomChance.name"),
            tooltip: () => Helper.Translation.Get("config.CustomChance.description"),
            getValue: () => Config.CustomChance,
            setValue: value => Config.CustomChance = value,
            min: 0,
            max: 100,
            interval: 1
        );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.GiftChance.name"),
            tooltip: () => Helper.Translation.Get("config.GiftChance.description"),
            getValue: () => Config.GiftChance,
            setValue: value => Config.GiftChance = value,
            min: 0,
            max: 100,
            interval: 1
        );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.MaxVisitsPerDay.name"),
            tooltip: () => Helper.Translation.Get("config.MaxVisitsPerDay.description"),
            getValue: () => Config.MaxVisitsPerDay,
            setValue: value => Config.MaxVisitsPerDay = value,
            min: 0,
            max: 24,
            interval: 1
        );
        configMenu.SetTitleScreenOnlyForNextOptions(
            mod: ModManifest,
            titleScreenOnly: true
        );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.VisitDuration.name"),
            tooltip: () => Helper.Translation.Get("config.VisitDuration.description"),
            getValue: () => Config.Duration,
            setValue: value => Config.Duration = value,
            min: 1,
            max: 20,
            interval: 1
        );
        configMenu.SetTitleScreenOnlyForNextOptions(
            mod: ModManifest,
            titleScreenOnly: false
        );
        //extra customization
        configMenu.AddPageLink(
            mod: ModManifest,
            pageId: "Extras",
            text: () => TL.Get("config.Extras")
        );

        //extra customization
        if (hasNonDestructive)
        {
            configMenu.AddPageLink(
                mod: ModManifest,
                pageId: "Places",
                text: () => TL.Get("config.Places.name")
            );
        }

        //extra customization
        configMenu.AddPageLink(
            mod: ModManifest,
            pageId: "Sleepovers",
            text: () => TL.Get("config.Sleepovers")
        );

        //developer config
        configMenu.AddPageLink(
            mod: ModManifest,
            pageId: "Debug",
            text: () => TL.Get("config.Debug.name")
        );

        configMenu.AddPage(
            mod: ModManifest,
            pageId: "Extras",
            pageTitle: () => TL.Get("config.Extras")
        );

        configMenu.AddSectionTitle(
            mod: ModManifest,
            text: () => TL.Get("config.VisitConfiguration"),
            tooltip: null);

        configMenu.SetTitleScreenOnlyForNextOptions(
            mod: ModManifest,
            titleScreenOnly: true
        );
        configMenu.AddTextOption(
            mod: ModManifest,
            getValue: () => Config.Blacklist,
            setValue: value => Config.Blacklist = value,
            name: () => TL.Get("config.Blacklist.name"),
            tooltip: () => TL.Get("config.Blacklist.description")
        );
        configMenu.SetTitleScreenOnlyForNextOptions(
            mod: ModManifest,
            titleScreenOnly: false
            );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.StartingHours.name"),
            tooltip: () => Helper.Translation.Get("config.StartingHours.description"),
            getValue: () => Config.StartingHours,
            setValue: value => Config.StartingHours = value,
            min: 630,
            max: 2300,
            interval: 100
        );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.EndingHours.name"),
            tooltip: () => Helper.Translation.Get("config.EndingHours.description"),
            getValue: () => Config.EndingHours,
            setValue: value => Config.EndingHours = value,
            min: 600,
            max: 2400,
            interval: 100
        );

        //from here on, ALL config is title-only
        configMenu.SetTitleScreenOnlyForNextOptions(
            mod: ModManifest,
            titleScreenOnly: true
            );

        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.UniqueDialogue.name"),
            tooltip: () => Helper.Translation.Get("config.UniqueDialogue.description"),
            getValue: () => Config.UniqueDialogue,
            setValue: value => Config.UniqueDialogue = value
        );

        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.AskForConfirmation.name"),
            tooltip: () => Helper.Translation.Get("config.AskForConfirmation.description"),
            getValue: () => Config.NeedsConfirmation,
            setValue: value => Config.NeedsConfirmation = value
        );
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.RejectionDialogues.name"),
            tooltip: () => Helper.Translation.Get("config.RejectionDialogues.description"),
            getValue: () => Config.RejectionDialogue,
            setValue: value => Config.RejectionDialogue = value
        );
        configMenu.AddTextOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.InLawComments.name"),
            tooltip: () => Helper.Translation.Get("config.InLawComments.description"),
            getValue: () => Config.InLawComments,
            setValue: value => Config.InLawComments = value,
            allowedValues: allowedStringVals,
            formatAllowedValue: value => Helper.Translation.Get($"config.InLawComments.values.{value}")
        );
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.ReplacerCompat.name"),
            tooltip: () => Helper.Translation.Get("config.ReplacerCompat.description"),
            getValue: () => Config.ReplacerCompat,
            setValue: value => Config.ReplacerCompat = value
        );

        //if the player doesn't have non destructive, don't allow walking on farm (to avoid destroying their things)
        if (hasNonDestructive == false)
        {
            Config.WalkOnFarm = false;
            Config.Shed = false;
            Config.Greenhouse = false;
        }
        else
        {
            configMenu.AddPage(
                ModManifest,
                "Places",
                () => TL.Get("config.Places.name")
            );

            configMenu.AddParagraph(
                ModManifest,
                () => Helper.Translation.Get("config.Places.description")
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.WalkOnFarm.name"),
                tooltip: () => Helper.Translation.Get("config.WalkOnFarm.description"),
                getValue: () => Config.WalkOnFarm,
                setValue: value => Config.WalkOnFarm = value
            );
            /*
            configMenu.AddBoolOption(
                ModManifest,
                getValue: () => Config.AnimalHomes,
                setValue: value => Config.AnimalHomes = value,
                name: () => Data.AnimalBuildingsTitle()
                );*/

            configMenu.AddBoolOption(
                ModManifest,
                getValue: () => Config.Greenhouse,
                setValue: value => Config.Greenhouse = value,
                name: () => Game1.content.LoadString("Strings/Buildings:Greenhouse_Name")
            );

            configMenu.AddBoolOption(
                ModManifest,
                getValue: () => Config.Shed,
                setValue: value => Config.Shed = value,
                name: () => Game1.content.LoadString("Strings/Buildings:Shed_Name")
            );
        }
        configMenu.AddPage(
            mod: ModManifest,
            pageId: "Debug",
            pageTitle: () => TL.Get("config.Debug.name")
        );
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.Debug.name"),
            tooltip: () => Helper.Translation.Get("config.Debug.Explanation"),
            getValue: () => Config.Debug,
            setValue: value => Config.Debug = value
        );
        configMenu.SetTitleScreenOnlyForNextOptions(
            mod: ModManifest,
            titleScreenOnly: false
            );
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.Verbose.name"),
            tooltip: () => Helper.Translation.Get("config.Verbose.description"),
            getValue: () => Config.Verbose,
            setValue: value => Config.Verbose = value
        );

        configMenu.AddPage(
            mod: ModManifest,
            pageId: "Sleepovers",
            pageTitle: () => TL.Get("config.Sleepovers")
            );

        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.SleepoverEnabled.name"),
            tooltip: () => Helper.Translation.Get("config.SleepoverEnabled.description"),
            getValue: () => Config.Sleepover,
            setValue: value => Config.Sleepover = value
        );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.SleepoverChance.name"),
            tooltip: () => Helper.Translation.Get("config.SleepoverChance.Explanation"),
            getValue: () => Config.SleepoverChance,
            setValue: value => Config.SleepoverChance = value,
            min: 0,
            max: 100,
            interval: 1
        );

        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.SleepoverMinHearts.name"),
            tooltip: () => Helper.Translation.Get("config.SleepoverMinHearts.Explanation"),
            getValue: () => Config.SleepoverMinHearts,
            setValue: value => Config.SleepoverMinHearts = value,
            min: 0,
            max: 12,
            interval: 1
        );
        #endregion
    }

    private static void DayStarted(object sender, DayStartedEventArgs e)
    {
#if DEBUG
        Log("Day Started", LogLevel.Info);
#endif
        TodaysVisitors.Clear();
        
        foreach (var player in Game1.getAllFarmers())
        {
            //TodaysVisitors.TryAdd(player.UniqueMultiplayerID, new List<string>());
            //PlayerHome[player.UniqueMultiplayerID] = Utility.getHomeOfFarmer(player);

            //if faulty config, don't do anything + mark as unvisitable
            if (!IsConfigValid)
            {
                Log("Configuration isn't valid. Mod will not work.", LogLevel.Warn);
                CanBeVisited = false;
                return;
            }

            //friendship data is reloaded.
            if (!FirstLoadedDay)
            {
                Log("Reloading data...");
                NameAndLevel?.TryAdd(player.UniqueMultiplayerID, new Dictionary<string, int>());
                RepeatedByLV?.TryAdd(player.UniqueMultiplayerID, new List<string>());

                Content.GetAllVisitors(player);
            }

            #region festival check
            /* if no friendship with anyone OR festival day:
             * make unvisitable & return
             */
            FestivalToday = Utility.isFestivalDay(Game1.dayOfMonth, Game1.season) || Utility.IsPassiveFestivalDay(Game1.dayOfMonth, Game1.season, Utility.getHomeOfFarmer(player).locationContextId);

            if (RepeatedByLV?.TryGetValue(player.UniqueMultiplayerID, out var repeatedByLevel) == false)
            {
                RepeatedByLV.TryAdd(player.UniqueMultiplayerID, new List<string>());
            }
            
            var anyInLv = RepeatedByLV?[player.UniqueMultiplayerID].Any() ?? false;
            Log($"isFestivalToday = {FestivalToday}; anyInLV = {anyInLv}");

            if (!anyInLv || FestivalToday)
            {
                CanBeVisited = false;
                return;
            }
            #endregion

            CanBeVisited = true;
        }

        //on winter, remove crop/animal data
        if (Game1.currentSeason == "winter")
        {
            Crops?.Clear();
            Animals?.Clear();
        }
        else
        {
            //if(Game1.player.mailReceived.Contains("ccPantry"))
            GreenhouseCrops = Values.GetCropsNameOnly(Utility.fuzzyLocationSearch("Greenhouse"));
            Crops = Values.GetCropsNameOnly(Game1.getFarm());
            Animals = Values.GetAnimals();
        }
    }

    private static void OnTimeChange(object sender, TimeChangedEventArgs e)
    {
        if (!Game1.IsMasterGame)
            return;
        
#if DEBUG
        Log("Time changed.");
#endif

        //on 610, fix children data
        if (e.OldTime == 600 || e.NewTime == 610)
        {
            Content.FixChildrenInfo();
            return;
        }

        var visitsOpen = e.NewTime > Config.StartingHours && e.NewTime < Config.EndingHours;
        var hasReachedMax = CounterToday >= Config.MaxVisitsPerDay;
        
        foreach (var player in Game1.getAllFarmers())
        {
#if DEBUG
            Log($"visitsOpen {visitsOpen}, hasReachedMax {hasReachedMax}, HasAnyVisitors {AreThereVisits(player)}");
#endif
            CheckVisitor(player, visitsOpen, hasReachedMax, e.NewTime, player.UniqueMultiplayerID);
        }
    }

    private static void CheckVisitor(Farmer player, bool visitsOpen, bool hasReachedMax, int newTime, long uid)
    {
        //if can visit & hasn't reached MAX
        if (visitsOpen && !hasReachedMax && !AreThereVisits(player))
        {
            var inFarmhouse = player.currentLocation.Equals(Utility.getHomeOfFarmer(player));
            var chanceMatch = Random.Next(1, 101) <= Config.CustomChance;

            if (chanceMatch && inFarmhouse)
                Content.ChooseRandom(uid);
            return;
        }

        var visitor = GetVisitor(uid);

        //if they're going to sleep, return
        if (VContext.ContainsKey(uid) == false || VContext.TryGetValue(uid, out var context) == false || context.IsGoingToSleep|| visitor is null)
            return;

        //in the future, add dialogue for when characters fall asleep.
        var soonToSleep = Values.IsCloseToSleepTime(context);

        //if custom visit and reached max time

        //if npc has stayed too long, check how to retire
        if (context.DurationSoFar >= MaxTimeStay || soonToSleep)
        {
            //if on a minigame and same location as visit (multiplayer)
            if (Game1.IsMultiplayer && Game1.currentMinigame is not null && player.currentLocation.Equals(visitor.currentLocation))
                return;
            
            if (NameAndLevel.TryGetValue(uid, out var nameAndLevel) == false)
                return;
            
            //sleepover bool checks: soon to sleep, enabled, % match, min hearts OK
            var shouldSleepOver = soonToSleep && Config.Sleepover && Game1.random.Next(0, 100) <= Config.SleepoverChance && Config.SleepoverMinHearts <= nameAndLevel[visitor.Name];

            //log, LV depends on config
            var action = shouldSleepOver ? "resting" : "retiring";
            Log($"{visitor.Name} is {action} for the day.", Level);

            //update info
            //ModEntry.Visitors?.Remove(Name);
            CounterToday++;
            
            TodaysVisitors.Add(visitor.Name);

            //get data before we remove it
            var durationSoFar = context.DurationSoFar;
            var controllerTime = context.ControllerTime;
            ForcedSchedule = false;

            if (Config.Debug)
            {
                Log($"HasAnyVisitors = false, CounterToday = {CounterToday}, TodaysVisitors= {Data.TurnToString(TodaysVisitors)}, DurationSoFar = {durationSoFar}, ControllerTime = {controllerTime}, VisitorName = {visitor.Name}", Level);
            }

            if (shouldSleepOver)
            {
                Actions.GoToSleep(visitor, VContext[uid], uid);
            }
            else
            {
                Actions.Retire(visitor, uid);
            }

            return;
        }

        //otherwise, they'll try to move around.
        Log($"{visitor.Name} will move around.", Level);

        if (Config.Verbose)
        {
            var endPoint = visitor.controller?.pathToEndPoint;
            if (endPoint != null)
                Log($"Current endpoint: {Data.TurnToString(endPoint)},moving: {visitor.isMovingOnPathFindPath}", LogLevel.Debug);
        }

        //if they just arrived they won't move.
        if (newTime == context.TimeOfArrival)
        {
            Log($"Time of arrival equals current time. NPC won't move around");
        }
        //if they've been moving too long, they'll stop
        else if (VContext?[uid] != null && visitor != null && context.ControllerTime != 0)
        {
            VContext[uid].ControllerTime = 0;
            visitor.Halt();
            visitor.temporaryController = null;
            visitor.controller = null;
            if (Config.Debug)
            {
                Log($"ControllerTime = {VContext[uid].ControllerTime}", Level);
            }
        }
        //otherwise, will try moving.
        else if (VContext?[uid] != null && visitor != null)
        {
            visitor.resetCurrentDialogue();
            VContext[uid].ControllerTime++;
            
            // ReSharper disable once RedundantAssignment
            var newTile = Point.Zero;

            //walk on farm OR house
            if (visitor.currentLocation.Name == "Farm")
            {
                if (Config.Verbose)
                    Log("Current is farm.");

                //Visitor.Idle = true;
                newTile = Data.RandomSpotInSquare(visitor, 10);
                visitor.controller = new PathFindController(
                    visitor,
                    visitor.currentLocation,
                    newTile,
                    Game1.random.Next(0, 4)
                )
                {
                    endPoint = newTile
                };
            }
            else if (visitor.currentLocation is FarmHouse f)
            {
                if (Config.Verbose)
                    Log("Current is farmhouse.");

                newTile = f.getRandomOpenPointInHouse(Game1.random);
                visitor.controller = new PathFindController(
                    visitor,
                    visitor.currentLocation,
                    newTile,
                    Game1.random.Next(0, 4)
                );
            }
            else
            {
                if (Config.Verbose)
                    Log("Current is probably a shed or greenhouse.");

                newTile = Data.RandomTile(visitor.currentLocation, visitor).ToPoint();

                //stop JIC
                visitor.Halt();
                visitor.temporaryController = null;
                visitor.controller = null;

                visitor.controller = new PathFindController(
                    visitor,
                    visitor.currentLocation,
                    newTile,
                    Game1.random.Next(0, 4)
                )
                {
                    endPoint = newTile
                };
            }

            if (Config.Debug)
                Log($"New position: {newTile}, pathing to {visitor.controller.endPoint}", LogLevel.Debug);
        }

        CheckForDialogue(uid);

        VContext[uid].DurationSoFar++;

        if (Config.Debug)
        {
            Log($"ControllerTime = {VContext[uid].ControllerTime}, DurationSoFar = {VContext[uid].DurationSoFar} ({VContext[uid].DurationSoFar * 10} minutes).", Level);
        }
    }

    private static bool AreThereVisits(Farmer player)
    {
        foreach (var npc in Utility.getHomeOfFarmer(player).characters)
        {
            //skip married/roommate
            if (player.friendshipData.TryGetValue(npc.Name, out var data) && data.IsMarried())
                continue;
            
            //skip pets
            if (npc is StardewValley.Characters.Pet)
                continue;
            
            return true;
        }

        return false;
    }

    private static void CheckForDialogue(long uid)
    {
        //check % for random dialogue
        if (Game1.random.Next(0, 11) > 5)
            return;

        var visitor = GetVisitor(uid);
        var isFarm = visitor.currentLocation.Name == "Farm";
        var furniture = visitor.currentLocation.furniture;
        var isFarmhouse = visitor.currentLocation is FarmHouse;
        var isShed = visitor.currentLocation is Shed;
        //if in farm
        if (isFarm)
        {
            var anyCrops = Crops.Any();

            if (Game1.currentSeason == "winter")
            {
                Actions.SetDialogue(visitor, Values.GetDialogueType(visitor, DialogueType.Winter));
            }
            else if ((Game1.random.Next(0, 2) <= 0 || !anyCrops) && Animals.Any())
            {
                var animal = Game1.random.ChooseFrom(Animals);
                var rawtext = Values.GetDialogueType(visitor, DialogueType.Animal);
                var formatted = string.Format(rawtext, animal);
                Actions.SetDialogue(visitor, formatted);
            }
            else if (anyCrops)
            {
                var crop = Game1.random.ChooseFrom(Crops);
                var rawtext = Values.GetDialogueType(visitor, DialogueType.Crop);
                var formatted = string.Format(rawtext, crop);
                Actions.SetDialogue(visitor, formatted);
            }
            else
            {
                Actions.SetDialogue(visitor, Values.GetDialogueType(visitor, DialogueType.NoneYet));
            }
        }
        //if in shed/house and any furniture
        else if ((isFarmhouse || isShed) && furniture.Any())
        {
            var text = Values.GetDialogueType(visitor, DialogueType.Furniture);
            var formatted = string.Format(text, Game1.random.ChooseFrom(furniture).DisplayName);
            Actions.SetDialogue(visitor, formatted);

            if (Config.Debug)
                Log($"Adding dialogue for {visitor.Name}...", Level);
        }
        else
        {
            var isCoopOrBarn = visitor.currentLocation.Name.Contains("Coop") || visitor.currentLocation.Name.Contains("Barn");
            var isGreenHouse = visitor.currentLocation.Name == "Greenhouse";

            if (isGreenHouse)
            {
                var crops = GreenhouseCrops;
                if (crops == null || crops.Count == 0)
                    return;

                var chosen = Game1.random.ChooseFrom(crops);
                var text = Values.GetDialogueType(visitor, DialogueType.Crop);
                var formatted = string.Format(text, chosen);
                Actions.SetDialogue(visitor, formatted);
                if (Config.Debug)
                    Log($"Adding dialogue for {visitor.Name}...", Level);
            }
            else if (isCoopOrBarn)
            {
                var animals = visitor.currentLocation.getAllFarmAnimals();

                if (animals == null || animals.Count == 0)
                    return;

                var chosen = Game1.random.ChooseFrom(animals);
                var text = Values.GetDialogueType(visitor, DialogueType.Animal);
                var formatted = string.Format(text, chosen.displayName);
                Actions.SetDialogue(visitor, formatted);
                if (Config.Debug)
                    Log($"Adding dialogue for {visitor.Name}...", Level);
            }
        }
    }

    internal static NPC GetVisitor(long uid)
    {
        var player = Game1.GetPlayer(uid);
        if (player == null)
        {
            Log($"Couldn't find player with ID {uid}.", LogLevel.Warn);
            return null;
        }
        
        foreach (var npc in Utility.getHomeOfFarmer(player).characters)
        {
            //skip married/roommate
            if (player.friendshipData.TryGetValue(npc.Name, out var data) && data.IsMarried())
                continue;
            
            //skip pets
            if (npc is StardewValley.Characters.Pet)
                continue;
            
            return npc;
        }

        return null;
    }

    /// <summary>
    /// Set all used visitor variables to null.
    /// </summary>
    //its here because i'd rather have a single call than repeat through code and forget one of them
    internal static void SetNoVisitor(long uid)
    {
        if (VContext.TryAdd(uid, null) == false)
            VContext[uid] = null;
    }

    #region used by visitors
    internal static Dictionary<string, List<string>> RetiringDialogue { get; set; } = new();
    internal static Dictionary<string, List<string>> InlawDialogue { get; set; } = new();
    //internal static List<string> FurnitureList { get; set; } = new();
    internal static List<string> Animals { get; private set; } = new();
    internal static List<string> Crops { get; private set; } = new();
    internal static List<string> GreenhouseCrops { get; private set; } = new();
    //internal static Character Puppet { get; set; }
    #endregion

    #region used by mod
#if DEBUG
    internal const LogLevel Level = LogLevel.Debug;
#else
    // ReSharper disable once UnusedMember.Local
    internal const LogLevel Level =  LogLevel.Trace;
#endif

    internal static Action<string, LogLevel> Logger { get; private set; }
    internal static void Log(string data, LogLevel type = Level) => Logger(data, type);

    internal static ITranslationHelper TL { get; private set; }
    internal static IModHelper Help { get; set; }

    private static Random Random
    {
        get
        {
            random ??= new Random((int)Game1.uniqueIDForThisGame * 26 + (int)(Game1.stats.DaysPlayed * 36));
            return random;
        }
    }
    private static Random random;
    #endregion

    #region player data
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    internal static Dictionary<long, Dictionary<string, List<string>>> InLaws { get; private set; } = new();
    internal static Dictionary<long, Dictionary<string, int>> NameAndLevel { get; private set; } = new();
    internal static Dictionary<long, List<string>> RepeatedByLV { get; set; } = new();
    internal static List<string> BlacklistParsed { get; set; } = new();
    internal static bool FirstLoadedDay;
    private static bool CanBeVisited { get; set; }
    internal static ModConfig Config;
    #endregion

    #region configurable
    internal static string BlacklistRaw { get; set; }
    internal static bool IsConfigValid { get; set; }
    #endregion

    internal static Dictionary<long,VisitData> VContext { get; set; } = new();
    internal static int MaxTimeStay { get; set; }
    internal static int CounterToday { get; set; }
    public static bool ForcedSchedule { get; internal set; }
    internal static List<string> TodaysVisitors { get; set; } = new();

    private static bool FestivalToday;
    internal static List<string> MarriedNPCs { get; private set; } = new();
}
