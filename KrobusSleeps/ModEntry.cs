using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace KrobusSleeps;

public class ModEntry : Mod
{
    public bool Enabled { get; set; }

    public static IMonitor Mon { get; set; }
    public ModConfig Config { get; private set; }
    
    public override void Entry(IModHelper helper)
    {
        Config = Helper.ReadConfig<ModConfig>();

        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        helper.Events.GameLoop.TimeChanged += OnTimeChange;

        Mon = Monitor;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        
        if (configMenu is null)
            return;
        
        configMenu?.Register(
            mod: ModManifest,
            reset: () => Config = new ModConfig(),
            save: () => Helper.WriteConfig(Config)
        );
        
        configMenu?.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("config.SleepHour.name"),
            tooltip: () => Helper.Translation.Get("config.SleepHour.description"),
            getValue: () => Config.SleepHour,
            setValue: value => Config.SleepHour = value,
            min: 600,
            max: 2400,
            interval: 100
        );
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        Enabled = IsMarriedToKrobus();
    }

    private void OnTimeChange(object? sender, TimeChangedEventArgs e)
    {
        //if it's not time yet
        if (e.NewTime < Config.SleepHour)
            return;

        var krobus = Game1.getCharacterFromName("Krobus");
        
        //if no krobus
        if (krobus is null)
            return;

        var farmHouse = Utility.getHomeOfFarmer(Game1.player);
        
        //if not home
        if (!krobus.currentLocation.Equals(farmHouse))
            return;
        
        //if sleeping
        if (krobus.isSleeping.Value)
            return;
        
        ModContent.RouteToBed(krobus, farmHouse);
    }

    private static bool IsMarriedToKrobus()
    {
        if (Game1.player.friendshipData.TryGetValue("Krobus", out var krobus) == false)
            return false;

        return krobus.IsMarried() || krobus.RoommateMarriage;
    }
}