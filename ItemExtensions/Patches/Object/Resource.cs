using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using Object = StardewValley.Object;
using static ItemExtensions.Additions.GeneralResource;
using StardewModdingAPI.Utilities;

namespace ItemExtensions.Patches;

public partial class ObjectPatches
{
    //used so weapon msg isn't repeated 5 times
    private static bool CanShowMessage { get; set; } = true;
    private static void Reset() => CanShowMessage = true;
    private static PerScreen<bool> InPickaxeDoFunction = new() { Value = false };

    internal static void Postfix_performToolAction(Object __instance, Tool t)
    {
        try
        {
            if (ModEntry.Ores.TryGetValue(__instance.ItemId, out var resource) == false)
            {
#if DEBUG
                Log("Not a node.");
#endif
                return;
            }

            if (__instance.MinutesUntilReady <= 0.0)
            {
                if (resource.Tool.Equals("vanilla"))
                {
                    CheckDrops(resource, __instance.Location, __instance.TileLocation, t);
                    return;
                }
            }

            if (ToolMatches(t, resource) == false)
            {
                if (ShouldShowWrongTool(t,resource) && CanShowMessage)
                {
                    var msg = Game1.content.LoadString("Strings/Locations:IslandNorth_CaveTool_3");
                    Game1.drawObjectDialogue(msg);
                    CanShowMessage = false;
                    Game1.delayedActions.Add(new DelayedAction(500, Reset));
                }

                return;
            }

            //set vars
            var location = __instance.Location;
            var tileLocation = __instance.TileLocation;
            var damage = GetDamage(t, -1, resource.EasyCalc);
#if DEBUG
            Log($"Damage: {damage}");
#endif

            if (damage <= 0)
                return;

            //if temp data doesn't exist, it means we also have to set minutes until ready
            __instance.tempData ??= new Dictionary<string, object>();
            if (__instance.tempData.TryGetValue("Health", out _) == false)
            {
                __instance.tempData.TryAdd("Health", resource.Health);
                __instance.MinutesUntilReady = resource.Health;
            }

            if (t is not null or MeleeWeapon && t.UpgradeLevel < resource.MinToolLevel)
            {
                foreach (var sound in resource.FailSounds)
                {
                    location.playSound(sound, tileLocation);
                }
                Game1.drawObjectDialogue(string.Format(ModEntry.Help.Translation.Get("CantBreak"), t.DisplayName));
                Game1.player.jitterStrength = 1f;
                return;
            }

            if (!string.IsNullOrWhiteSpace(resource.Sound))
                location.playSound(resource.Sound, tileLocation);

            __instance.MinutesUntilReady -= damage + 1;

            if (__instance.MinutesUntilReady <= 0.0)
            {
                //shown regardless of animation
                //resource fade
                var tilePositionToTry = __instance.TileLocation;
                //dust
                var dust = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 1600, 64, 128), tilePositionToTry * 64f+ new Vector2(0f, -64f), __instance.Flipped, 0.01f, Color.White)
                {
                    layerDepth = 0.1792f,
                    totalNumberOfLoops = 1,
                    currentNumberOfLoops = 1,
                    interval = 80f,
                    animationLength = 8
                };
                Game1.Multiplayer.broadcastSprites(Game1.player.currentLocation, dust);

                //do drops & destroy
                CheckDrops(resource, location, tileLocation, t);
                Destroy(__instance);

                return;
            }

            if (resource.Shake)
                __instance.shakeTimer = 100;
        }
        catch (Exception e)
        {
            Log($"Error: {e}", LogLevel.Error);
        }
    }

    internal static void Destroy(Object o, bool onlySetDestroyable = false)
    {
        try
        {
            if (o.lightSource is not null)
            {
                var id = o.lightSource.Id;
                o.Location.removeLightSource(id);
            }

            o.CanBeSetDown = true;
            o.CanBeGrabbed = true;
            o.IsSpawnedObject = true; //by default false IIRC

            if(onlySetDestroyable)
                return;
        
            o.Location.removeObject(o.TileLocation,false);
        }
        catch (Exception e)
        {
            Log($"Error: {e}", LogLevel.Error);
        }
    }

    internal static bool CheckForImmuneNodes(Object __instance, Farmer who, ref bool __result)
    {
        try
        {
            if (!ModEntry.Ores.TryGetValue(__instance.ItemId, out var resource))
                return true;

            if (resource == null)
                return true;

            if (!resource.ImmuneToBombs)
                return true;

            __result = false;
            return false;
        }
        catch
        {
            return true;
        }
    }

    internal static void Pre_onExplosion(Object __instance, Farmer who)
    {
        try
        {
            if (!ModEntry.Ores.TryGetValue(__instance.ItemId, out var resource))
                return;

            if (resource == null)
                return;

            if (resource.ImmuneToBombs)
                return;

            var where = __instance.Location;
            var tile = __instance.TileLocation;

            CheckDrops(resource, where, tile, null, true);

            Destroy(__instance, true);
        }
        catch (Exception e)
        {
            Log($"Error: {e}", LogLevel.Error);
        }
    }

    private static void Post_IsBreakableStone(Object __instance, ref bool __result)
    {
        if (!InPickaxeDoFunction.Value && ModEntry.Ores.ContainsKey(__instance.ItemId))
            __result = true;
    }

    /// <summary>
    // This pair of patches help <see cref="Post_IsBreakableStone"/> detect being Pickaxe.DoFunction.
    // This allow pickaxe action to reach Object.performToolAction rather than being trapped into the vanilla one.
    //  </summary>
    private static void Pre_Pickaxe_DoFunction() => InPickaxeDoFunction.Value = true;
    private static void Fin_Pickaxe_DoFunction() => InPickaxeDoFunction.Value = false;
}
