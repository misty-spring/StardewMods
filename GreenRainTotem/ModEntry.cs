using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.LocationContexts;
using StardewValley.GameData.Objects;
using Object = StardewValley.Object;

namespace GreenRainTotem;

public class ModEntry : Mod
{
    public static int NextGreenRainDay { get; internal set; }
    public const string ModId = "mistyspring.GreenRainTotem";

    public override void Entry(IModHelper helper)
    {
        helper.Events.Content.AssetRequested += OnAssetRequested;
        helper.Events.GameLoop.ReturnedToTitle += OnTitleReturn;

        var harmony = new Harmony(ModManifest.UniqueID);
        Monitor.Log($"Applying Harmony patch \"{nameof(ModEntry)}\": postfixing SDV method \"Utility.getSpouseBedSpot\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(Utility), nameof(Utility.isGreenRainDay), new[] { typeof(int), typeof(Season)}),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(Post_isGreenRainDay))
        );
        Monitor.Log($"Applying Harmony patch \"{nameof(ModEntry)}\": postfixing SDV method \"Utility.getSpouseBedSpot\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(Object), nameof(Object.performUseAction)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(Post_performUseAction))
        );
    }

    private void OnTitleReturn(object? sender, ReturnedToTitleEventArgs e)
    {
        NextGreenRainDay = 0;
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.Name.Equals("Data/Objects"))
        {
            e.Edit(asset =>
            {
                //this fixes special order text to be translated in all languages. hopefully
                var data = asset.AsDictionary<string, ObjectData>().Data;
                data.Add(ModId, new ObjectData()
                {
                    Name = "Green Rain Totem",
                    DisplayName = "[LocalizedText Strings\\Objects:GreenRainTotem_Name]",
                    Description = "[LocalizedText Strings\\Objects:RainTotem_Description]",
                    Type = "Crafting",
                    Category = 0,
                    Price = 20,
                    Texture = "Mods/mistyspring.GreenRainTotem/Objects",
                    SpriteIndex = 0,
                    ColorOverlayFromNextIndex = false,
                    Edibility = -300,
                    IsDrink = false,
                    Buffs = null,
                    GeodeDropsDefaultItems = false,
                    GeodeDrops = null,
                    ArtifactSpotChances = null,
                    CanBeGivenAsGift = true,
                    CanBeTrashed = true,
                    ExcludeFromFishingCollection = false,
                    ExcludeFromShippingCollection = false,
                    ExcludeFromRandomSale = true,
                    ContextTags = new() {
                      "color_green",
                      "not_placeable",
                      "totem_item"
                    },
                    CustomFields = null

                });
            });
        }

        if (e.NameWithoutLocale.Name.Equals("Strings/Objects"))
        { 
            e.Edit(asset => {
                var data = asset.AsDictionary<string, string>().Data;
                data.Add("GreenRainTotem_Name", Helper.Translation.Get("GreenRainTotem"));
            }); 
        }

        if (e.NameWithoutLocale.Name.Equals("Data/CraftingRecipes"))
        {
            e.Edit(asset => {
                var data = asset.AsDictionary<string, string>().Data;
                data.Add("GreenRainTotem", $"709 1 432 1 Moss 5/Field/{ModId}/false/s Foraging 9/");
            });
        }

