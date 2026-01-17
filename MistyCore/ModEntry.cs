using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using MistyCore.Additions;
using MistyCore.Additions.EventCommands;
using MistyCore.Models;
using MistyCore.Patches;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Triggers;
// ReSharper disable MemberCanBePrivate.Global

namespace MistyCore;

// ReSharper disable once ClassNeverInstantiated.Global
public class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        Config = Helper.ReadConfig<ModConfig>();

        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.GameLoop.SaveLoaded += OnSaveLoad;
        helper.Events.Content.AssetRequested += Events.Asset.OnRequest;
        helper.Events.Player.Warped += Events.Player.OnWarp;
        helper.Events.Display.RenderedHud += Events.Display.RenderedHud;
        
        Mon = Monitor;
        Help = Helper;

        // TAS related
        Event.RegisterCommand("addScene", AnimatedSprites.AddScene);
        Event.RegisterCommand("removeScene", AnimatedSprites.RemoveScene);
        Event.RegisterCommand("addFire", AnimatedSprites.AddFire);
        Event.RegisterCommand("removeFire", AnimatedSprites.RemoveFire);

        // world related
        Event.RegisterCommand("objectHunt", World.ObjectHunt);

        // event extension
        Event.RegisterCommand("if", Extensions.IfElse);
        Event.RegisterCommand("foreach", Extensions.Foreach);
        Event.RegisterCommand("append", Extensions.Append);

        // character related
        Event.RegisterCommand("resetName", Characters.ResetName);

        // player related
        Event.RegisterCommand("health", Player.Health);
        Event.RegisterCommand("stamina", Player.Stamina);
        Event.RegisterCommand("multiplayerMail", Player.MultiplayerMail);
        Event.RegisterCommand("addExp", Player.AddExp); 
        Event.RegisterCommand("makeInvincible", Player.MakeInvincible);

        GameStateQuery.Register(ModId + "_PlayerWearing", Queries.Wearing);
        
        TriggerActionManager.RegisterAction(ModId + "_AddExp", TriggerActions.AddExp);
        TriggerActionManager.RegisterAction(ModId + "_addItemHoldUp", TriggerActions.AddItemHoldUp);
        
        GameLocation.RegisterTileAction(ModId + "_AddItem", TileAction.AddItem); 
        GameLocation.RegisterTileAction(ModId + "_Question", TileAction.Question);
        GameLocation.RegisterTileAction(ModId + "_ConditionalWarp", TileAction.ConditionalWarp);

        var harmony = new Harmony(ModManifest.UniqueID);

        EventPatches.Apply(harmony);
        GameLocationPatches.Apply(harmony); 
        TreePatches.Apply(harmony);
    }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        var configMenu = Help.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        
        if (configMenu is null) 
            return;
        
        // register mod
        configMenu.Register(
            mod: ModManifest,
            reset: () => Config = new ModConfig(),
            save: () => Helper.WriteConfig(Config)
        );
        
        configMenu.SetTitleScreenOnlyForNextOptions(ModManifest, true);
        
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.CustomHoeDirt.name"),
            tooltip: () => Helper.Translation.Get("config.CustomHoeDirt.description"),
            getValue: () => Config.CustomHoeDirt,
            setValue: value => Config.CustomHoeDirt = value
            );
        
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.CustomBackgrounds.name"),
            tooltip: () => Helper.Translation.Get("config.CustomBackgrounds.description"),
            getValue: () => Config.CustomBackgrounds,
            setValue: value => Config.CustomBackgrounds = value
            );
        
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.CritterSpawning.name"),
            tooltip: () => Helper.Translation.Get("config.CritterSpawning.description"),
            getValue: () => Config.CritterSpawning,
            setValue: value => Config.CritterSpawning = value
            );
        
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.FishingOverrides.name"),
            tooltip: () => Helper.Translation.Get("config.FishingOverrides.description"),
            getValue: () => Config.FishingOverrides,
            setValue: value => Config.FishingOverrides = value
            );
        
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.TreeChopEvents.name"),
            tooltip: () => Helper.Translation.Get("config.TreeChopEvents.description"),
            getValue: () => Config.TreeChopEvents,
            setValue: value => Config.TreeChopEvents = value
            );
        
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.TreeFallEvents.name"),
            tooltip: () => Helper.Translation.Get("config.TreeFallEvents.description"),
            getValue: () => Config.TreeFallEvents,
            setValue: value => Config.TreeFallEvents = value
            );
    }
    
    private void OnSaveLoad(object sender, SaveLoadedEventArgs e)
    {
        if (Config.CustomHoeDirt)
        {
            HoeDirt = Helper.GameContent.Load<Dictionary<string,string>>($"Mods/{ModId}/Locations/HoeDirt");
        }
        if (Config.CustomBackgrounds)
        {
            Backgrounds = Helper.GameContent.Load<Dictionary<string,BackgroundData>>($"Mods/{ModId}/Locations/Backgrounds");
        }

        ObjectHunt = Helper.GameContent.Load<Dictionary<string, HuntContext>>("Mods/"+ModId+"/Commands/objectHunt");

        if (Config.FishingOverrides)
        {
            FishingOverrides = Helper.GameContent.Load<Dictionary<string, FishingOverrideData>>($"Mods/{ModId}/Locations/FishingOverrides");
        }
        
        AddItemTileAction = Helper.GameContent.Load<Dictionary<string, ItemAdditionData>>($"Mods/{ModId}/TileActions/AddItem");
        
        QuestionTileAction = Helper.GameContent.Load<Dictionary<string, QuestionTileActionData>>($"Mods/{ModId}/TileActions/Question");

        if (Config.CritterSpawning)
        {
            CritterSpawning = Helper.GameContent.Load<Dictionary<string, Dictionary<string,CritterSpawnData>>>($"Mods/{ModId}/Locations/Critters");
        }

        CustomCursors = Help.ModContent.Load<Texture2D>("assets/Cursors-export.png");
        
        ConditionalWarpTileAction = Helper.GameContent.Load<Dictionary<string,QuestionTileActionData>>($"Mods/{ModId}/TileActions/ConditionalWarp");

        if (Config.TreeFallEvents)
        {
            TreeFallEvents = Helper.GameContent.Load<Dictionary<string, Dictionary<string,ResourceDestroyData>>>($"Mods/{ModId}/Events/OnTreeFall");
        }
        
        if (Config.TreeChopEvents)
        {
            TreeChopEvents = Helper.GameContent.Load<Dictionary<string, Dictionary<string,ResourceHitData>>>($"Mods/{ModId}/Events/OnTreeChop");
        }
    }

    public static Dictionary<string,Dictionary<string,ResourceHitData>> TreeChopEvents { get; set; } = new();

    public static Dictionary<string, Dictionary<string,ResourceDestroyData>> TreeFallEvents { get; set; } = new();

    public static Texture2D CustomCursors { get; set; }

    public static Dictionary<string,QuestionTileActionData> ConditionalWarpTileAction { get; set; } = new();

    public static Dictionary<string, Dictionary<string,CritterSpawnData>> CritterSpawning { get; set; }

    public static Dictionary<string,QuestionTileActionData> QuestionTileAction { get; set; } = new();

    public static Dictionary<string, ItemAdditionData> AddItemTileAction { get; set; } = new();

    public static Dictionary<string,FishingOverrideData> FishingOverrides { get; set; } = new();

    public static Dictionary<string, HuntContext> ObjectHunt { get; set; } = new();

    public static Dictionary<string, BackgroundData> Backgrounds { get; set; } = new();

    public static Dictionary<string,string> HoeDirt { get; set; } = new();

    internal static IMonitor Mon { get; private set; }
    internal static void Log(string msg, LogLevel lv = Level) => Mon.Log(msg, lv);
#if DEBUG
    internal const LogLevel Level = LogLevel.Debug;
#else
    internal const LogLevel Level =  LogLevel.Trace;
#endif
    internal static IModHelper Help { get; private set; }
    internal static ModConfig Config { get; private set; }
    
    internal const string ModId = "mistyspring.mistycore";
}