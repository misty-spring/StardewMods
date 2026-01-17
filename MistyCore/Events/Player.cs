using Microsoft.Xna.Framework;
using MistyCore.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace MistyCore.Events;

public static class Player
{
    private static void Log(string str) => ModEntry.Mon.Log(str);

    public static void OnWarp(object sender, WarpedEventArgs e)
    {
        if (ModEntry.Config.CritterSpawning == false)
            return;

        if (ModEntry.CritterSpawning.TryGetValue(e.NewLocation.Name, out var spawnData) == false || spawnData.Count == 0)
        {
#if DEBUG
	     Log("Couldn't find data for this location.");
#endif
	     return; 
        }

        //instantiate
	e.NewLocation.instantiateCrittersList();

        foreach (var critterSpawn in spawnData)
        { 
            /* This didn't work out because before I could test the code I realized they all have different parameters
             *
               var type = typeof(Critter);
               var spawnType = type.Assembly.GetType(nameof(critterSpawn.Critter));
               if (spawnType != null)
               {
                   var invoked = spawnType.InvokeMember(nameof(critterSpawn.Critter), BindingFlags.Default, null, null,
                       null);
                   var critter = invoked as Critter;
                   e.NewLocation.critters.Add(critter);
               }
             */
            
            if (Game1.random.NextDouble() > critterSpawn.Value.Chance)
            {
	            Log($"Double was higher than chance for critter {critterSpawn.Key}. Skipping...");
	            continue; 
            }
	
            if (GameStateQuery.CheckConditions(critterSpawn.Value.Condition, e.NewLocation, Game1.player) == false)
            {
	            Log($"Conditions didn't apply for critter {critterSpawn.Key}. Skipping...");
	            continue;
            }
            
            Critter critter = null;
            var vector2 = new Vector2(critterSpawn.Value.X, critterSpawn.Value.Y);

            critter = critterSpawn.Value.Critter switch
            {
	            CritterType.Butterfly => new Butterfly(e.NewLocation, vector2, critterSpawn.Value.IslandButterfly, critterSpawn.Value.ForceSummerButterfly),
	            CritterType.CalderaMonkey => new CalderaMonkey(vector2 * 64f),
	            CritterType.CrabCritter => new CrabCritter(vector2 * 64f),
	            CritterType.Crow => new Crow(critterSpawn.Value.X, critterSpawn.Value.Y),
	            CritterType.Firefly => new Firefly(vector2),
	            CritterType.Frog => new Frog(vector2, false, critterSpawn.Value.Flip),
	            CritterType.Opossum => new Opossum(e.NewLocation, vector2, critterSpawn.Value.Flip),
	            CritterType.OverheadParrot => new OverheadParrot(vector2 * 64f),
	            CritterType.Owl => new Owl(vector2 * 64f),
	            CritterType.Rabbit => new Rabbit(e.NewLocation, vector2, critterSpawn.Value.Flip),
	            CritterType.Seagull => new Seagull(vector2 * 64f, (int)critterSpawn.Value.SeagullBehavior),
	            CritterType.Squirrel => new Squirrel(vector2, critterSpawn.Value.Flip),
	            _ => critter
            };

            if (critter == null)
            {
	            ModEntry.Mon.Log($"Critter {critterSpawn.Key} is null. Skipping...", LogLevel.Warn);
	            continue;
            }

            e.NewLocation.critters.Add(critter);
        }
    }
}