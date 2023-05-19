using DynamicDialogues.Framework;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Linq;
using StardewModdingAPI;

namespace DynamicDialogues
{
    internal class ModPatches
    {
        //for custom greetings
        public static bool SayHiTo_Prefix(ref NPC __instance, Character c)
        {
            var instancename = __instance.Name;
            var cname = (c as NPC).Name;
            var mainAndRef = (instancename, cname);
            var refAndMain = (cname, instancename);

            try
            {
                //if a (thisnpc, othernpc) key exists
                if (ModEntry.Greetings.TryGetValue(mainAndRef, out var greeting))
                {
                    //log, then use previous key to find value
                    ModEntry.Mon.Log($"Found greeting patch for {__instance.Name}");
                    __instance.showTextAboveHead(greeting);

                    //if that other npc has a key for thisnpc
                    if (ModEntry.Greetings.TryGetValue(refAndMain, out var greeting1))
                    {
                        //same as before
                        ModEntry.Mon.Log($"Found greeting patch for {(c as NPC)?.Name}");
                        (c as NPC)?.showTextAboveHead(greeting1);
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                ModEntry.Mon.Log($"Error while applying patch: {ex}", StardewModdingAPI.LogLevel.Error);
            }

            return true;
        }

        //for AddScene
        internal static bool PrefixTryGetCommandH(Event __instance, GameLocation location, GameTime time, string[] args) =>
            PrefixTryGetCommand(__instance, location, time, args);
        
        internal static bool PrefixTryGetCommand(Event __instance, GameLocation location, GameTime time, string[] split)
        {
            if (split.Length < 2) //scene has optional parameters, so its 2 OR more
            {
                return true;
            }
            else if (split[0].Equals(ModEntry.AddScene, StringComparison.Ordinal))
            {
                EventScene.Add(__instance, location, time, split);
                return false;
            }
            else if (split[0].Equals(ModEntry.RemoveScene, StringComparison.Ordinal))
            {
                EventScene.Remove(__instance, location, time, split);
                return false;
            }
            else if(split[0].Equals(ModEntry.PlayerFind, StringComparison.Ordinal))
            {
                Finder.ObjectHunt(__instance, location, time, split);
                return false;
            }
            return true;
        }
        
        //for custom gifting dialogue
        internal static bool TryToReceiveItem(ref NPC __instance, Farmer who)
        {
            var item = who.ActiveObject;

            var hasCustomDialogue = HasCustomDialogue(__instance,item.parentSheetIndex);

            if (!hasCustomDialogue)
            {
                return true;
            }
            else
            {
                __instance.CurrentDialogue.Push(
                    new Dialogue(
                        Game1.content.LoadString(
                            $"Characters\\Dialogue\\{__instance.Name}:Gift.{item.parentSheetIndex}"),
                        __instance)
                    );
                
                return false;
            }
        }

        private static bool HasCustomDialogue(Character __instance, int parentSheetIndex)
        {
            if (!ModEntry.HasCustomGifting.ContainsKey(__instance.Name))
                return false;

            var index = $"{parentSheetIndex}";
            foreach (var key in ModEntry.HasCustomGifting[__instance.Name])
                if (Equals(key, index))
                    return true;
            return false;
        }
        
        //for changing NPCs mid dialogue
        internal static bool PrefixCurrentDialogueForDisplay(Dialogue __instance)
        {
            try
            {
                var str1 = Utility.ParseGiftReveals(__instance.dialogues[__instance.currentDialogueIndex]);
                if (str1.StartsWith("$npc"))
                {
                    var nameof = str1.Replace("$npc", "");
                    var who = Game1.getCharacterFromName(nameof);
                    __instance.speaker = who;
    
                    //remove so it doesnt show a text thing
                    __instance.dialogues.Remove(str1);
                }
            
                return true;
            }
            catch (Exception e)
            {
                ModEntry.Mon.Log("Error: "+e,LogLevel.Error);
                throw;
            }
        }
    }
}