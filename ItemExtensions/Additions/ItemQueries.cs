using System.Runtime.CompilerServices;
using ItemExtensions.Models;
using ItemExtensions.Models.Internal;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace ItemExtensions.Additions;

public class ItemQueries
{
    /// <summary>
    /// Returns all clumps. See <see cref="ItemQueryResolver.DefaultResolvers.ALL_ITEMS"/>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="arguments"></param>
    /// <param name="context"></param>
    /// <param name="avoidrepeat"></param>
    /// <param name="avoiditemids"></param>
    /// <param name="logerror"></param>
    /// <returns>All clumps that meet conditions.</returns>
    public static IEnumerable<ItemQueryResult> AllClumps(string key, string arguments, ItemQueryContext context, bool avoidrepeat, HashSet<string> avoiditemids, Action<string, string> logerror)
    {
        /* ALL_CLUMPS [flags] */
        return SolveQuery(key, arguments, logerror);
    }

    private static IEnumerable<ItemQueryResult> SolveQuery(string key, string arguments, Action<string, string> logerror)
    {
        var array = ItemQueryResolver.Helpers.SplitArguments(arguments);
        ResolveArguments(key, array, out var tool, out var skill, out var statCounter, out var width, out var height, out var hasLight, out var hasExtraDrops, out var secretNotes, out var addsHay);

        var weapons = new string[]{ "meleeweapon", "weapon", "dagger", "club", "hammer", "sword", "slash", "slashing", "slashing sword", "slashingsword", "stabbing sword", "stabbingsword" };
        
        foreach (var (id, clump) in ModEntry.BigClumps)
        {
            //if has tool AND tool isn't "any"
            if (!string.IsNullOrWhiteSpace(tool) && tool.Equals("any", IgnoreCase) == false)
            {
                //if a weapon
                if (tool.Equals("weapon", IgnoreCase))
                {
                    //if int
                    if (int.TryParse(clump.Tool, out var number))
                    {
                        if(number < 0 || number > 3)
                            continue;
                    }
                    //if target tool isn't weapon
                    else if(weapons.Contains(clump.Tool.ToLower()) == false)
                        continue;
                }
                
                //if not, compare
                if(tool.Equals(clump.Tool, IgnoreCase) == false)
                    continue;
            }
            //if skill requirement (-2 any, otherwise specific)
            if (skill != -1)
            {
                //if any and clump DOESN'T have skill
                if(skill == -2 && clump.ActualSkill <= -1)
                    continue;
                //otherwise compare them
                else if(skill != clump.ActualSkill)
                    continue;
            }
            
            if (statCounter.HasValue && statCounter != clump.CountTowards)
                continue;

            if (height.Item1 > 0)
            {
                //if doesnt reach minimum
                if (height.Item1 >  clump.Height)
                    continue;
                //if passes maximum
                if(height.Item2 < clump.Height)
                    continue;
            }
            
            if (width.Item1 > 0)
            {
                //if doesnt reach minimum
                if (width.Item1 >  clump.Width)
                    continue;
                //if passes maximum
                if(width.Item2 < clump.Width)
                    continue;
            }
            
            if (hasLight.HasValue)
            {
                if (hasLight.Value == true && clump.Light == null)
                    continue;
                if(hasLight.Value == false && clump.Light != null)
                    continue;
            }
            
            if (hasExtraDrops.HasValue)
            {
                if (hasExtraDrops.Value == true && clump.ExtraItems == null)
                    continue;
                if(hasExtraDrops.Value == false && clump.ExtraItems != null)
                    continue;
            }
            
            if (secretNotes is true && clump.SecretNotes == false)
                continue;
            
            if (addsHay.Item1 >= 0)
            {
                //if doesnt reach minimum
                if (addsHay.Item1 >  clump.AddHay)
                    continue;
                //if passes maximum
                if(addsHay.Item2 < clump.AddHay)
                    continue;
            }
            
            yield return new ItemQueryResult(new Object { ItemId = id });
        }
    }

