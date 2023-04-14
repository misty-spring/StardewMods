using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using GenericModConfigMenu;
using System;

namespace AudioDescription
{
    public class ModEntry : Mod
    {
        #region config
        internal static List<string> AllowedCues { get; set; } = new();
        internal static ModConfig Config;
        internal static int Cooldown;
        #endregion

        #region set variables
        /*//depend on lang
        private static string NoMusic;
        private static string Playing;

        //depend on music
        internal static string _lastTrack;
        internal static string Musictext;

        //depend on toolbar position
        internal static bool IsToolbarAbove = false;
        internal static int Bottom; //draw-able bottom of screen for track purposes
        internal static Rectangle ToolbarRect { get; private set; } = new();
        internal static Rectangle TrackZone { get; private set; } = new();
        internal static Rectangle AltTrackZone { get; private set; } = new();
        internal static Vector2 SafePositionTop { get; set; } = new(99f,99f); //can't be null so we use 99f as placeholder.
        internal static Vector2 SafePositionBottom { get; set; } = new(99f,99f); //can't be null so we use 99f as placeholder.*/
        #endregion

        #region utilities
        internal static IModHelper Help { get; private set; }
        internal static IMonitor Mon { get; private set; }
        //internal static Dictionary<string, string> Sounds { get; set; } = new();
        public static Texture2D MuteIcon { get; internal set; }
        #endregion

