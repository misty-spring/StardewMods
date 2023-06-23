using System;
using System.Collections.Generic;
using System.Linq;
using GenericModConfigMenu;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

// ReSharper disable InconsistentNaming

namespace FarmVisitors
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.GameLaunched += GameLaunched;
            helper.Events.GameLoop.SaveLoaded += SaveLoaded;

            helper.Events.GameLoop.DayStarted += DayStarted;
            helper.Events.GameLoop.TimeChanged += OnTimeChange;
            helper.Events.GameLoop.DayEnding += DayEnding;

            helper.Events.Player.Warped += FarmOutside.PlayerWarp;

            helper.Events.GameLoop.ReturnedToTitle += TitleReturn;
            helper.Events.Content.AssetRequested += Extras.AssetRequest;

            Config = Helper.ReadConfig<ModConfig>();

            Log = Monitor.Log;
            TL = Helper.Translation;

            if (Config.Debug)
            {
                helper.ConsoleCommands.Add("print", "List the values requested.", Debugging.Print);
                helper.ConsoleCommands.Add("vi_reload", "Reload visitor info.", Reload);
                helper.ConsoleCommands.Add("vi_force", "Force a visit to happen.", Debugging.ForceVisit);
            }
        }

        #region hooks
        
        private void GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            if (Config.Debug)
            {
                Monitor.Log("Debug has been turned on. This will change configuration for testing purposes.", LogLevel.Warn);

                Monitor.Log("Chance set to 100 (% every 10 min)");
                Config.CustomChance = 100;
                Monitor.Log("Starting hour will be 600.");
                Config.StartingHours = 600;
            }

            var allowedStringVals = new[]
            {
                "VanillaOnly",
                "VanillaAndMod",
                "None"
            };

            //clear values. better safe than sorry
            ClearValues();

            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;
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
                pageId: "Extras"
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
                min: 600,
                max: 2400,
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

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.WalkOnFarm.name"),
                tooltip: () => Helper.Translation.Get("config.WalkOnFarm.description"),
                getValue: () => Config.WalkOnFarm,
                setValue: value => Config.WalkOnFarm = value
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
        }
        
        private void SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            _firstLoadedDay = true;

            //clear ALL values and temp data on load. this makes sure there's no conflicts with savedata cache (e.g if player had returned to title)
            ClearValues();
            CleanTempData();

            //check config
            if (Config.StartingHours >= Config.EndingHours)
            {
                Monitor.Log("Starting hours can't happen after ending hours! To use the mod, fix this and reload savefile.", LogLevel.Warn);
                IsConfigValid = false;
                return;
            }

            Monitor.Log("User config is valid.");
            IsConfigValid = true;

            //get all possible visitors- which also checks blacklist and married NPCs, etc.
            GetAllVisitors();

            /* if allowed, get all inlaws. 
             * this doesn't need daily reloading. NPC dispositions don't vary 
             * (WON'T add compat for the very small % of mods with conditional disp., its an expensive action)*/
            if (Config.InLawComments is "VanillaAndMod")
            {
                Monitor.Log("Getting all in-laws...");
                var tempdict = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
                foreach (var name in NameAndLevel.Keys)
                {
                    var temp = Moddeds.GetInlawOf(tempdict, name);
                    if (temp is not null)
                    {
                        InLaws.Add(name, temp);
                    }
                }
            }
        }
        
        private void DayStarted(object sender, DayStartedEventArgs e)
        {
            //if faulty config, don't do anything + mark as unvisitable
            if (IsConfigValid == false)
            {
                Monitor.Log("Configuration isn't valid. Mod will not work.", LogLevel.Warn);
                CanBeVisited = false;
                return;
            }

            //every sunday, friendship data is reloaded.
            if (Game1.dayOfMonth % 7 == 0 && _firstLoadedDay == false)
            {
                Monitor.Log("Day is sunday and not first loaded day. Reloading data...");
                NameAndLevel?.Clear();
                RepeatedByLV?.Clear();

                GetAllVisitors();
            }
            
            /* if no friendship with anyone OR festival day:
             * make unvisitable
             * return (= dont check custom schedule)
             */
            isFestivalToday = Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason);
            var anyInLv = RepeatedByLV != null && RepeatedByLV.Any();
            //log info
            Monitor.Log($"isFestivalToday = {isFestivalToday}; anyInLV = {anyInLv}");

            if (anyInLv == false || isFestivalToday)
            {
                CanBeVisited = false;
                return;
            }

            CanBeVisited = true;

            //update animal and crop list
            if (Game1.currentSeason == "winter")
            {
                Crops?.Clear();
                Animals?.Clear();
            }
            else
            {
                Crops = Values.GetCrops();
                Animals = Values.GetAnimals();
            }
        }
        
        private void OnTimeChange(object sender, TimeChangedEventArgs e)
        {
            if (!CanBeVisited)
            {
                return;
            }
            
            //if first day playing or sunday
            if ((e.OldTime == 600 || e.NewTime == 610) && (_firstLoadedDay || Game1.dayOfMonth % 7 == 0))
            {
                FixChildrenInfo();
                return;
            }
            
            //if avaiable visit time and hasn't reached MAX visitors yet
            if (e.NewTime > Config.StartingHours && e.NewTime < Config.EndingHours && CounterToday < Config.MaxVisitsPerDay)
            {
                if (!HasAnyVisitors && HasAnySchedules) //custom
                {
                    foreach (var pair in SchedulesParsed)
                    {
                        var visit = Game1.getCharacterFromName(pair.Key);
                        
                        if (!e.NewTime.Equals(pair.Value.From) || !Values.IsFree(visit, false)) continue;
                        
                        visit.IsInvisible = true;
                        Visitor = new DupeNPC(visit)
                        {
                        DurationSoFar = 0,
                        TimeOfArrival = e.NewTime,
                        ControllerTime = 0,
                        CustomVisiting = true,
                        CustomData = pair.Value
                        };
                        
                        HasAnyVisitors = true;

                        if(pair.Value.Force.Enable)
                        {
                            Monitor.Log($"Adding NPC {Visitor.Name} by force (scheduled, Force.Enable = {pair.Value.Force.Enable})...");

                            Actions.AddWhileOutside(Visitor);

                            ForcedSchedule = true; //set forced to true. (avoids certain behavior)

                            var mail = pair.Value.Force.Mail;
                            if(!string.IsNullOrWhiteSpace(mail)) //if there's a mail string, add for tomorrow
                            {
                                Game1.player.mailForTomorrow.Add(mail);
                            }
                            return;
                        }

                        if (Config.NeedsConfirmation)
                        {
                            AskToEnter();
                        }
                        else
                        {
                            //add them to farmhouse (last to avoid issues)
                            Actions.AddCustom(Visitor, farmHouse, Visitor.CustomData,false);

                            if (Config.Verbose)
                            {
                                Monitor.Log($"\nHasAnyVisitors set to true.\n{Visitor.Name} will begin visiting player.\nTimeOfArrival = {Visitor.TimeOfArrival};\nControllerTime = {Visitor.ControllerTime};", LogLevel.Debug);
                            }
                        }

                        break;
                    }
                }
                if (!HasAnyVisitors) //random
                {
                    if (Random.Next(1, 101) <= Config.CustomChance && Game1.currentLocation.Equals(farmHouse))
                    {
                        ChooseRandom();
                    }
                }
            }

            //if there's no current visitors, do nothing
            if (!HasAnyVisitors) return;

            //in the future, add unique dialogue for when characters fall asleep in your house.
            var soonToSleep = Values.IsCloseToSleepTime(Visitor);
            //if custom visit and reached max time
            var maxCustomTime = Visitor.CustomVisiting && e.NewTime >= Visitor.CustomData?.To;
            
            //if npc has stayed too long, check how to retire
            if (Visitor.DurationSoFar >= MaxTimeStay || maxCustomTime || soonToSleep)
            {
                var shouldSleepOver = soonToSleep && Config.Sleepover && Random.Next(0, 100) <= Config.SleepoverChance && Config.SleepoverMinHearts <= NameAndLevel[Visitor.Name];
                
                Monitor.Log($"{Visitor.Name} is retiring for the day.");

                //if custom AND has custom dialogue: exit with custom. else normal
                if (Visitor.CustomVisiting)
                {
                    var exitd = Visitor.CustomData?.ExitDialogue;
                    if (!string.IsNullOrWhiteSpace(exitd))
                        Actions.RetireCustom(Visitor, farmHouse, exitd);
                    else
                        Actions.Retire(Visitor, farmHouse);
                }
                else if (shouldSleepOver)
                {
                    Actions.GoToSleep(Visitor);
                }
                else
                {
                    Actions.Retire(Visitor, farmHouse);
                }

                HasAnyVisitors = false;
                CounterToday++;
                TodaysVisitors.Add(Visitor.Name);
                var durationSoFar = Visitor.DurationSoFar;
                var controllerTime = Visitor.ControllerTime;
                ForcedSchedule = false;

                if (Config.Verbose)
                {
                    Monitor.Log($"HasAnyVisitors = false, CounterToday = {CounterToday}, TodaysVisitors= {Actions.TurnToString(TodaysVisitors)}, DurationSoFar = {durationSoFar}, ControllerTime = {controllerTime}, VisitorName = {Visitor?.Name}", LogLevel.Debug);
                }
                return;
            }
            //otherwise, they'll try to move around.
            if (Config.Verbose)
            {
                Monitor.Log($"{Visitor.Name} will move around.", LogLevel.Debug);
            }

            //if they just arrived they won't move.
            if (e.NewTime.Equals(Visitor.TimeOfArrival))
            {
                Monitor.Log("Time of arrival equals current time. NPC won't move around", LogLevel.Debug);
            }
            //if they've been moving too long, they'll stop
            else if (Visitor.ControllerTime != 0)
            {
                Visitor.Halt();
                Visitor.temporaryController = null;
                Visitor.controller = null;
                Visitor.ControllerTime = 0;
                if (Config.Verbose)
                {
                    Monitor.Log($"ControllerTime = {Visitor.ControllerTime}", LogLevel.Debug);
                }
            }
            //otherwise, will try moving.
            else
            {
                Visitor.DurationSoFar++;
                Visitor.ControllerTime++;
                

                //walk on farm
                if (Visitor.IsOutside)
                {
                    FarmOutside.WalkAroundFarm(Visitor);
                    Monitor.Log($"ControllerTime = {Visitor.ControllerTime}, DurationSoFar = {Visitor.DurationSoFar} ({Visitor.DurationSoFar * 10} minutes).", LogLevel.Debug);
                }
                else
                {
                    Visitor.controller = new PathFindController(
                        Visitor,
                        Visitor.currentLocation,
                        Actions.GetRandomTile(Visitor.currentLocation),
                        Random.Next(0, 4)
                    );
                }

                if (Visitor.CustomVisiting)
                {
                    if(Config.Verbose)
                        Monitor.Log("Checking if NPC has any custom dialogue...");
                        
                    var hasCustomDialogue = false;
                    try
                    {
                        var AnyDialogue = Visitor.CustomData?.Dialogues.Any();
                        if (AnyDialogue != null) hasCustomDialogue = (bool)AnyDialogue;
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch (Exception)
                    {
                    }

                    if(hasCustomDialogue)
                    {
                        Visitor.setNewDialogue(Visitor.CustomData.Dialogues[0], true);

                        Monitor.Log($"Adding custom dialogue for {Visitor.Name}...");
                                
                        if (Config.Verbose)
                            Monitor.Log($"C. Dialogue: {Visitor.CustomData.Dialogues[0]}", LogLevel.Debug);

                        //remove this dialogue from the queue
                        Visitor.CustomData.Dialogues.RemoveAt(0);
                    }
                }
                        
                else if (Random.Next(0, 11) <= 5 && FurnitureList.Any())
                {
                    Visitor.setNewDialogue(
                        string.Format(
                            Values.GetDialogueType(
                                Visitor,
                                DialogueType.Furniture),
                            Values.GetRandomObj
                                (ItemType.Furniture)),
                        true);

                    if (Config.Verbose)
                    {
                        Monitor.Log($"Adding dialogue for {Visitor.Name}...", LogLevel.Debug);
                    }
                }
                if (Config.Verbose)
                {
                    Monitor.Log($"ControllerTime = {Visitor.ControllerTime}", LogLevel.Debug);
                }
            }

            Visitor.DurationSoFar++;
            if (Config.Verbose)
            {
                Monitor.Log($"DurationSoFar = {Visitor.DurationSoFar} ({Visitor.DurationSoFar * 10} minutes).", LogLevel.Debug);
            }
        }
      
        private void DayEnding(object sender, DayEndingEventArgs e)
        {
            _firstLoadedDay = false;

            CleanTempData();

            SchedulesParsed?.Clear();

            if (Config.Verbose)
            {
                Monitor.Log("Clearing today's visitor list, visitor count, and all other temp info...", LogLevel.Debug);
            }
        }
        
        private void TitleReturn(object sender, ReturnedToTitleEventArgs e)
        {
            ClearValues();

            Monitor.Log($"Removing cached information: HasAnyVisitors= {HasAnyVisitors}, TimeOfArrival={Visitor.TimeOfArrival}, CounterToday={CounterToday}, VisitorName={Visitor.Name}. TodaysVisitors, NameAndLevel, FurnitureList, and RepeatedByLV cleared.");
        }
#endregion
        
        #region mod methods
        
        private void FixChildrenInfo()
        {
            var hasC2N = Helper.ModRegistry.Get("Loe2run.ChildToNPC") != null;
            var hasLNPCs = Helper.ModRegistry.Get("Candidus42.LittleNPCs") != null;

            if (!hasC2N && !hasLNPCs) return;
            
            //check all npcs in farmhouse, since C2N and LNPCs add them at first timechange (iirc)
            foreach (var chara in Utility.getHomeOfFarmer(Game1.player).characters)
            {
                //if npc isnt married to farmer, assuming it's a child.
                if (chara.isMarried() || chara.isRoommate()) continue;
                try
                {
                    NameAndLevel.Remove(chara.Name);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
        
        private void ParseBlacklist()
        {
            Monitor.Log("Getting raw blacklist.");
            BlacklistRaw = Config.Blacklist;
            if (BlacklistRaw is null)
            {
                Monitor.Log("No characters in blacklist.");
            }

            var charsToRemove = new[] { "-", ",", ".", ";", "\"", "\'", "/" };
            foreach (var c in charsToRemove)
            {
                BlacklistRaw = BlacklistRaw?.Replace(c, string.Empty);
            }
            if (Config.Verbose)
            {
                Monitor.Log($"Raw blacklist: \n {BlacklistRaw} \nWill be parsed to list now.", LogLevel.Debug);
            }
            BlacklistParsed = BlacklistRaw?.Split(' ').ToList();
        }
        private void CleanTempData()
        {
            CounterToday = 0;
            Visitor = null;
            HasAnyVisitors = false;
            TodaysVisitors?.Clear();

            if (MaxTimeStay != (Config.Duration - 1))
            {
                MaxTimeStay = (Config.Duration - 1);
                Monitor.Log($"MaxTimeStay = {MaxTimeStay}; Config.Duration = {Config.Duration};");
            }

            Animals?.Clear();
            Crops?.Clear();
            FurnitureList?.Clear();
            FurnitureList = Values.UpdateFurniture(Utility.getHomeOfFarmer(Game1.player));

            if (Config.Verbose)
            {
                Monitor.Log($"Furniture list updated. Count: {FurnitureList?.Count ?? 0}", LogLevel.Debug);
            }
        }
        private void ClearValues()
        {
            InLaws.Clear();
            NameAndLevel?.Clear();
            RepeatedByLV?.Clear();
            TodaysVisitors?.Clear();
            FurnitureList?.Clear();
            SchedulesParsed?.Clear();
            BlacklistRaw = null;
            BlacklistParsed?.Clear();
            Locations?.Clear();
            
            if (!string.IsNullOrWhiteSpace(Config.Blacklist))
            {
                ParseBlacklist();
            }

            CounterToday = 0;
            HasAnyVisitors = false;
            Visitor = null;
        }
        private void ReloadCustomschedules()
        {
            Monitor.Log("Began reloading custom schedules.");

            SchedulesParsed?.Clear();

            var schedules = Game1.content.Load<Dictionary<string, ScheduleData>>("mistyspring.farmhousevisits/Schedules");

            if (schedules.Any())
            {
                foreach (var pair in schedules)
                {
                    Monitor.Log($"Checking {pair.Key}'s schedule...");
                    var isPatchValid = Extras.IsScheduleValid(pair);

                    if (!isPatchValid)
                    {
                        Monitor.Log($"{pair.Key} schedule won't be added.", LogLevel.Error);
                    }
                    else
                    {
                        SchedulesParsed?.Add(pair.Key, pair.Value); //NRE
                    }
                }
            }

            HasAnySchedules = SchedulesParsed?.Any() ?? false;

            Monitor.Log("Finished reloading custom schedules.");
        }

        //below methods: REQUIRED, dont touch UNLESS it's for bug-fixing
        internal static void ChooseRandom()
        {
            Log("Getting random...",LogLevel.Trace);
            var RChoice = Random.Next(0, (RepeatedByLV.Count));

            var visitorName = RepeatedByLV[RChoice];
            Log($"Random: {RChoice}; VisitorName= {visitorName}",LogLevel.Trace);

            var visit = Game1.getCharacterFromName(visitorName);

            if (!Values.IsFree(visit, true)) return;
            
            visit.IsInvisible = true;
            //save values
            Visitor = new DupeNPC(visit)
            {
                CustomVisiting = false,
                DurationSoFar = 0,
                TimeOfArrival = Game1.timeOfDay,
                ControllerTime = 0
            };

            HasAnyVisitors = true;


            if (Config.NeedsConfirmation)
            {
                AskToEnter();
            }
            else
            {
                //add them to farmhouse (last to avoid issues)
                Actions.AddToFarmHouse(Visitor, farmHouse, false);

                if (Config.Verbose)
                {
                    Log($"\nHasAnyVisitors set to true.\n{Visitor.Name} will begin visiting player.\nTimeOfArrival = {Visitor.TimeOfArrival};\nControllerTime = {Visitor.ControllerTime};", LogLevel.Debug);
                }
            }
        }
        private void GetAllVisitors()
        {
            if (!IsConfigValid)
            {
                return;
            }

            SearchAllPossible();
            
            Monitor.Log("Began obtaining all visitors.");
            if (!string.IsNullOrWhiteSpace(Config.Blacklist))
            {
                ParseBlacklist();
            }
            NpcNames = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions").Keys.ToList();

            MaxTimeStay = (Config.Duration - 1);
            Monitor.Log($"MaxTimeStay = {MaxTimeStay}; Config.Duration = {Config.Duration};");

            farmHouse = Utility.getHomeOfFarmer(Game1.player);

            //get all friended excluding children and spouses/divorced
            foreach (var pair in Game1.player.friendshipData.Pairs)
            {
                if (!NpcNames.Contains(pair.Key)) continue;
                
                if (Config.Debug)
                {
                    Monitor.Log($"Name: {pair.Key}");
                }

                var hearts = pair.Value.Points / 250;

                var isDivorced = pair.Value.IsDivorced();
                var isMarried = pair.Value.IsMarried() || pair.Value.IsRoommate();

                if (isMarried == false && hearts is not 0)
                {
                    if (BlacklistParsed is not null)
                    {
                        if (BlacklistParsed.Contains(pair.Key))
                        {
                            Monitor.Log($"{pair.Key} is in the blacklist.", LogLevel.Info);
                        }
                        else
                        {
                            NameAndLevel.Add(pair.Key, hearts);
                        }
                    }
                    else if (isDivorced)
                    {
                        Monitor.Log($"{pair} is Divorced.");
                    }
                    else
                    {
                        if (pair.Key.Equals("Dwarf"))
                        {
                            if (!Game1.player.canUnderstandDwarves)
                                Monitor.Log("Player can't understand dwarves yet!");

                            else
                                NameAndLevel.Add(pair.Key, hearts);
                        }
                        else
                            NameAndLevel.Add(pair.Key, hearts);
                    }
                }
                else
                {
                    if (isMarried)
                    {
                        MarriedNPCs.Add(pair.Key);
                        Monitor.Log($"Adding {pair.Key} to married list...");
                    }

                    if (isDivorced)
                    {
                        Monitor.Log($"{pair.Key} is Divorced. They won't visit player");
                    }
                    else if (Config.Verbose)
                    {
                        Monitor.Log($"{pair.Key} won't be added to the visitor list.", LogLevel.Debug);
                    }
                }
            }
            
            //log all npcs friended
            var call = "\n Name   | Hearts\n--------------------";
            foreach (var pair in NameAndLevel)
            {
                var fixedstr = pair.Key + "               ".Remove(0, pair.Key.Length);
                
                call += $"\n   {fixedstr} {pair.Value}";

                var tempdict = Enumerable.Repeat(pair.Key, pair.Value).ToList();
                RepeatedByLV.AddRange(tempdict);
            }
            
            Monitor.Log(call);

            FurnitureList?.Clear();
            FurnitureList = Values.UpdateFurniture(farmHouse);
            Monitor.Log($"Furniture count: {FurnitureList.Count}");

            Monitor.Log("Finished obtaining all visitors.");

            ReloadCustomschedules();
        }

        private static void SearchAllPossible()
        {
            Locations?.Clear();
            
            //get farmhouse valid tiles
            var home = Utility.getHomeOfFarmer(Game1.player);
            FarmOutside.DoFloodFill(home,home.getEntryLocation());
            
            //get farm valid tiles
            if (!Config.WalkOnFarm) return;
            var farmEntry = Game1.getFarm().GetMainFarmHouseEntry();
            FarmOutside.DoFloodFill(Game1.getFarm(),new Point(farmEntry.X,farmEntry.Y++));
/*
            //get all sheds
            foreach (var building in Game1.getFarm().buildings)
            {
                if (building.indoors.Value is not Shed) return;
                
                FarmOutside.DoFloodFill(Utility.fuzzyLocationSearch(building.nameOfIndoors),building.getPointForHumanDoor());
            }*/
        }

        //if user wants confirmation for NPC to come in
        private static void AskToEnter()
        {
            //knock on door
            DelayedAction.playSoundAfterDelay("stoneStep",300,Game1.player.currentLocation);
            DelayedAction.playSoundAfterDelay("stoneStep",600,Game1.player.currentLocation);
            DelayedAction.playSoundAfterDelay("stoneStep",900,Game1.player.currentLocation);

            //get name, place in question string
            var displayName = Visitor.displayName;
            var formattedQuestion = string.Format(TL.Get("Visit.AllowOrNot"), displayName);

            var res = Game1.player.currentLocation.createYesNoResponses();
            Game1.player.currentLocation.createQuestionDialogue(formattedQuestion, res, _entryAfterQuestion);
        }
        private static void CancelVisit()
        {
            var name = Visitor.Name;
            TodaysVisitors.Add(name);
            if (Config.RejectionDialogue)
            {
                Game1.drawDialogue(Visitor, Values.GetDialogueType(Visitor,DialogueType.Rejected));
            }

            HasAnyVisitors = false;
            Visitor = null;
            
            Game1.getCharacterFromName(name).IsInvisible = false;
        }
        private static void Proceed()
        {
            if (Visitor.CustomVisiting)
            {
                Actions.AddCustom(Visitor, farmHouse, Visitor.CustomData, true);
            }
            else
            {
                Actions.AddToFarmHouse(Visitor, farmHouse, true);
            }
        }
        private static void _entryAfterQuestion(Farmer who, string whichAnswer)
        {
            if (whichAnswer == "Yes") Proceed();
            else CancelVisit();
        }

        /*  console commands  */
        private void Reload(string command, string[] arg2) => GetAllVisitors();
        public static void SetFromCommand(NPC visit)
        {
            Visitor = new DupeNPC(visit)
            {
                DurationSoFar = 0,
                TimeOfArrival = Game1.timeOfDay,
                ControllerTime = 0
            };

            HasAnyVisitors = true;

            Log($"\nHasAnyVisitors set to true.\n{Visitor.Name} will begin visiting player.\nTimeOfArrival = {Visitor.TimeOfArrival};\nControllerTime = {Visitor.ControllerTime};", LogLevel.Info);
        }
#endregion

        #region used by mod
        internal static List<string> FurnitureList { get; private set; } = new();
        internal static List<string> Animals { get; private set; } = new();
        internal static Dictionary<int, string> Crops { get; private set; } = new();

        internal static Action<string, LogLevel> Log { get; private set; }
        internal static ITranslationHelper TL { get; private set; }
        internal static DupeNPC Visitor { get; set; }

        private static Random Random
        {
            get
            {
                random ??= new Random(((int)Game1.uniqueIDForThisGame * 26) + (int)(Game1.stats.DaysPlayed * 36));
                return random;
            }
        }

        private bool _firstLoadedDay;
        private static FarmHouse farmHouse { get; set; }

        #endregion
        
        #region player data
        internal static ModConfig Config;
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private static List<string> RepeatedByLV = new();
        private static Random random;
        private static bool isFestivalToday;
        private static bool CanBeVisited;
        internal static List<string> BlacklistParsed { get; private set; } = new();
        #endregion
        #region configurable

        private static string BlacklistRaw { get; set; }
        private bool IsConfigValid { get; set; }
        private static bool HasAnyVisitors { get; set; }
        private static bool HasAnySchedules { get; set; }
#endregion
        
        #region visitdata
        private int CounterToday { get; set; }
        private int MaxTimeStay { get; set; }
        public static bool ForcedSchedule { get; private set; }
#endregion
        #region game information
        internal static Dictionary<string,List<Point>> Locations { get; set; } = new ();
        internal static Dictionary<string, int> NameAndLevel { get; private set; } = new();
        internal static List<string> NpcNames { get; private set; } = new();
        private static Dictionary<string, ScheduleData> SchedulesParsed { get; set; } = new();
        internal static Dictionary<string, List<string>> InLaws { get; private set; } = new();
        internal static List<string> MarriedNPCs { get; private set; } = new();
        internal static List<string> TodaysVisitors { get; set; } = new();
        //internal static readonly char[] Slash = new char['/'];
#endregion
    }
}