using HarmonyLib;
using ItemExtensions.Models;
using ItemExtensions.Models.Enums;
using ItemExtensions.Models.Internal;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;
using StardewValley.Objects;
using Object = StardewValley.Object;
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator

namespace ItemExtensions.Patches;

public static class InventoryPatches
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);
    
    internal static void Apply(Harmony harmony)
    {
        Log($"Applying Harmony patch \"{nameof(InventoryPatches)}\": postfixing SDV method \"InventoryMenu.rightClick(int, int, Item, bool, bool)\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.rightClick)),
            postfix: new HarmonyMethod(typeof(InventoryPatches), nameof(Post_rightClick))
        );
    }

    /// <summary>
    /// Does item behavior actions.
    /// </summary>
    /// <param name="__instance">This menu instance</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    /// <param name="toAddTo">Item being held.</param>
    /// <param name="__result">The resulting item.</param>
    /// <param name="playSound">Sound to play.</param>
    /// <param name="onlyCheckToolAttachments"></param>
    /// <exception cref="ArgumentOutOfRangeException">If the price modifier isn't valid.</exception>
    /// <see cref="StardewValley.Preconditions.FreeInventorySlots"/>
    internal static void Post_rightClick(InventoryMenu __instance, int x, int y, ref Item toAddTo, ref Item __result, bool playSound = true, bool onlyCheckToolAttachments = false)
    {
        try
        {
            var affectedItem = __instance.getItemAt(x, y);
            var heldItem = toAddTo;

            //if there's no using item OR target item
            if (affectedItem == null || heldItem == null)
            {
                //if mouse grabbed something
                if (__result != null)
                {
#if DEBUG
                Log($"\nPosition: {x}, {y}\nChecking item {__result.DisplayName} ({__result?.QualifiedItemId})\nplaySound: {playSound}, onlyCheckToolAttachments: {onlyCheckToolAttachments}\n");
#endif
                    CallWithoutItem(__instance, ref __result, x, y);
                }
                return;
            }

#if DEBUG
            Log($"\nPosition: {x}, {y}\nHeld item: {heldItem?.QualifiedItemId} ({heldItem?.DisplayName}), Affected item: {affectedItem?.QualifiedItemId}\nplaySound: {playSound}, onlyCheckToolAttachments: {onlyCheckToolAttachments}\n");
#endif

            if (onlyCheckToolAttachments)
                return;

            if (ModEntry.MenuActions == null || ModEntry.MenuActions?.Count == 0)
                return;


            // ReSharper disable once PossibleNullReferenceException
            if (!ModEntry.MenuActions.TryGetValue(heldItem.QualifiedItemId, out var options))
                return;

            Log("Found conversion data for item.");

            foreach (var data in options)
            {
                if (data.TargetId != affectedItem.QualifiedItemId)
                    continue;

                if (!string.IsNullOrWhiteSpace(data.Conditions) && !GameStateQuery.CheckConditions(data.Conditions))
                {
                    Log($"Conditions for {data.TargetId} don't match.");
                    break;
                }

                IWorldChangeData.Solve(data);
                
                //removeamount is PER item to avoid cheating
                if (heldItem.Stack < data.RemoveAmount)
                {
                    Log($"Minimum to remove from {data.TargetId} isn't avaiable.");
                    break;
                }

                //if we can't convert entire stack AND no more spaces, return
                if (heldItem.Stack < data.RemoveAmount * affectedItem.Stack && Game1.player.freeSpotsInInventory() == 0)
                {
                    Game1.showRedMessageUsingLoadString("Strings/StringsFromCSFiles:BlueprintsMenu.cs.10002");
                    Game1.playSound("cancel");

                    Log("No spaces avaiable in inventory. Can't partially convert stack.");
                    break;
                }

                if (data.RandomItemId.Any() || !string.IsNullOrWhiteSpace(data.ReplaceBy))
                {
                    Log($"Replacing {affectedItem.QualifiedItemId} for {data.ReplaceBy}.");
                    var indexOf = __instance.actualInventory.IndexOf(affectedItem);
                    
                    //if there's a random item list, it'll be preferred over normal Id
                    var whichItem = data.RandomItemId.Any() ? Game1.random.ChooseFrom(data.RandomItemId) : data.ReplaceBy;
                    
                    if (data.RemoveAmount <= 0)
                    {
                        var newItem = ItemRegistry.Create(whichItem, data.RetainAmount ? affectedItem.Stack : 1, data.RetainQuality ? affectedItem.Quality : 0);
                        __instance.actualInventory[indexOf] = newItem;
                    }
                    else
                    {
                        /* e.g: stack of 8, remove 3 per created
                     *
                     * (heldItem.Stack - heldItem.Stack % data.RemoveAmount) / data.RemoveAmount
                     * (8 - (8 % 3)) / 3
                     * (8 - 2) / 3
                     * 6 / 3
                     * 2
                     *
                     * if we have 8 and need 3 per conversion, we can make 2 max.
                     * if affected stack is smaller than max, make stack count. else, maxpossible is set
                     */
                        var maxToCreate = (heldItem.Stack - heldItem.Stack % data.RemoveAmount) / data.RemoveAmount;
                        var actualCreateCount = affectedItem.Stack < maxToCreate ? affectedItem.Stack : maxToCreate;
                        var newItem = ItemRegistry.Create(whichItem, actualCreateCount,
                            data.RetainQuality ? affectedItem.Quality : 0);

                        Log($"Created {actualCreateCount} items from stack with {heldItem.Stack} items ({data.RemoveAmount} per change).");

                        //if stack is the same, replace.
                        if (affectedItem.Stack == actualCreateCount)
                            __instance.actualInventory[indexOf] = newItem;
                        else
                        {
                            __instance.actualInventory[indexOf]
                                .ConsumeStack(actualCreateCount); //Stack -= newItem.Stack;
                            Game1.player.addItemByMenuIfNecessary(newItem);
                        }

                        Log($"Removing {data.RemoveAmount} for each converted item.");
                        var consumed = actualCreateCount * data.RemoveAmount;

                        Log($"New stack will be {heldItem.Stack - consumed} ...");

                        //either reduce count OR remove item
                        if (heldItem.Stack - consumed > 0)
                        {
                            heldItem.ConsumeStack(consumed);
                        }
                        else
                        {
                            //__instance.actualInventory.Remove(heldItem); //not part of inventory so this won't work
                            //heldItem.Stack = 0;
                            __result = null;
                        }

                        //this is to avoid copying new values on a preexisting item (that isnt supposed to be changed)
                        Log($"New affected item will be {newItem.QualifiedItemId} ({newItem.DisplayName}).");
                        affectedItem = newItem;
                    }
                }

                TryContextTags(data, affectedItem);
                
                TryModData(data, affectedItem);

                TryQualityChange(data, affectedItem);

                TryPriceChange(data, affectedItem);

                TryTextureChange(data.TextureIndex, affectedItem);

                break;
            }
        }
        catch (Exception e)
        {
            Log($"Error: {e}", LogLevel.Error);
        }
    }

    private static void CallWithoutItem(InventoryMenu menu, ref Item target, int x, int y)
    {
        if (ModEntry.MenuActions == null || ModEntry.MenuActions?.Count == 0)
            return;
        
        // ReSharper disable once PossibleNullReferenceException
        if (!ModEntry.MenuActions.TryGetValue("None", out var options))
            return;
        
        Log("Found conversion data for item. (Mouse/empty action)");

        //search for ID
        foreach (var data in options)
        {
            //if not id, keep searching
            if (data.TargetId != target.QualifiedItemId)
                continue;

            //if conditions don't match
            if (!string.IsNullOrWhiteSpace(data.Conditions) && !GameStateQuery.CheckConditions(data.Conditions))
            {
                Log($"Conditions for {data.TargetId} don't match.");
                break;
            }

            //solve basic data
            IWorldChangeData.Solve(data);
            
            //check if item should be replaced. this ignores the Remove field, because we're holding no item
            if (data.RandomItemId.Any() || !string.IsNullOrWhiteSpace(data.ReplaceBy))
            {
                Log($"Replacing {target.QualifiedItemId} for {data.ReplaceBy}.");
                var indexOf = menu.actualInventory.IndexOf(target);
                    
                //if there's a random item list, it'll be preferred over normal Id
                var whichItem = data.RandomItemId.Any() ? Game1.random.ChooseFrom(data.RandomItemId) : data.ReplaceBy;

                if (whichItem.Equals("Remove", StringComparison.OrdinalIgnoreCase))
                {
                    if(data.RemoveAmount > 0)
                    {}
                    else
                    {
                        menu.actualInventory[indexOf] = null;
                    }
                }
                else
                {
                    var newItem = ItemRegistry.Create(whichItem, data.RetainAmount ? target.Stack : 1,
                        data.RetainQuality ? target.Quality : 0);
                    menu.actualInventory[indexOf] = newItem;
                }
            }

                //check for changes in these fields
            TryContextTags(data, target);
            TryModData(data, target);
            TryQualityChange(data, target);
            TryPriceChange(data, target);
            TryTextureChange(data.TextureIndex, target);

            menu.leftClick(x, y, target, false);
            break;
        }
    }

    private static void TryContextTags(MenuBehavior data, Item affectedItem)
    {
        if (data.AddContextTags.Count > 0)
        {
            var tags = ModEntry.Help.Reflection.GetField<HashSet<string>>(affectedItem, "_contextTags");
            var value = tags.GetValue();

            foreach (var tag in data.AddContextTags)
            {
                Log($"Attempting to add tag {tag}");
                value.Add(tag);
            }

            tags.SetValue(value);
        }

        if (data.RemoveContextTags.Count > 0)
        {
            var tags = ModEntry.Help.Reflection.GetField<HashSet<string>>(affectedItem, "_contextTags");
            var value = tags.GetValue();

            foreach (var tag in data.RemoveContextTags)
            {
                Log($"Attempting to remove tag {tag}");
                value.Remove(tag);
            }

            tags.SetValue(value);
        }
    }
    
    private static void TryModData(MenuBehavior data, Item affectedItem)
    {
        if (data.AddModData is null || data.AddModData.Count <= 0) 
            return;
        
        foreach (var p in data.AddModData)
        {
            Log($"Attempting to add mod data \"{p.Key}\":\"{p.Value}\"");
            if (!affectedItem.modData.TryAdd(p.Key, p.Value))
                affectedItem.modData[p.Key] = p.Value;
        }
    }

    private static void TryQualityChange(MenuBehavior data, Item affectedItem)
    {
        if (string.IsNullOrWhiteSpace(data.QualityChange)) 
            return;
        
        Log($"Changing quality: modifier {data.QualityModifier}, int {data.ActualQuality}");
        switch (data.QualityModifier)
        {
            case Modifier.Set when data.ActualQuality is >= 0 and <= 4:
                affectedItem.Quality = data.ActualQuality;
                if (affectedItem.Quality == 3)
                    affectedItem.Quality = 4;
                break;
            case Modifier.Sum when affectedItem.Quality < 4:
                affectedItem.Quality++;
                if (affectedItem.Quality == 3)
                    affectedItem.Quality = 4;
                break;
            case Modifier.Substract when affectedItem.Quality > 0:
                affectedItem.Quality--;
                if (affectedItem.Quality == 3)
                    affectedItem.Quality = 2;
                break;
            //not considered for quality
            case Modifier.Divide:
            case Modifier.Multiply:
            case Modifier.Percentage:
            default:
                break;
        }
    }

    private static void TryPriceChange(MenuBehavior data, Item affectedItem)
    {
        if (string.IsNullOrWhiteSpace(data.PriceChange) || affectedItem is not (Object or Ring or Boots)) 
            return;
        
        Log($"Changing price: modifier {data.PriceModifier}, int {data.ActualPrice}");
        Item obj = affectedItem switch
        {
            Object o => o,
            Ring r => r,
            Boots b => b,
            _ => throw new ArgumentOutOfRangeException(affectedItem.GetType().ToString())
        };

        var reflectedField = ModEntry.Help.Reflection.GetField<NetInt>(obj, "price");
        var netPrice = reflectedField.GetValue();
        var price = (double)netPrice.Value;

        switch (data.PriceModifier)
        {
            case Modifier.Set when data.ActualPrice >= 0:
                price = data.ActualPrice;
                break;
            case Modifier.Sum:
                price += data.ActualPrice;
                break;
            case Modifier.Substract:
                price -= data.ActualPrice;
                if (price < 0)
                    price = 0;
                break;
            case Modifier.Divide:
                price /= data.ActualPrice;
                break;
            case Modifier.Multiply:
                price *= data.ActualPrice;
                break;
            case Modifier.Percentage:
                price /= data.ActualPrice / 100;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(data.PriceModifier),
                    "Value isn't an allowed one.");
        }

        netPrice.Set((int)price);
        reflectedField.SetValue(netPrice);
    }

    private static void TryTextureChange(int index, Item affectedItem)
    {
        if (index < 0) 
            return;
        
        Log($"Changing texture index to {index}");
        affectedItem.ParentSheetIndex = index;
    }
}