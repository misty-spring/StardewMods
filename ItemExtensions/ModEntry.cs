using HarmonyLib;
using ItemExtensions.Additions;
using ItemExtensions.Events;
using ItemExtensions.Models;
using ItemExtensions.Models.Contained;
using ItemExtensions.Models.Internal;
using ItemExtensions.Patches;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Triggers;

namespace ItemExtensions;

public sealed class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        #if DEBUG
        helper.Events.GameLoop.GameLaunched += Assets.WriteTemplates;
        #endif
        
        helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        helper.Events.GameLoop.DayStarted += Day.Started;
        helper.Events.GameLoop.DayEnding += Day.Ending;
        
        helper.Events.Content.AssetRequested += Assets.OnRequest;
        helper.Events.Content.AssetsInvalidated += Assets.OnReload;
        
        helper.Events.Input.ButtonPressed += ActionButton.Pressed;
        helper.Events.World.ObjectListChanged += World.ObjectListChanged;
        
        helper.Events.Content.LocaleChanged += LocaleChanged;
        
        Mon = Monitor;
        Help = Helper;
        Id = ModManifest.UniqueID;
        
        // patches
        var harmony = new Harmony(ModManifest.UniqueID);
        CropPatches.Apply(harmony);
        FarmerPatches.Apply(harmony);
        GameLocationPatches.Apply(harmony);
        InventoryPatches.Apply(harmony);
        ItemPatches.Apply(harmony);
        ObjectPatches.Apply(harmony);
        ShopMenuPatches.Apply(harmony);
        UtilityPatches.Apply(harmony);
        
        if(helper.ModRegistry.Get("mistyspring.dynamicdialogues") is null)
            NpcPatches.Apply(harmony);
        
        //GSQ
        GameStateQuery.Register($"{Id}_ToolUpgrade", Queries.ToolUpgrade);
        GameStateQuery.Register($"{Id}_InInventory", Queries.InInventory);
        
        // trigger actions
        TriggerActionManager.RegisterTrigger($"{Id}_OnBeingHeld");
        TriggerActionManager.RegisterTrigger($"{Id}_OnStopHolding");
        
        TriggerActionManager.RegisterTrigger($"{Id}_OnPurchased");
        TriggerActionManager.RegisterTrigger($"{Id}_OnItemRemoved");
        TriggerActionManager.RegisterTrigger($"{Id}_OnItemDropped");
        
        TriggerActionManager.RegisterTrigger($"{Id}_OnEquip");
        TriggerActionManager.RegisterTrigger($"{Id}_OnUnequip");
        
        TriggerActionManager.RegisterTrigger($"{Id}_AddedToStack");
        
        #if DEBUG
        helper.ConsoleCommands.Add("ie", "Tests ItemExtension's mod capabilities", Tester);
        #endif
    }

    private void Tester(string arg1, string[] arg2)
    {
        if (arg2 is null || arg2.Any() == false)
        {
            Mon.Log("Must have at least 1 argument.", LogLevel.Warn);
            return;
        }

        if(!Context.IsWorldReady)
            Mon.Log("Must load a save.", LogLevel.Warn);

        const string testPack = "mistyspring.testobj";
        
        if (Helper.ModRegistry.Get(testPack) is null)
        {
            Mon.Log("The test pack was not found.", LogLevel.Error);
            return;
        }
            
        switch (arg2[0])
        {
            case "ore":
            case "clump":
                var pos = new Vector2(Game1.player.Tile.X + 2, Game1.player.Tile.Y);
                var clump = new ExtensionClump($"{testPack}_TestClump", pos);
                Mon.Log($"Adding clump ID {clump.ResourceId} at {pos}...", LogLevel.Info);
                Game1.player.currentLocation.resourceClumps.Add(clump);
                break;
            case "jelly":
                Game1.player.eatObject(new StardewValley.Object($"{testPack}_Jelly",1));
                break;
            case "eat":
                Game1.player.eatObject(new StardewValley.Object($"{testPack}_trash",1));
                break;
            case "sip":
            case "drink":
                Game1.player.eatObject(new StardewValley.Object("614",1));
                break;
            case "list":
                Mon.Log($"Possible commands: eat, clump, drink, jelly.", LogLevel.Info);
                break;
            default:
                Mon.Log($"Command {arg2[0]} not recognized.", LogLevel.Warn);
                break;
        }
    }

    public override object GetApi() =>new Api();

    private static void LocaleChanged(object sender, LocaleChangedEventArgs e)
    {
        Comma = e.NewLanguage switch
        {
            LocalizedContentManager.LanguageCode.ja => "、",
            LocalizedContentManager.LanguageCode.zh => "，",
            _ => ", "
        };
    }

    private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
    {
        //get obj data
        var objData = Help.GameContent.Load<Dictionary<string, ItemData>>($"Mods/{Id}/Data");
        Parser.ObjectData(objData);
        var dc = Data?.Count ?? 0;
        Monitor.Log($"Loaded {dc} item data.", LogLevel.Debug);
        
        //get custom animations
        var animations = Help.GameContent.Load<Dictionary<string, FarmerAnimation>>($"Mods/{Id}/EatingAnimations");
        Parser.EatingAnimations(animations);
        var ac = EatingAnimations?.Count ?? 0;
        Monitor.Log($"Loaded {ac} eating animations.", LogLevel.Debug);
        
        //get item actions
        var menuActions = Help.GameContent.Load<Dictionary<string, List<MenuBehavior>>>($"Mods/{Id}/MenuActions");
        Parser.ItemActions(menuActions);
        var ic = MenuActions?.Count ?? 0;
        Monitor.Log($"Loaded {ic} menu actions.", LogLevel.Debug);
        
        // get shop ext
        var shopExtensions = Help.GameContent.Load<Dictionary<string, Dictionary<string, List<ExtraTrade>>>>($"Mods/{Id}/Shops");
        Parser.ShopExtension(shopExtensions);
        var sc = Shops?.Count ?? 0;
        Monitor.Log($"Loaded {sc} shop extensions.", LogLevel.Debug);
        
        // get resources
        var oreData = Help.GameContent.Load<Dictionary<string, ResourceData>>($"Mods/{Id}/Resources");
        Parser.Resources(oreData);
        var oc = Ores?.Count ?? 0;
        Monitor.Log($"Loaded {oc} custom resources.", LogLevel.Debug);

        var temp = new List<SButton>();
        foreach (var b in Game1.options.actionButton)
        {
            temp.Add(b.ToSButton());
            Monitor.Log("Button: " + b);
        }
        Monitor.Log($"Total {Game1.options.actionButton.Length}");

        ActionButtons = temp;
    }

    internal static List<SButton> ActionButtons { get; set; }

    public static string Id { get; set; }
    internal static string Comma { get; private set; } = ", ";

    internal static IModHelper Help { get; set; }
    internal static IMonitor Mon { get; set; }
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif

    internal static bool Holding { get; set; }
    public static Dictionary<string, ResourceData> BigClumps { get; set; } = new();
    public static Dictionary<string, ItemData> Data { get; set; } = new();
    internal static Dictionary<string, FarmerAnimation> EatingAnimations { get; set; } = new();
    internal static Dictionary<string, List<MenuBehavior>> MenuActions { get; set; } = new();
    public static Dictionary<string, ResourceData> Ores { get; set; } = new();
    internal static Dictionary<string, Dictionary<string, List<ExtraTrade>>> Shops { get; set; } = new();
    internal static Dictionary<string, List<MixedSeedData>> Seeds { get; set; }
}