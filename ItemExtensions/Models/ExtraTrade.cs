using StardewModdingAPI;
using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace ItemExtensions.Models;

public class ExtraTrade
{
    private static void Log(string msg, LogLevel lv = LogLevel.Trace) => ModEntry.Mon.Log(msg, lv);
    public string QualifiedItemId { get; set; }
    public int Count { get; set; } = 1;
    /*private string Quality { get; set; } = "0";
    internal int ActualQuality { get; set; }*/
    internal ParsedItemData Data { get; set; }

    public ExtraTrade(string qualifiedId, int count)
    {
        QualifiedItemId = qualifiedId;
        Count = count;
    }
    internal bool IsValid(out ExtraTrade data)
    {
        try 
        {
            /*ActualQuality = int.TryParse(Quality, out var intQuality) ? intQuality : GetQualityFromString(Quality);
            if (ActualQuality == 3)
                ActualQuality = 4;*/

            QualifiedItemId = ItemRegistry.QualifyItemId(QualifiedItemId);
            Data = ItemRegistry.GetDataOrErrorItem(QualifiedItemId);
            data = this;
            var errorOrNot = Data.IsErrorItem ? "was not found." : "was found";
            
            Log($"Item {QualifiedItemId} {errorOrNot}.");
            
            return !Data.IsErrorItem;
        }
        catch(Exception e)
        {
            data = null;
            Log($"Error: {e}", LogLevel.Error);
            return false;
        }
    }

    private static int GetQualityFromString(string quality)
    {
        var result = quality.ToLower() switch
        {
            "silver" or "low" => 1,
            "gold" or "mid" or "middle" => 2,
            "iridium" or "high" or "max" => 4,
            _ => 0
        };
        return result;
    }
}