    private static void ResolveArguments(string key, string[] array, out string tool, out int skill, out StatCounter? statCounter, out (int,int) width, out (int,int) height, out bool? hasLight, out bool? hasExtraDrops,  out bool? secretNotes, out (int,int)  addsHay)
    { 
        /*
         * flags:
         * - @tool:TYPE
         * - @skill:TYPE
         * - @stat:TYPE
         * - @width:min_max
         * - @height:min_max
         * - @hasLight
         * - @hasExtraDrops         //if accompanied of a :, checks ids separately
         * - @addsHay
         * - @secretNotes
         */
        
        tool = null;
        skill = -1;
        statCounter = null;
        width = (0, 999);
        height = (0, 999);
        hasLight = null;
        hasExtraDrops = null;
        addsHay = (-1, 999);
        secretNotes = null;
        
        //for parsing
        string[] minmax;
        int min;
        int max;
        
        
        for (var index = 0; index < array.Length; ++index)
        {
          var str = array[index];
          //light
          if (str.Equals("@hasLight", IgnoreCase))
          {
              hasLight = true;
          }
          else if (str.Equals("@noLight", IgnoreCase))
          {
              hasLight = false;
          }
          //has drops
          else if (str.Equals("@hasExtraDrops", IgnoreCase))
          {
              hasExtraDrops = true;
          }
          else if (str.Equals("@noExtraDrops", IgnoreCase))
          {
              hasExtraDrops = false;
          }
          //has hay
          else if (str.Equals("@addsHay", IgnoreCase))
          {
              addsHay = (1, 999);
          }
          //specific hay
          else if (str.StartsWith("@addsHay:", IgnoreCase))
          {
              minmax = str.Remove(0,9).Split('_');
              ArgUtility.TryGetInt(minmax, 0, out min, out _);
              ArgUtility.TryGetOptionalInt(minmax, 1, out max, out _, 999);
              addsHay = (min, max);
          }
          //has notes
          else if (str.Equals("@secretNotes", IgnoreCase))
          {
              secretNotes = true;
          }
          //has skill
          else if (str.Equals("@skill", IgnoreCase))
          {
              skill = -2; //must have any
          }
          else if (str.StartsWith("@skill:", IgnoreCase))
          {
              var skillType = str.Remove(0,7);
              skill = ResourceData.GetSkill(skillType);
          }
          //this tool type
          else if (str.StartsWith("@tool:", IgnoreCase))
          {
              var tooltype = str.Remove(0,6);
              tool = tooltype;
          }
          //any stat
          else if (str.Equals("@statcounter", IgnoreCase))
          {
              statCounter = StatCounter.Any;
          }
          //specific stat
          else if (str.StartsWith("@statcounter:", IgnoreCase))
          {
              var trimmed = str.Remove(0, 13);
              if(Enum.TryParse<StatCounter>(trimmed, out var stat))
                  statCounter = stat;
          }
          else if (str.StartsWith("@width:", IgnoreCase))
          {
              minmax = str.Remove(0,7).Split('_');
              ArgUtility.TryGetInt(minmax, 0, out min, out _);
              ArgUtility.TryGetOptionalInt(minmax, 1, out max, out _, 999);
              width = (min, max);
          }
          else if (str.StartsWith("@height:", IgnoreCase))
          {
              minmax = str.Remove(0,8).Split('_');
              ArgUtility.TryGetInt(minmax, 0, out min, out _);
              ArgUtility.TryGetOptionalInt(minmax, 1, out max, out _, 999);
              height = (min, max);
          }
          else if (str.StartsWith('@'))
          {
              throw new ArgumentException($"{key}: index {index} has unknown option flag '{str}'");
          }
        }
    }

    private const StringComparison IgnoreCase = StringComparison.OrdinalIgnoreCase;

    /// <summary>
    /// See <see cref="ItemQueryResolver.DefaultResolvers.RANDOM_ITEMS"/>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="arguments"></param>
    /// <param name="context"></param>
    /// <param name="avoidrepeat"></param>
    /// <param name="avoiditemids"></param>
    /// <param name="logerror"></param>
    /// <returns></returns>
    public static IEnumerable<ItemQueryResult> RandomClumps(string key, string arguments, ItemQueryContext context, bool avoidrepeat, HashSet<string> avoiditemids, Action<string, string> logerror)
    {
        //RANDOM_CLUMPS [min amt] [max amt] [flags]
        
        var all = SolveQuery(key, arguments, logerror);
        var random = context.Random ?? Game1.random;
        foreach (var data in all)
        {
            if (random.NextBool())
                yield return data;
        }
    }
}