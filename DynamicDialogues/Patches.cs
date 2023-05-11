using DynamicDialogues.Framework;
using Microsoft.Xna.Framework;
using StardewValley;
using System;


namespace DynamicDialogues
{
    internal class ModPatches
    {
        //for custom greetings
        public static bool SayHiTo_Prefix(ref NPC instance, Character c)
        {
            var instancename = instance.Name;
            var cname = (c as NPC).Name;
            var mainAndRef = (instancename, cname);
            var refAndMain = (cname, instancename);

            try
            {
                //if a (thisnpc, othernpc) key exists
                if (ModEntry.Greetings.TryGetValue(mainAndRef, out var greeting))
                {
                    //log, then use previous key to find value
                    ModEntry.Mon.Log($"Found greeting patch for {instance.Name}");
                    instance.showTextAboveHead(greeting);

                    //if that other npc has a key for thisnpc
                    if (ModEntry.Greetings.TryGetValue(refAndMain, out var greeting1))
                    {
                        //same as before
                        ModEntry.Mon.Log($"Found greeting patch for {(c as NPC).Name}");
                        (c as NPC).showTextAboveHead(greeting1, Color.Black, 2, 3000, 1000 + Game1.random.Next(500));
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
        internal static bool PrefixTryGetCommand(Event instance, GameLocation location, GameTime time, string[] split)
        {
            if (split.Length < 2) //scene has optional parameters, so its 2 OR more
            {
                return true;
            }
            else if (split[0].Equals(ModEntry.AddScene, StringComparison.Ordinal))
            {
                EventScene.Add(instance, location, time, split);
                return false;
            }
            else if (split[0].Equals(ModEntry.RemoveScene, StringComparison.Ordinal))
            {
                EventScene.Remove(instance, location, time, split);
                return false;
            }
            else if(split[0].Equals(ModEntry.PlayerFind, StringComparison.Ordinal))
            {
                Finder.ObjectHunt(instance, location, time, split);
                return false;
            }
            return true;
        }
        
        internal static bool TryToReceiveItem(ref NPC instance, Farmer who)
        {
            var item = who.ActiveObject;

            bool hasCustomDialogue = HasCustomDialogue(instance,item.QualifiedItemId);

            if (!hasCustomDialogue)
            {
                return true;
            }
            else
            {
                instance.idk;
                return false;
            }
        }

        private static bool HasCustomDialogue(Character instance, string itemId)
        {
            return ModEntry.CustomDialogues?[instance.Name].Contains(itemId);
        }
    }
}