using HarmonyLib;
using ItemExtensions.Additions;
using ItemExtensions.Models;
using ItemExtensions.Patches;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Triggers;
using Object = StardewValley.Object;

namespace ItemExtensions;

public sealed class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        helper.Events.GameLoop.GameLaunched += OnGameLaunch;
        helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        
        helper.Events.Content.AssetRequested += Assets.OnRequest;
        helper.Events.Content.AssetsInvalidated += Assets.OnReload;
        
        helper.Events.Input.ButtonPressed += Additions.ActionButton.Pressed;
        helper.Events.World.ObjectListChanged += Additions.World.ObjectListChanged;
        
        helper.Events.Content.LocaleChanged += LocaleChanged;
        
        Mon = Monitor;
        Help = Helper;
        Id = ModManifest.UniqueID;
        
        var harmony = new Harmony(ModManifest.UniqueID);
        
        FarmerPatches.Apply(harmony);
        GameLocationPatches.Apply(harmony);
        ItemPatches.Apply(harmony);
        MenuPatches.Apply(harmony);
        ObjectPatches.Apply(harmony);
        ShopMenuPatches.Apply(harmony);
        
        GameStateQuery.Register($"{Id}_ToolUpgrade", Queries.ToolUpgrade);
        GameStateQuery.Register($"{Id}_InInventory", Queries.InInventory);
        
        #region trigger actions
        TriggerActionManager.RegisterTrigger($"{Id}_OnBeingHeld");
        TriggerActionManager.RegisterTrigger($"{Id}_OnStopHolding");
        
        TriggerActionManager.RegisterTrigger($"{Id}_OnPurchased");
        TriggerActionManager.RegisterTrigger($"{Id}_OnItemRemoved");
        TriggerActionManager.RegisterTrigger($"{Id}_OnItemDropped");
        
        TriggerActionManager.RegisterTrigger($"{Id}_OnEquip");
        TriggerActionManager.RegisterTrigger($"{Id}_OnUnequip");
        
        TriggerActionManager.RegisterTrigger($"{Id}_AddedToStack");
        #endregion
        
        #if DEBUG
        helper.ConsoleCommands.Add("test", "Tests mod content.", Test);
        #endif
    }

    private void OnGameLaunch(object sender, GameLaunchedEventArgs e)
    {
        #if DEBUG
        Assets.WriteTemplates();
        #endif

        //get obj data
        _ = Help.GameContent.Load<Dictionary<string, ItemData>>($"Mods/{Id}/Items");
        //Parser.ObjectData(objData);
        //var dc = Data?.Count ?? 0;
    }

    private void Test(string arg1, string[] arg2)
    {
        if (!arg2.Any())
        {
            Mon.Log("No input given.", LogLevel.Warn);
            return;
        }

        switch (arg2[0])
        {
            case "animation":
            case "anim": 
            case "food": 
            case "eat":
                FarmerPatches.Pre_eatObject(Game1.player, new Object("mistyspring.testobj_Gem", 1));
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
        //get item actions
        var menuActions = Help.GameContent.Load<Dictionary<string, List<MenuBehavior>>>($"Mods/{Id}/MenuActions");
        Parser.ItemActions(menuActions);
        var ic = ItemActions?.Count ?? 0;
        Monitor.Log($"Loaded {ic} item actions.", LogLevel.Debug);
        
        //get obj data
        var objData = Help.GameContent.Load<Dictionary<string, ItemData>>($"Mods/{Id}/Items");
        Parser.ObjectData(objData);
        var dc = Data?.Count ?? 0;
        Monitor.Log($"Loaded {dc} item data.", LogLevel.Debug);
        
        //get custom animations
        var animations = Help.GameContent.Load<Dictionary<string, FarmerAnimation>>($"Mods/{Id}/EatingAnimations");
        Parser.EatingAnimations(animations);
        var ac = EatingAnimations?.Count ?? 0;
        Monitor.Log($"Loaded {ic} item actions.", LogLevel.Debug);
        
        //get shop ext
        var shopExtensions = Help.GameContent.Load<Dictionary<string, Dictionary<string, List<ExtraTrade>>>>($"Mods/{Id}/Shops");
        Parser.ShopExtension(shopExtensions);
        var sc = ExtraTrades?.Count ?? 0;
        Monitor.Log($"Loaded {sc} shop extensions.", LogLevel.Debug);

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

    #region item related
    internal static bool Holding { get; set; }
    internal static Dictionary<string, List<MenuBehavior>> ItemActions { get; set; } = new();
    public static Dictionary<string, ItemData> Data { get; set; } = new();
    internal static Dictionary<string, FarmerAnimation> EatingAnimations { get; set; } = new();
    internal static Dictionary<string, Dictionary<string, List<ExtraTrade>>> ExtraTrades { get; set; } = new();
    #endregion
}