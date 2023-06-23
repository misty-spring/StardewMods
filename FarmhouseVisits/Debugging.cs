using System;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using static FarmVisitors.ModEntry;

namespace FarmVisitors
{
    internal static class Debugging
    {
        public static void ForceVisit(string command, string[] arg2)
        {
            var farmHouse = Utility.getHomeOfFarmer(Game1.player);

            if (Context.IsWorldReady)
            {
                if (Game1.player.currentLocation.Equals(farmHouse))
                {
                    try
                    {
                        if (arg2 is null || arg2.Length == 0)
                        {
                            ChooseRandom();
                        }
                        else if (NpcNames.Contains(arg2[0]))
                        {
                            Log($"VisitorName= {arg2[0]}", LogLevel.Trace);

                            if (!TodaysVisitors.Contains(arg2[0]) || arg2[1] is "force")
                            {
                                //save values
                                Visitor = new DupeNPC(Game1.getCharacterFromName(arg2[0]));
                                //add them to farmhouse
                                Actions.AddToFarmHouse(Visitor, farmHouse, false);
                                SetFromCommand(Visitor);
                            }
                            else
                            {
                                Log($"{arg2[0]} has already visited the Farm today!", LogLevel.Trace);
                            }
                        }
                        else
                        {
                            Log(TL.Get("error.InvalidValue"), LogLevel.Error);
                        }
                    }
                    catch(Exception)
                    { 
                        //ignore
                    }
                }
                else
                {
                    Log(TL.Get("error.NotInFarmhouse"), LogLevel.Error);
                }
            }
            else
            {
                Log(TL.Get("error.WorldNotReady"), LogLevel.Error);
            }
        }

        public static void Print(string command, string[] arg2)
        {
            if (!Context.IsWorldReady)
            {
                Log(TL.Get("error.WorldNotReady"), LogLevel.Error);
            }
            else
            {
                //Func<string, bool> InArg2 = word => arg2.Any(s => s.ToLower().Equals(word));

                if (arg2 == null || !arg2.Any())
                {
                    Log("Please input an option (Avaiable: animal, blacklist, crop, furniture, info, inlaws, visits).", LogLevel.Warn);
                }
/*
                if ((bool)arg2.Any( s => s.ToLower().Equals("info")))
                {
                    var cc = CurrentCustom?.Count.ToString() ?? "none";
                    var f = Visitor?.Facing.ToString() ?? "none";
                    var pv = Visitor?.CurrentPreVisit?.Count.ToString() ?? "none";
                    var n = Visitor?.Name ?? "none";
                    var am = Visitor?.AnimationMessage ?? "none";

                    Log($"\ncurrentCustom count = {cc}; \nVisitorData: \n   Name = {n},\n   Facing = {f}, \n  AnimationMessage = {am}, \n  Dialogues pre-visit: {pv}", LogLevel.Trace);
                }*/
                else
                {
                    var print = "\n";
                    foreach (var arg in arg2)
                    {
                        var toLower = arg.ToLower();
                        switch (toLower)
                        {
                            case "inlaw":
                            case "inlaws":
                            {
                                print += "\n In-Laws \n--------------------";
                                var result = "\n";
                                foreach (var pair in InLaws)
                                {
                                    var pairvalue = "";
                                    foreach (var name in pair.Value)
                                    {
                                        if (pair.Value[^1].Equals(name))
                                        {
                                            pairvalue += $"{name}.";
                                        }
                                        else
                                        {
                                            pairvalue += $"{name}, ";
                                        }
                                    }

                                    result += $"\n{pair.Key}: {pairvalue}";
                                }

                                if (result.Equals("\n"))
                                {
                                    Log("No in-laws found. (Searched all NPCs with friendship)", LogLevel.Warn);
                                }
                                else
                                {
                                    print += result;
                                }

                                break;
                            }
                            case "animal":
                            case "animals":
                            {
                                print += "\n Animals \n--------------------";
                                foreach(var name in Animals)
                                {
                                    print += $"{name} \n";
                                }

                                break;
                            }
                            case "crop":
                            case "crops":
                            {
                                print += "\n Crops \n--------------------";
                                foreach (var type in Crops)
                                {
                                    print += $"{type.Value} \n";
                                }

                                break;
                            }
                            case "visits":
                            case "v":
                            {
                                print += "\n Visits \n--------------------";
                                foreach (var name in TodaysVisitors)
                                {
                                    print += $"{name} \n";
                                }

                                break;
                            }
                            case "blacklist":
                            case "bl":
                            {
                                print += "\n Blacklist \n--------------------";
                                foreach (var name in BlacklistParsed)
                                {
                                    print += $"{name} \n";
                                }

                                break;
                            }
                            case "furniture":
                            case "f":
                            {
                                print += "\n Furniture \n--------------------";
                                foreach (var name in FurnitureList)
                                {
                                    print += $"{name} \n";
                                }

                                break;
                            }
                        }
                    }
                    Log(print, LogLevel.Info);
                }

                //ModEntry.Log(print,)
            }
        }
    }
}
