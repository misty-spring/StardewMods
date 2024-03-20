using HarmonyLib;
using ItemExtensions.Additions;
using ItemExtensions.Models;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using StardewValley.Triggers;
using Object = StardewValley.Object;

namespace ItemExtensions.Patches;

public class ObjectPatches
{
    private static string StackModData { get; set; } = $"{ModEntry.Id}/MaximumStack";
    private static string ItemHeadModData { get; set; } = $"{ModEntry.Id}/ShowAboveHead";
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);
    
    internal static void Apply(Harmony harmony)
    {
        Log($"Applying Harmony patch \"{nameof(ObjectPatches)}\": prefixing SDV method \"Object.IsHeldOverHead()\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(Object), nameof(Object.IsHeldOverHead)),
            prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(Pre_IsHeldOverHead))
        );
        
        Log($"Applying Harmony patch \"{nameof(ObjectPatches)}\": prefixing SDV method \"Object.maximumStackSize()\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(Object), nameof(Object.maximumStackSize)),
            prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(Pre_maximumStackSize))
        );
        
        Log($"Applying Harmony patch \"{nameof(ObjectPatches)}\": postfixing SDV method \"Object.actionWhenBeingHeld(Farmer)\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(Object), nameof(Object.actionWhenBeingHeld)),
            postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(Post_actionWhenBeingHeld))
        );
        
        Log($"Applying Harmony patch \"{nameof(ObjectPatches)}\": postfixing SDV method \"Object.actionWhenStopBeingHeld(Farmer)\".");
        harmony.Patch(
          original: AccessTools.Method(typeof(Object), nameof(Object.actionWhenStopBeingHeld)),
          postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(Post_actionWhenStopBeingHeld))
        );
        
        Log($"Applying Harmony patch \"{nameof(ObjectPatches)}\": postfixing SDV method \"Object.performRemoveAction()\".");
        harmony.Patch(
          original: AccessTools.Method(typeof(Object), nameof(Object.performRemoveAction)),
          postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(Post_performRemoveAction))
        );
        
        Log($"Applying Harmony patch \"{nameof(ObjectPatches)}\": postfixing SDV method \"Object.dropItem(GameLocation, Vector2, Vector2)\".");
        harmony.Patch(
          original: AccessTools.Method(typeof(Object), nameof(Object.dropItem)),
          postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(Post_dropItem))
        );
        
        Log($"Applying Harmony patch \"{nameof(ObjectPatches)}\": postfixing SDV method \"Object.performToolAction\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(Object), "performToolAction"),
            postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(Postfix_performToolAction))
        );

        Log($"Applying Harmony patch \"{nameof(ObjectPatches)}\": postfixing SDV method \"Object.initializeLightSource\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(Object), "initializeLightSource"),
            postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(Post_initializeLightSource))
        );

        Log($"Applying Harmony patch \"{nameof(ObjectPatches)}\": prefixing SDV method \"Object.IsHeldOverHead()\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(Object), nameof(Object.IsHeldOverHead)),
            prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(Pre_IsHeldOverHead))
        );
        
        Log($"Applying Harmony patch \"{nameof(ObjectPatches)}\": prefixing SDV method \"Object.onExplosion\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(Object), nameof(Object.onExplosion)),
            prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(Pre_onExplosion))
        );
        /*
        Log($"Applying Harmony patch \"{nameof(ObjectPatches)}\": postfixing SDV constructor \"Object(string,int,bool,int,int)\".");
        harmony.Patch(
            original: AccessTools.Constructor(typeof(Object), new[]{typeof(string),typeof(int),typeof(bool),typeof(int),typeof(int)}),
            postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(Post_new))
        )*/
        
        /*Log($"Applying Harmony patch \"{nameof(ObjectPatches)}\": postfixing SDV method \"Object.checkForAction()\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(Object), nameof(Object.checkForAction)),
            postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(Post_checkForAction))
        );*/
    }
    
    #region triggers
    public static void Post_actionWhenBeingHeld(Farmer who)
    {
      if (ModEntry.Holding)
        return;
      
      TriggerActionManager.Raise($"{ModEntry.Id}_OnBeingHeld");
      
      ModEntry.Holding = true;
    }

    public static void Post_actionWhenStopBeingHeld(Farmer who)
    {
      ModEntry.Holding = false;
      TriggerActionManager.Raise($"{ModEntry.Id}_OnStopHolding");
    }
    
    public static void Post_performRemoveAction()
    {
      TriggerActionManager.Raise($"{ModEntry.Id}_OnItemRemoved");
    }
    
    public static void Post_dropItem(Object __instance, GameLocation location, Vector2 origin, Vector2 destination)
    {
      TriggerActionManager.Raise($"{ModEntry.Id}_OnItemDropped");
      
      if (!ModEntry.Data.TryGetValue(__instance.QualifiedItemId, out var mainData))
          return;

      if (mainData.OnDrop == null)
          return;
      
      ActionButton.CheckBehavior(mainData.OnDrop);
    }
    #endregion
    
    #region other changes
    public static void Pre_maximumStackSize(Object __instance, ref int __result)
    {
        if(__instance.modData.TryGetValue(StackModData, out var stack))
            __result = int.Parse(stack);
        
        if (!ModEntry.Data.TryGetValue(__instance.QualifiedItemId, out var data))
            return;

        if (data.MaximumStack == 0)
            return;

        __result = data.MaximumStack;
    }
    
    public static void Pre_IsHeldOverHead(Object __instance, ref bool __result)
    {
        if(__instance.modData.TryGetValue(ItemHeadModData, out var boolean))
            __result = bool.Parse(boolean);
        
        if (!ModEntry.Data.TryGetValue(__instance.QualifiedItemId, out var data))
            return;

        __result = data.HideItem;
    }

    public static void Post_checkForAction(Farmer who, bool justCheckingForActivity = false)
    {
        if (justCheckingForActivity)
            return;
        
        Log("!!!",LogLevel.Debug);
    }
    #endregion
    
    #region resource patches
    private static void Postfix_performToolAction(Object __instance, Tool t)
    {
        if (!ModEntry.Data.TryGetValue(__instance.QualifiedItemId, out var mainData))
            return;

        if (mainData.Resource is null)
            return;

        var data = mainData.Resource;

        if (!ToolMatches(t, mainData.Resource))
            return;

        //set vars
        var location = __instance.Location;
        var tileLocation = __instance.TileLocation;
        var damage = GetDamage(t);
        
        //if temp data doesn't exist, it means we also have to set minutes until ready
        try
        {
            _ = __instance.tempData["Health"];
        }
        catch(Exception)
        {
            _ = __instance.tempData ?? new Dictionary<string, object>();
            __instance.tempData.TryAdd("Health", data.Health);
            __instance.MinutesUntilReady = data.Health;
        }

        if (damage < data.MinToolLevel)
        {
            location.playSound("clubhit", tileLocation);
            location.playSound("clank", tileLocation);
            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceClump.cs.13952"));
            Game1.player.jitterStrength = 1f;
            return;
        }

        if(!string.IsNullOrWhiteSpace(data.Sound))
            location.playSound(data.Sound, tileLocation);
        
        /*var num1 = t.UpgradeLevel + 1; //Math.Max(1f, (t.UpgradeLevel + 1) * 0.75f);
        health -= num1;
        //re-set health info
        __instance.tempData["Health"] = health;*/
        __instance.MinutesUntilReady -= damage + 1;

        if (__instance.MinutesUntilReady <= 0.0)
        {
            //create notes
            if (location.HasUnlockedAreaSecretNotes(t.getLastFarmerToUse()) && Game1.random.NextDouble() < 0.05)
            {
                var unseenSecretNote = location.tryToCreateUnseenSecretNote(t.getLastFarmerToUse());
                if (unseenSecretNote != null)
                    Game1.createItemDebris(unseenSecretNote, tileLocation * 64f, -1, location);
            }
            
            var num2 = Game1.random.Next(data.MinDrops, data.MaxDrops);
            
            if(!string.IsNullOrWhiteSpace(data.Debris))
                CreateRadialDebris(Game1.currentLocation, data.Debris, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(1, 6), false);

            if (!string.IsNullOrWhiteSpace(data.ItemDropped))
            {
                if (Game1.IsMultiplayer)
                {
                    Game1.recentMultiplayerRandom = Utility.CreateRandom(tileLocation.X * 1000.0, tileLocation.Y);
                    for (var index = 0; index < Game1.random.Next(2, 4); ++index)
                        CreateItemDebris(data.ItemDropped, num2, (int)tileLocation.X, (int)tileLocation.Y, location);
                }
                else
                {
                    CreateItemDebris(data.ItemDropped, num2, (int)tileLocation.X, (int)tileLocation.Y, location);
                }
            }

            var chance = Game1.random.NextDouble();
            if (data.ExtraItems != null)
            {
                foreach (var item in data.ExtraItems)
                {
                    //if it has a condition, check first
                    if(!string.IsNullOrWhiteSpace(item.Condition) && !GameStateQuery.CheckConditions(item.Condition))
                        continue;
                    
                    if (chance <= item.Chance)
                        CreateItemDebris(item.Id, item.Count, (int)tileLocation.X, (int)tileLocation.Y, location);
                }
            }

            if(!string.IsNullOrWhiteSpace(data.BreakingSound))
                location.playSound(data.BreakingSound, tileLocation);
            
            //var obj = location.getObjectAtTile((int)tileLocation.X, (int)tileLocation.Y);
            if (__instance.lightSource is not null)
            {
                var id = __instance.lightSource.Identifier;
                location.removeLightSource(id);
            }

            Destroy(__instance);
            return;
        }
        __instance.shakeTimer = 100;
    }

    private static void Destroy(Object o, bool onlySetDestroyable = false)
    {
        o.CanBeSetDown = true;
        o.CanBeGrabbed = true;
        o.IsSpawnedObject = true; //by default false IIRC
        
        if(onlySetDestroyable)
            return;
        
        //o.performRemoveAction();
        o.Location.removeObject(o.TileLocation,false);
        o = null;
    }

    private static int GetDamage(Tool tool)
    {
        //if melee weapon, do 10% of average DMG
        if (tool is MeleeWeapon w)
        {
            var middle = (w.minDamage.Value + w.maxDamage.Value) / 2;
            return middle / 10;
        }

        return tool.UpgradeLevel;
    }

    private static bool ToolMatches(Tool tool, ResourceData data)
    {
        var required = data.Tool.ToLower();
        
        if (tool is MeleeWeapon w)
        {
            //if the user set a number, we assume it's a custom tool
            if (int.TryParse(data.Tool, out var number))
                return w.type.Value == number;
            
            //any wpn
            if (required is "meleeweapon" or "weapon")
                return true;

            var weaponType = required switch
            {
                "stabbing sword" or "stabbing" or "stab" => 0,
                "dagger" => 1,
                "club" or "hammer" => 2,
                "slashing sword" or "slashing" or "slash" => 3,
                "sword" => 30,
                _ => 0
            };

            if (weaponType == 30)
            {
                return w.type.Value is 0 or 3;
            }

            return weaponType == w.type.Value;
        }
        
        var className = tool.GetToolData().ClassName.ToLower();
        #if DEBUG
        Log($"Tool: {className}");
        #endif
        return className == required;
    }

    private static void CreateRadialDebris(GameLocation location, string debrisType, int xTile, int yTile, int numberOfChunks, bool resource, bool item = false)
    {
        /*
        #if DEBUG
        Log($"Debris: {debrisType}");
        #endif*/
        
        var vector = new Vector2(xTile * 64 + 64, yTile * 64 + 64);
        
        if (item)
        {
            while (numberOfChunks > 0)
            {
                var vector2 = Game1.random.Next(4) switch
                {
                    0 => new Vector2(-64f, 0f),
                    1 => new Vector2(64f, 0f),
                    2 => new Vector2(0f, 64f),
                    _ => new Vector2(0f, -64f),
                };
                var item2 = ItemRegistry.Create(debrisType);
                location.debris.Add(new Debris(item2, vector, vector + vector2));
                numberOfChunks--;
            }
        }
        
        if (resource)
        {
            location.debris.Add(new Debris(debrisType, numberOfChunks / 4, vector, vector + new Vector2(-64f, 0f)));
            numberOfChunks++;
            location.debris.Add(new Debris(debrisType, numberOfChunks / 4, vector, vector + new Vector2(64f, 0f)));
            numberOfChunks++;
            location.debris.Add(new Debris(debrisType, numberOfChunks / 4, vector, vector + new Vector2(0f, -64f)));
            numberOfChunks++;
            location.debris.Add(new Debris(debrisType, numberOfChunks / 4, vector, vector + new Vector2(0f, 64f)));
        }
        else
        {
            var spawned = true;
            
            switch (debrisType.ToLower())
            {
                case "coins":
                    Game1.createRadialDebris(Game1.currentLocation, 8, xTile, yTile, Game1.random.Next(2, 6), false);
                    break;
                case "stone":
                    Game1.createRadialDebris(Game1.currentLocation, 14, xTile, yTile, Game1.random.Next(2, 6), false);
                    break;
                case "bigstone":
                    Game1.createRadialDebris(Game1.currentLocation, 32, xTile, yTile, Game1.random.Next(3, 10), false);
                    break;
                case "wood":
                    Game1.createRadialDebris(Game1.currentLocation, 12, xTile, yTile, Game1.random.Next(2, 6), false);
                    break;
                case "bigwood":
                    Game1.createRadialDebris(Game1.currentLocation, 34, xTile, yTile, Game1.random.Next(3, 10), false);
                    break;
                default:
                    spawned = false;
                    break;
            }

            if (spawned)
                return;
            
            while (numberOfChunks > 0)
            {
                var vector2 = Game1.random.Next(4) switch
                {
                    0 => new Vector2(-64f, 0f),
                    1 => new Vector2(64f, 0f),
                    2 => new Vector2(0f, 64f),
                    _ => new Vector2(0f, -64f),
                };
                
                //create debris with item data
                var item2 = ItemRegistry.GetData(debrisType);
                var sourceRect = item2.GetSourceRect();
                var debris = new Debris(spriteSheet: item2.TextureName, sourceRect, 1, vector + vector2);
                
                //fix item shown
                debris.Chunks[0].xSpriteSheet.Value = sourceRect.X;
                debris.Chunks[0].ySpriteSheet.Value = sourceRect.Y;
                debris.Chunks[0].scale = 4f;
                
                //debris.debrisType.Set(Debris.DebrisType.CHUNKS);
                
                location.debris.Add(debris);
                numberOfChunks--;
            }
        }
    }

    private static void CreateItemDebris(string itemId, int howMuchDebris, int xTile, int yTile, GameLocation where) => CreateRadialDebris(where, itemId, xTile, yTile, howMuchDebris, true);

    public static void Pre_onExplosion(Object __instance, Farmer who)
    {
        if (!ModEntry.Data.TryGetValue(__instance.QualifiedItemId, out var mainData))
            return;

        if (mainData.Resource == null)
            return;

        var data = mainData.Resource;

        //var sheetName = ItemRegistry.GetData(data.ItemDropped).TextureName;
        var where = __instance.Location;
        var tile = __instance.TileLocation;
        var num2 = Game1.random.Next(data.MinDrops, data.MaxDrops + 1);

        if (string.IsNullOrWhiteSpace(data.ItemDropped))
            return;
        
        if (Game1.IsMultiplayer)
        {
            Game1.recentMultiplayerRandom = Utility.CreateRandom(tile.X * 1000.0, tile.Y);
            for (var index = 0; index < Game1.random.Next(2, 4); ++index)
                CreateItemDebris(data.ItemDropped, num2, (int)tile.X, (int)tile.Y, where);
        }
        else
        {
            CreateItemDebris(data.ItemDropped, num2, (int)tile.X, (int)tile.Y, where);
        }
        
        Destroy(__instance, true);
    }
    #endregion

    #region light
    public static void Post_initializeLightSource(Object __instance, Vector2 tileLocation, bool mineShaft = false)
    {
        var id = __instance.QualifiedItemId;
        if(id == null)
            return;

        if (!ModEntry.Data.TryGetValue(__instance.QualifiedItemId, out var mainData))
            return;

        if (mainData.Light == null)
            return;

        var data = mainData.Light;

        var color = data.GetColor();
        
        var rad = data.Size;
        var position = new Vector2(tileLocation.X * 64f + 16f, tileLocation.Y * 64f + 16f);

        //var identifier = (int)(tileLocation.X * 2000f + tileLocation.Y);
        __instance.lightSource = new LightSource(4, position, rad, color);
    }
    #endregion
    
    #region constructor

    internal static void Post_new(ref Object __instance, string itemId, int initialStack, bool isRecipe = false,
        int price = -1, int quality = 0)
    {
        if (!ModEntry.Data.TryGetValue(__instance.QualifiedItemId, out var mainData))
            return;
        
        if(mainData.Resource == null || mainData.Resource == new ResourceData())
            return;

        Log("Created item has resource data. Adding...");

        __instance.MinutesUntilReady = mainData.Resource.Health; //mainData.Resource.MinToolLevel + 1;
        
        if (__instance.tempData is null)
        {
            __instance.tempData =  new Dictionary<string, object>
            {
                { "Health", mainData.Resource.Health }
            };
        }
        else
        {
            __instance.tempData.Add("Health", mainData.Resource.Health);
        }

        //__instance.Fragility = Object.fragility_Delicate;
        __instance.modData["Esca.FarmTypeManager/CanBePickedUp"] = "false";
        __instance.IsSpawnedObject = false;
        
        __instance.CanBeGrabbed = false;
        __instance.CanBeSetDown = true;
    }
    #endregion
}