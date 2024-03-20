using StardewValley;
using StardewValley.Tools;
using Object = StardewValley.Object;
using static ItemExtensions.Additions.GeneralResource;

namespace ItemExtensions.Patches;

public partial class ObjectPatches
{
    internal static void Postfix_performToolAction(Object __instance, Tool t)
    {
        if (ModEntry.Ores.TryGetValue(__instance.ItemId, out var resource) == false)
            return;

        if (!ToolMatches(t, resource))
            return;

        //set vars
        var location = __instance.Location;
        var tileLocation = __instance.TileLocation;
        var damage = GetDamage(t);
        
        if (damage <= 0)
            return;
        
        //if temp data doesn't exist, it means we also have to set minutes until ready
        try
        {
            _ = __instance.tempData["Health"];
        }
        catch(Exception)
        {
            __instance.tempData ??= new Dictionary<string, object>();
            __instance.tempData.TryAdd("Health", resource.Health);
            __instance.MinutesUntilReady = resource.Health;
        }

        if (damage < resource.MinToolLevel)
        {
            location.playSound("clubhit", tileLocation);
            location.playSound("clank", tileLocation);
            Game1.drawObjectDialogue(string.Format(ModEntry.Help.Translation.Get("CantBreak"), t.DisplayName));
            Game1.player.jitterStrength = 1f;
            return;
        }

        if(!string.IsNullOrWhiteSpace(resource.Sound))
            location.playSound(resource.Sound, tileLocation);
        
        __instance.MinutesUntilReady -= damage + 1;

        if (__instance.MinutesUntilReady <= 0.0)
        {
            CheckDrops(resource, location, tileLocation, t);
            Destroy(__instance);
            
            return;
        }
        
        if(resource.Shake)
            __instance.shakeTimer = 100;
    }
    
    private static void Destroy(Object o, bool onlySetDestroyable = false)
    {
        if (o.lightSource is not null)
        {
            var id = o.lightSource.Identifier;
            o.Location.removeLightSource(id);
        }    
        
        o.CanBeSetDown = true;
        o.CanBeGrabbed = true;
        o.IsSpawnedObject = true; //by default false IIRC
        
        if(onlySetDestroyable)
            return;
        
        //o.performRemoveAction();
        o.Location.removeObject(o.TileLocation,false);
        o = null;
    }

    internal static int GetDamage(Tool tool)
    {
        if (tool is null)
            return -1;
        
        //if melee weapon, do 10% of average DMG
        if (tool is MeleeWeapon w)
        {
            var middle = (w.minDamage.Value + w.maxDamage.Value) / 2;
            return middle / 10;
        }

        return tool.UpgradeLevel;
    }
    
    internal static void Pre_onExplosion(Object __instance, Farmer who)
    {
        if (!ModEntry.Ores.TryGetValue(__instance.ItemId, out var resource))
            return;

        if (resource == null)
            return;

        //var sheetName = ItemRegistry.GetData(data.ItemDropped).TextureName;
        var where = __instance.Location;
        var tile = __instance.TileLocation;
        var num2 = Game1.random.Next(resource.MinDrops, resource.MaxDrops + 1);

        if (string.IsNullOrWhiteSpace(resource.ItemDropped))
            return;
        
        if (Game1.IsMultiplayer)
        {
            Game1.recentMultiplayerRandom = Utility.CreateRandom(tile.X * 1000.0, tile.Y);
            for (var index = 0; index < Game1.random.Next(2, 4); ++index)
                CreateItemDebris(resource.ItemDropped, num2, (int)tile.X, (int)tile.Y, where);
        }
        else
        {
            CreateItemDebris(resource.ItemDropped, num2, (int)tile.X, (int)tile.Y, where);
        }
        
        Destroy(__instance, true);
    }
}