        public override void Entry(IModHelper helper)
        {
            //helper.Events.Content.LocaleChanged += LocaleChanged;
            helper.Events.GameLoop.GameLaunched += OnGameStart;
            helper.Events.GameLoop.SaveLoaded += ConfigInfo.SaveLoaded;
            helper.Events.GameLoop.OneSecondUpdateTicked += SecondPassed;
            //helper.Events.Display.RenderedHud += RenderedHUD;

            ModEntry.Config = this.Helper.ReadConfig<ModConfig>();
            Help = this.Helper;
            Mon = this.Monitor;

            var harmony = new Harmony(this.ModManifest.UniqueID);

            this.Monitor.Log($"Applying Harmony patch \"{nameof(SoundPatches)}\": postfixing SDV method \"Game1.playSound\".");
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Game1), nameof(StardewValley.Game1.playSound)),
                postfix: new HarmonyMethod(typeof(SoundPatches), nameof(SoundPatches.PostFix_playSound))
                );

            this.Monitor.Log($"Applying Harmony patch \"{nameof(SoundPatches)}\": postfixing SDV method \"Game1.playSoundPitched\".");
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Game1), nameof(StardewValley.Game1.playSoundPitched)),
                postfix: new HarmonyMethod(typeof(SoundPatches), nameof(SoundPatches.PostFix_playSoundPitched))
                );

            this.Monitor.Log($"Applying Harmony patch \"{nameof(SoundPatches)}\": prefixing SDV method \"HUDMessage.draw\".");
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.HUDMessage), nameof(StardewValley.HUDMessage.draw)),
                prefix: new HarmonyMethod(typeof(SoundPatches), nameof(SoundPatches.PrefixHUDdraw))
                );
        }

        private void SecondPassed(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (Cooldown != 0)
                Cooldown--;
        }

        /*private void LocaleChanged(object sender, LocaleChangedEventArgs e)
        {
            NoMusic = Game1.content.LoadString("Strings\\UI:Character_none") ?? "None";
            Playing = Game1.content.LoadString("Strings\\UI:Emote_Music") ?? "Music";
            Playing += ":";
        }*/

        private void OnGameStart(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(Config)
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.Cooldown.name"),
                tooltip: () => this.Helper.Translation.Get("config.Cooldown.description"),
                getValue: () => Config.CoolDown,
                setValue: value => Config.CoolDown = value
                );
            /*configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.Tracks.name"),
                tooltip: () => this.Helper.Translation.Get("config.Tracks.description"),
                getValue: () => Config.TrackCue,
                setValue: value => Config.TrackCue = value
            );*/
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.Environment.name"),
                tooltip: () => this.Helper.Translation.Get("config.Environment.description"),
                getValue: () => Config.Environment,
                setValue: value => Config.Environment = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.NPCs.name"),
                tooltip: () => this.Helper.Translation.Get("config.NPCs.description"),
                getValue: () => Config.NPCs,
                setValue: value => Config.NPCs = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.Player.name"),
                tooltip: () => this.Helper.Translation.Get("config.Player.description"),
                getValue: () => Config.PlayerSounds,
                setValue: value => Config.PlayerSounds = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.Items.name"),
                tooltip: () => this.Helper.Translation.Get("config.Items.description"),
                getValue: () => Config.ItemSounds,
                setValue: value => Config.ItemSounds = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.Fishing.name"),
                tooltip: () => this.Helper.Translation.Get("config.Fishing.description"),
                getValue: () => Config.FishingCatch,
                setValue: value => Config.FishingCatch = value
            );
        }

        private void RenderedHUD(object sender, RenderedHudEventArgs e)
        {
            if (Game1.eventUp) //if any event is going on. could also use ' Game1.CurrentEvent != null ' 
                return;
            if (Config.TrackCue == false)
                return;
            if (Game1.activeClickableMenu != null)
                return;
            else
            {
                #region variables
                if(SafePositionTop == new Vector2(99f,99f))
                {
                    //
                    int safeX = Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Left;
                    int safeY = Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Top;

                    Vector2 pos = new(safeX + 30, safeY + 20);
                    SafePositionTop = pos;
                }

                //check position
                for (int j = 0; j < Game1.onScreenMenus.Count; j++)
                {
                    if (Game1.onScreenMenus[j] is Toolbar)
                    {

                        //could make all of this OR i could use the toolbar to make a "toolbarregion" rectangle, then check if it intersects SafeTop or RectangleTop or fucking whatever
                        var toolBar = (Game1.onScreenMenus[j] as Toolbar);

                        Mon.Log("Toolbar Y position:" + toolBar.yPositionOnScreen,LogLevel.Info);

                        if (Bottom == 0)
                        {
                            Bottom = Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Bottom - 80;
                            SafePositionBottom = new(
                                (int)SafePositionTop.X,
                                Bottom
                                );

                            ToolbarRect = new Rectangle(
                                toolBar.xPositionOnScreen,
                                toolBar.yPositionOnScreen,
                                toolBar.width,
                                toolBar.height
                                );
                        }
                        
                        //gets variable. changes Y and then sets back to rect
                        var actualRect = ToolbarRect;
                        actualRect.Y = toolBar.yPositionOnScreen;
                        ToolbarRect = actualRect;

                        IsToolbarAbove = TrackZone.Intersects(ToolbarRect);
                    }
                }

                #endregion

                DrawTrack(
                    e.SpriteBatch,
                    IsToolbarAbove ? SafePositionBottom : SafePositionTop, Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Left,
                    Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Top
                    );
            }
}

        private static void DrawTrack(SpriteBatch spriteBatch, Vector2 pos, int x, int y)
        {   
            //if track changed
            if(_lastTrack != Game1.getMusicTrackName())
            {
                #region music track variables
                //get track name and 'playing' size
                var trackname = Game1.isMusicContextActiveButNotPlaying() ? NoMusic : Utility.getSongTitleFromCueName(Game1.getMusicTrackName());

                //if the track has no display name, use ours
                if(trackname == Game1.getMusicTrackName())
                {
                    trackname = Help.Translation.Get("Track." + trackname.ToLower());
                }

                //if islandmusic
                if(trackname == "IslandMusic")
                {
                    trackname = Help.Translation.Get("Track.IslandMusic");
                }
#endregion

                //join both strings
                Musictext = Playing + " " + trackname;

                //measure size
                var size = (int)Game1.dialogueFont.MeasureString(Musictext).X + 5;
                TrackZone = new Rectangle(x + 10, y, size + 32, 80);
                AltTrackZone = new Rectangle(x + 10, Bottom, size + 32, 80);

                //set current track. this way, the calc will only be done when the track changes
                _lastTrack = Game1.getMusicTrackName();
            }
            
            //draw box
            IClickableMenu.drawTextureBox(
                spriteBatch, 
                x + 10, 
                IsToolbarAbove ? Bottom : y, 
                (int)TrackZone.Width, 
                80, 
                Color.White);

            //draw "Playing: <track>"
            Utility.drawTextWithShadow(
                spriteBatch,
                Musictext,
                Game1.dialogueFont,
                pos,
                Color.Black
                );

            //if mouse is hovering over track title
            if(TrackZone.Contains(Game1.getMousePositionRaw()) || (IsToolbarAbove && AltTrackZone.Contains(Game1.getMousePositionRaw())))
            {
                DrawDescription(spriteBatch);
            }
        }

        private static void DrawDescription(SpriteBatch spriteBatch)
        {
            //get description for track, or default
            string desc = Game1.parseText(
                Help.Translation.Get(
                    "Description." + Game1.getMusicTrackName()) ?? Help.Translation.Get("noDesc"),
                Game1.smallFont,
                (int)Game1.smallFont.MeasureString(Playing).X * 2
                );

            //measure string
            var descSize = Game1.smallFont.MeasureString(desc);

            //depending on toolbar, set position
            var boxY = IsToolbarAbove ? (int)SafePositionBottom.Y - (int)descSize.Y : TrackZone.Y + TrackZone.Height + 5;

            //draw box (for description)
            IClickableMenu.drawTextureBox(
                spriteBatch, 
                TrackZone.X + 5, 
                boxY, 
                (int)descSize.X,
                (int)descSize.Y, 
                Color.White);

            //draw description

            Utility.drawTextWithShadow(
            spriteBatch,
            desc,
            Game1.smallFont,
            new Vector2(
                TrackZone.X + 5, 
                boxY
                ),
            Color.Black
            );
        }
    }
}