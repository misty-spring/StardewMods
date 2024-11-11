using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace ConfigurableMasteryPoints;

public class ModEntry : Mod
{
    /// <summary>
    /// <see cref="MasteryTrackerMenu"/>
    /// </summary>
    /// <param name="helper"></param>
    /// <exception cref="NotImplementedException"></exception>
    public override void Entry(IModHelper helper)
    {
        Config = Helper.ReadConfig<ModConfig>();
        Mon = Monitor;
        Help = Helper;
        
        helper.Events.GameLoop.GameLaunched += OnLaunch;
        
        var harmony = new Harmony(ModManifest.UniqueID);

        MasteryPatches.Apply(harmony);
    }

    private void OnLaunch(object? sender, GameLaunchedEventArgs e)
    {
        var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        
        // register mod
        configMenu?.Register(
            mod: ModManifest,
            reset: () => Config = new ModConfig(),
            save: () => Helper.WriteConfig(Config)
        );
        
        configMenu?.SetTitleScreenOnlyForNextOptions(ModManifest, true);

        var level = Helper.Translation.Get("Level");
        
        configMenu?.AddNumberOption(
            mod:  ModManifest,
            name: () => string.Format(level, 1),
            getValue: () => Config.Level1,
            setValue: value => Config.Level1 = value
        );
        
        configMenu?.AddNumberOption(
            mod:  ModManifest,
            name: () => string.Format(level, 2),
            getValue: () => Config.Level2,
            setValue: value => Config.Level2 = value
        );
        
        configMenu?.AddNumberOption(
            mod:  ModManifest,
            name: () => string.Format(level, 3),
            getValue: () => Config.Level3,
            setValue: value => Config.Level3 = value
        );
        
        configMenu?.AddNumberOption(
            mod:  ModManifest,
            name: () => string.Format(level, 4),
            getValue: () => Config.Level4,
            setValue: value => Config.Level4 = value
        );
        
        configMenu?.AddNumberOption(
            mod:  ModManifest,
            name: () => string.Format(level, 5),
            getValue: () => Config.Level5,
            setValue: value => Config.Level5 = value
        );
    }

    public static ModConfig Config { get; private set; }

    internal static IModHelper Help { get; private set; }

    internal static IMonitor Mon { get; private set; }
}