        if (e.NameWithoutLocale.Name.Equals("Mods/mistyspring.GreenRainTotem/Objects"))
            e.LoadFromModFile<Texture2D>("assets/totem.png", AssetLoadPriority.Medium);
    }

    /// <summary>
    /// Checks if the current day matches the green rain int.
    /// </summary>
    /// <param name="day"></param>
    /// <param name="season"></param>
    /// <param name="__result"></param>
    /// see <see cref="Game1"/> for more details.
    public static void Post_isGreenRainDay(int day, Season season, ref bool __result)
    {
        if (__result == true)
            return;

        if (season == Season.Summer)
        {
            __result = day == NextGreenRainDay;
        }
    }

    public static void Post_performUseAction(Object __instance, GameLocation location, ref bool __result)
    {
        if (__result == true || __instance.ItemId.Equals(ModId) == false)
            return;

        if (!Game1.player.canMove || __instance.isTemporarilyInvisible)
        {
            return;
        }

        bool flag = !Game1.eventUp && !Game1.isFestival() && !Game1.fadeToBlack && !Game1.player.swimming.Value && !Game1.player.bathingClothes.Value && !Game1.player.onBridge.Value;
        if (flag)
        {
            if (Game1.player.currentLocation.InValleyContext() == false)
            {
                Game1.showRedMessageUsingLoadString("Strings\\UI:Item_CantBeUsedHere");
                return;
            }
            else if (Game1.season is not Season.Summer)
            {
                Game1.showRedMessageUsingLoadString("Strings/StringsFromCSFiles:HoeDirt.cs.13924");
                return;
            }
            else
            {
                GreenRainTotem(Game1.player);
                __result = true;
            }
        }
    }

    /// see <see cref="Object"/> for how to make the totem work.
    private static void GreenRainTotem(Farmer who)
    {
        GameLocation currentLocation = who.currentLocation;

        /*//if NOT in valley
        if (currentLocation.InValleyContext() == false)
        {
            Game1.showRedMessageUsingLoadString("Strings\\UI:Item_CantBeUsedHere");
            return;
        }*/

        string text = currentLocation.GetLocationContextId();
        LocationContextData locationContext = currentLocation.GetLocationContext();
        if (!locationContext.AllowRainTotem)
        {
            Game1.showRedMessageUsingLoadString("Strings\\UI:Item_CantBeUsedHere");
            return;
        }

        if (locationContext.RainTotemAffectsContext != null)
        {
            text = locationContext.RainTotemAffectsContext;
        }

        bool flag = false;
        if (text == "Default")
        {
            if (!Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.season))
            {
                Game1.weatherForTomorrow = Game1.weather_green_rain;
                NextGreenRainDay = Game1.dayOfMonth + 1;
                flag = true;
            }
        }
        else
        {
            Game1.weatherForTomorrow = Game1.weather_green_rain;
            NextGreenRainDay = Game1.dayOfMonth + 1;
            flag = true;
        }

        if (flag)
        {
            Game1.pauseThenMessage(2000, Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12822"));
        }

        Game1.screenGlow = false;
        currentLocation.playSound("thunder");
        who.canMove = false;
        Game1.screenGlowOnce(Color.SlateBlue, hold: false);
        Game1.player.faceDirection(2);
        Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[1]
        {
            new FarmerSprite.AnimationFrame(57, 2000, secondaryArm: false, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true)
        });
        for (int i = 0; i < 6; i++)
        {
            Game1.Multiplayer.broadcastSprites(currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 1045, 52, 33), 9999f, 1, 999, who.Position + new Vector2(0f, -128f), flicker: false, flipped: false, 1f, 0.01f, Color.White * 0.8f, 2f, 0.01f, 0f, 0f)
            {
                motion = new Vector2((float)Game1.random.Next(-10, 11) / 10f, -2f),
                delayBeforeAnimationStart = i * 200
            });
            Game1.Multiplayer.broadcastSprites(currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 1045, 52, 33), 9999f, 1, 999, who.Position + new Vector2(0f, -128f), flicker: false, flipped: false, 1f, 0.01f, Color.White * 0.8f, 1f, 0.01f, 0f, 0f)
            {
                motion = new Vector2((float)Game1.random.Next(-30, -10) / 10f, -1f),
                delayBeforeAnimationStart = 100 + i * 200
            });
            Game1.Multiplayer.broadcastSprites(currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 1045, 52, 33), 9999f, 1, 999, who.Position + new Vector2(0f, -128f), flicker: false, flipped: false, 1f, 0.01f, Color.White * 0.8f, 1f, 0.01f, 0f, 0f)
            {
                motion = new Vector2((float)Game1.random.Next(10, 30) / 10f, -1f),
                delayBeforeAnimationStart = 200 + i * 200
            });
        }

        TemporaryAnimatedSprite temporaryAnimatedSprite = new TemporaryAnimatedSprite(0, 9999f, 1, 999, Game1.player.Position + new Vector2(0f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
        {
            motion = new Vector2(0f, -7f),
            acceleration = new Vector2(0f, 0.1f),
            scaleChange = 0.015f,
            alpha = 1f,
            alphaFade = 0.0075f,
            shakeIntensity = 1f,
            initialPosition = Game1.player.Position + new Vector2(0f, -96f),
            xPeriodic = true,
            xPeriodicLoopTime = 1000f,
            xPeriodicRange = 4f,
            layerDepth = 1f
        };
        temporaryAnimatedSprite.CopyAppearanceFromItemId("(O)" + ModId);
        Game1.Multiplayer.broadcastSprites(currentLocation, temporaryAnimatedSprite);
        DelayedAction.playSoundAfterDelay("rainsound", 2000);
    }

}
