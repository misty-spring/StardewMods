using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace PrismaticButterflyNotifier;

public class ModEntry : Mod
{
    private static ModConfig Config { get; set; }
    public static IModHelper Help { get; set; }
    internal static IMonitor Mon { get; private set; }
    public override void Entry(IModHelper helper)
    {
        Config = Helper.ReadConfig<ModConfig>();
        Mon = Monitor;
        Help = Helper;

        helper.Events.GameLoop.GameLaunched += OnLaunch;
        
        var harmony = new Harmony(ModManifest.UniqueID);
        Monitor.Log($"Applying Harmony patch \"{nameof(ModEntry)}\": transpiling SDV method \"GameLocation.tryAddPrismaticButterfly\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.tryAddPrismaticButterfly)),
            transpiler: new HarmonyMethod(typeof(ModEntry), nameof(Transpiler))
        );
#if DEBUG
        helper.ConsoleCommands.Add("prismaticbutterfly", "Adds the corresponding prismatic buff.", PrismaticButterfly);
#endif
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
        
        configMenu?.AddBoolOption(
            mod: ModManifest,
            name: () => Help.Translation.Get("config.ShowCoords.name"),
            getValue: () => Config.ShowCoords,
            setValue: value => Config.ShowCoords = value
        );
    }

    private static void PrismaticButterfly(string arg1, string[] arg2)
    {
        Game1.player.applyBuff("statue_of_blessings_6");
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        //new one
        var codes = new List<CodeInstruction>(instructions);
        var instructionsToInsert = new List<CodeInstruction>();

        var index = codes.FindIndex(ci => ci.opcode == OpCodes.Ret);
#if DEBUG
        Mon.Log($"index: {index}", LogLevel.Info);
#endif
        if (index <= -1) 
            return codes.AsEnumerable();
        
        /* ...end of code
         * ShowPrismaticButterfly()
         */
        
        //arguments
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg, 0)); //spawndata arg
        
        //call my code w/ prev args
        instructionsToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ShowPrismaticButterfly))));

        Mon.Log($"codes count: {codes.Count}, insert count: {instructionsToInsert.Count}");
        Mon.Log("Inserting method");
        codes.InsertRange(index, instructionsToInsert);
        
        return codes.AsEnumerable();
    }
    
    public static void ShowPrismaticButterfly(GameLocation __instance)
    {
        Mon.Log($"{__instance.DisplayName}");
        var coords = Vector2.Zero;

        foreach (var critter in __instance.critters)
        {
            if (critter is not Butterfly { isPrismatic: true }) 
                continue;

            coords = critter.position / 64;
        }
        
        var text = string.Format(Help.Translation.Get(Config.ShowCoords ? "ButterflyLocationCoords" : "ButterflyLocation"), coords);
        Game1.addHUDMessage(new HUDMessage(text, HUDMessage.newQuest_type));
    }
}