using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace FarmVisitors
{
    /* Extras *
     * extra methods that are important, yet don't fit into specific categories (e.g titles, assetloading, booleans) */
    internal static class Extras
    {
        /* Related to custom visits*/
        internal static void AssetRequest(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("mistyspring.farmhousevisits/Schedules", true))
            {
                e.LoadFrom(
                () => new Dictionary<string, ScheduleData>(),
                AssetLoadPriority.Medium
            );
            }
        }
        internal static bool IsScheduleValid(KeyValuePair<string, ScheduleData> pair)
        {
            var patch = pair.Value;
            try
            {
                if (patch.From is 600 || patch.From is 0)
                {
                    ModEntry.Log(ModEntry.TL.Get("CantBe600"), LogLevel.Error);
                    return false;
                }
                if (patch.To is 2600)
                {
                    ModEntry.Log(ModEntry.TL.Get("CantBe2600"), LogLevel.Error);
                    return false;
                }

                var inSave = ModEntry.NameAndLevel.Keys.Contains(pair.Key);
                if (!inSave)
                {
                    ModEntry.Log(ModEntry.TL.Get("NotInSave"), LogLevel.Error);
                    return false;
                }
                if (patch.From > patch.To && patch.To is not 0)
                {
                    ModEntry.Log(ModEntry.TL.Get("FromHigherThanTo"), LogLevel.Error);
                    return false;
                }
                return true;
            }
            catch(Exception ex)
            {
                ModEntry.Log($"Error when checking schedule: {ex}", LogLevel.Error);
                return false;
            }
        }

        /* in the future, see if using these
         * instead of using .Any()  
         * (as of now, they make the whole mod fail, though).
         
        internal static bool AnyInList(List<string> list)
        {
            try
            {
                return list.Count != 0;
            }
            catch
            {
                return false;
            }
        }
        internal static bool AnySchedule()
        {
            try
            {
                return ModEntry.SchedulesParsed.Count != 0;
            }
            catch
            {
                return false;
            }
        }
        internal static bool AnySchedule(Dictionary<string, ScheduleData> sch)
        {
            try
            {
                return sch.Count != 0;
            }
            catch
            {
                return false;
            }
        }
*/    
    }

    /* For vanilla NPCS *
     * Gets dialogue for when ur in-laws visit */
    internal static class Vanillas
    {
        public static bool InLawOfSpouse(string who)
        {
            var of = who switch
            {
                "Caroline" => "Abigail",
                "Pierre" => "Abigail",

                "Demetrius" => "Maru&Seb",
                "Robin" => "Maru&Seb",

                "Emily" => "Haley", //emily is inlaw of haley
                "Haley" => "Emily", //and haley of emily.

                "Evelyn" => "Alex",
                "George" => "Alex",

                "Pam" => "Penny",

                "Jodi" => "Sam",
                "Kent" => "Sam",

                "Marnie" => "Shane",

                _ => "none",
            };

            foreach(var spouse in ModEntry.MarriedNPCs)
            {
                if (spouse.Equals("Maru") || spouse.Equals("Sebastian"))
                {
                    if (of.Equals("Maru&Seb"))
                    {
                        return true;
                    }

                    return false;
                }

                if (spouse.Equals(of))
                {
                    return true;
                }

                return false;
            }

            //if none applied
            return false;
        }

        public static string GetInLawDialogue(string who)
        {
            var choice = Game1.random.Next(0,11);
            string result;

            if (choice >= 5)
            {
                var ran = Game1.random.Next(1, 4);
                result = who switch
                {
                    //for abigail
                    "Caroline" => ModEntry.TL.Get($"InLaw.Abigail.{ran}"),
                    "Pierre" => ModEntry.TL.Get($"InLaw.Abigail.{ran}"),

                    //for alex
                    "Evelyn" => ModEntry.TL.Get($"InLaw.Alex.{ran}"),
                    "George" => ModEntry.TL.Get($"InLaw.Alex.{ran}"),

                    //for haley and emily
                    "Emily" => ModEntry.TL.Get($"InLaw.Haley.{ran}"),
                    "Haley" => ModEntry.TL.Get($"InLaw.Emily.{ran}"),

                    //for maru
                    "Demetrius" => ModEntry.TL.Get($"InLaw.Maru.{ran}"),

                    //for penny
                    "Pam" => ModEntry.TL.Get($"InLaw.Penny.{ran}"),

                    //for sam
                    "Jodi" => ModEntry.TL.Get($"InLaw.Sam.{ran}"),
                    "Kent" => ModEntry.TL.Get($"InLaw.Sam.{ran}"),

                    //for sebastian
                    "Robin" => ModEntry.TL.Get($"InLaw.Sebastian.{ran}"),

                    //for shane
                    "Marnie" => ModEntry.TL.Get($"InLaw.Shane.{ran}"),

                    _ => null,
                };
            }
            else
            {
                var ran = Game1.random.Next(1, 16);

                string notParsed = ModEntry.TL.Get($"InLaw.Generic.{ran}");
                var spousename = GetSpouseName(who);

                result = string.Format(notParsed, spousename);
            }

            return result;
        }

        private static string GetSpouseName (string who)
        {
            var relatedTo = who switch
            {
                "Caroline" => "Abigail",
                "Pierre" => "Abigail",

                "Demetrius" => "Maru&Seb",
                "Robin" => "Maru&Seb",

                "Emily" => "Haley", //emily is inlaw of haley
                "Haley" => "Emily", //and haley of emily.

                "Evelyn" => "Alex",
                "George" => "Alex",

                "Pam" => "Penny",

                "Jodi" => "Sam",
                "Kent" => "Sam",

                "Marnie" => "Shane",

                _ => "none",
            };

            foreach(var spouse in ModEntry.MarriedNPCs)
            {
                if (spouse.Equals("Maru") && relatedTo.Equals("Maru&Seb"))
                {
                    return "Maru";
                }

                if(spouse.Equals("Sebastian") && relatedTo.Equals("Maru&Seb"))
                {
                    return "Sebastian";
                }

                if(spouse.Equals(relatedTo))
                {
                    return spouse;
                }
            }

            throw new ArgumentException("Character must be related to a vanilla spouse.", nameof(who));
        }

        internal static string AskAboutKids(Farmer player)
        {
            var ran = Game1.random.Next(1, 6);

            var kids = player.getChildren();
            string result;
            
            if(kids.Count is 1)
            {
                var notformatted = ModEntry.TL.Get($"ask.singlechild.{ran}");
                result = String.Format(notformatted, kids[0].Name);
            }
            else
            {
                var notformatted = ModEntry.TL.Get($"ask.multiplechild.{ran}");
                result = String.Format(notformatted, kids[0].Name, kids[1].Name);
            }

            return result;
        }
    }

    /* For modded NPC data *
     * (e.g get characters theyre family of) */
    internal static class Moddeds
    {
        //get all relatives for NPC
        internal static List<string> GetInlawOf(Dictionary<string, string> reference, string who)
        {
            if(!reference.Keys.Contains(who))
            {
                ModEntry.Log("NPC not in dictionary!", LogLevel.Trace);
                return null;
            }

            List<string> result = new();

            //set new char array
            var toSplitWith = new char[1];
            toSplitWith[0] = '/';

            //get relationships field
            var closeOnes = reference[who].GetNthChunk(toSplitWith, 9);
            if (closeOnes.IsEmpty)
                return null;

            //pass to string, split by space
            string closeString = closeOnes.ToString();
            var closeArray = closeString.Split(' ');

            //foreach. if word isnt alias, add
            foreach (var word in closeArray)
            {
                if (!word.Contains('\''))
                {
                    result.Add(word);
                }
            }

            return result;
        }
        //get name of spouse the NPC is related to
        public static string GetRelativeName(string who)
        {
            if (!ModEntry.InLaws.Any())
            {
                return null;
            }

            if (!ModEntry.InLaws.Keys.Contains(who))
            {
                return null;
            }
            if(ModEntry.InLaws[who] is null)
            {
                return null;
            }

            foreach(var spousename in ModEntry.MarriedNPCs)
            {
                if (ModEntry.InLaws[who].Contains(spousename))
                {
                    return spousename;
                }
            }

            return null;
        }
        //get raw dialogue (needs formatting)
        public static string GetDialogueRaw()
        {
            var ran = Game1.random.Next(0, 16);
            string raw = ModEntry.TL.Get($"InLaw.Generic.{ran}");

            //string result = string.Format(Raw, GetSpouseName(who));
            
            return raw;
        }
        public static bool IsVanillaInLaw(string who)
        {
            var result = who switch
            {
                "Caroline" => true,
                "Pierre" => true,
                "Demetrius" => true,
                "Robin" => true,
                "Emily" => true,
                "Haley" => true,
                "Evelyn" => true,
                "George" => true,
                "Pam" => true,
                "Jodi" => true,
                "Kent" => true,
                "Marnie" => true,

                _ => false,
            };

            return result;
        }
    }

    public static class SpanSplit
    {
        //code below taken from atravita !

        /// <summary>
        /// Faster replacement for str.Split()[index];.
        /// </summary>
        /// <param name="str">String to search in.</param>
        /// <param name="deliminators">deliminator to use.</param>
        /// <param name="index">index of the chunk to get.</param>
        /// <returns>a readonlyspan char with the chunk, or an empty readonlyspan for failure.</returns>
        /// <remarks>Inspired by the lovely Wren.</remarks>
        public static ReadOnlySpan<char> GetNthChunk(this string str, char[] deliminators, int index = 0)
        {
            if (index < 0)
            {
                throw new ArgumentException();
            }

            var start = 0;
            var ind = 0;
            while (index-- >= 0)
            {
                ind = str.IndexOfAny(deliminators, start);
                if (ind == -1)
                {
                    // since we've previously decremented
                    // index, check against -1;
                    // this means we're done.
                    if (index == -1)
                    {
                        return str.AsSpan()[start..];
                    }

                    // else, we've run out of entries
                    // and return an empty span to mark as failure.
                    return ReadOnlySpan<char>.Empty;
                }

                if (index > -1)
                {
                    start = ind + 1;
                }
            }
            return str.AsSpan()[start..ind];
        }
    }
}

