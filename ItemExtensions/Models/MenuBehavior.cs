using System.Text.RegularExpressions;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Triggers;

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace ItemExtensions.Models;

public enum Modifier
{
    Set, // null or =
    Sum, // +
    Substract, // -
    Divide, // : or / or \
    Multiply, // * or x
    Percentage // %
}

/// <summary>
/// Behavior on item used.
/// </summary>
/// <see cref="CachedTriggerAction"/>
/// <see cref="StardewValley.Menus.GameMenu"/>
public class MenuBehavior
{
    private static void Log(string msg, LogLevel lv = LogLevel.Trace) => ModEntry.Mon.Log(msg, lv);
    
    public string TargetID { get; set; } //qualified item ID
    public int RemoveAmount { get; set; }
    
    public string ReplaceBy { get; set; } //qualified item ID
    public bool RetainQuality { get; set; } = true;
    public bool RetainAmount { get; set; } = true;
    
    public List<string> AddContextTags { get; set; } = new();
    public List<string> RemoveContextTags { get; set; } = new();
    public Dictionary<string,string> AddModData { get; set; } = new();
    
    public string QualityChange { get; set; }
    internal int ActualQuality { get; set; }
    internal Modifier QualityModifier { get; set; }
    
    public string PriceChange { get; set; }
    internal double ActualPrice { get; set; }
    internal Modifier PriceModifier { get; set; }
    
    public int TextureIndex { get; set; } = -1;
    public string PlaySound { get; set; }
    
    public string TriggerActionID { get; set; }
    public string Conditions { get; set; } = "TRUE";

    public MenuBehavior()
    {}
    
    public MenuBehavior(MenuBehavior i)
    {
        TargetID = i.TargetID;
        RemoveAmount = i.RemoveAmount;
        
        ReplaceBy = i.ReplaceBy;
        RetainAmount = i.RetainAmount;
        RetainQuality = i.RetainQuality;
        
        AddContextTags = i.AddContextTags;
        RemoveContextTags = i.RemoveContextTags;
        AddModData = i.AddModData;

        QualityChange = i.QualityChange;
        PriceChange = i.PriceChange;

        TextureIndex = i.TextureIndex;
        PlaySound = i.PlaySound;
        
        TriggerActionID = i.TriggerActionID;
        Conditions = i.Conditions;
    }

    /// <summary>
    /// Check which price change we're doing, and set modifier
    /// </summary>
    private void ParsePrice()
    {
        if (string.IsNullOrWhiteSpace(PriceChange))
            return;
        
        var raw = PriceChange.Replace(" ", "").Replace(',','.');
        //var first = raw.AsSpan(0,1);
        if (raw.Contains('+'))
        {
            PriceModifier = Modifier.Sum;
        }
        else if (raw.Contains('-'))
        {
            PriceModifier = Modifier.Substract;
        }
        else if (raw.Contains(':') || raw.Contains('/') || raw.Contains('\\'))
        {
            PriceModifier = Modifier.Divide;
        }
        else if (raw.Contains('*') || raw.Contains('x'))
        {
            PriceModifier = Modifier.Multiply;
        }
        else if (raw.Contains('%'))
        {
            PriceModifier = Modifier.Percentage;
        }
        else
        {
            PriceModifier = Modifier.Set;
        }
        
        var stripped = Regex.Replace(raw, "[^0-9.]", "");
        ModEntry.Mon.Log("Stripped string: " + stripped);
        ActualPrice = int.Parse(stripped);
    }

    private void ParseQuality()
    {
        if (string.IsNullOrWhiteSpace(QualityChange))
            return;
        
        var raw = QualityChange.Replace(" ", "");
        //var first = raw.AsSpan(0,1);
        if (raw.Contains('+'))
        {
            QualityModifier = Modifier.Sum;
        }
        else if (raw.Contains('-'))
        {
            QualityModifier = Modifier.Substract;
        }
        else
        {
            QualityModifier = Modifier.Set;
        }
        
        var stripped = Regex.Replace(raw, "[^0-9]", "");
        ActualQuality = int.Parse(stripped);
    }
    
    public bool Parse(out MenuBehavior o)
    {
        try
        {
            ParsePrice();
        }
        catch (Exception e)
        {
            Log("Error when parsing price: "+ e, LogLevel.Error);
            o = null;
            return false;
        }
        
        try
        {
            ParseQuality();
        }
        catch (Exception e)
        {
            Log("Error when parsing quality: "+ e, LogLevel.Error);
            o = null;
            return false;
        }

        if (!string.IsNullOrWhiteSpace(PlaySound) && !Game1.soundBank.Exists(PlaySound))
        {
            Log($"Error: Sound doesn't exist. ({PlaySound})", LogLevel.Error);
            o = null;
            return false;
        }
        
        var target = ItemRegistry.GetDataOrErrorItem(TargetID);
        if (target.DisplayName == ItemRegistry.GetErrorItemName())
        {
            Log("Error finding item. Behavior won't be added.", LogLevel.Error);
            o = null;
            return false;
        }
        
        o = this;
        return true;
    }

    public char? GetQualityModifier()
    {
        return QualityModifier switch
        {
            Modifier.Set => '=',
            Modifier.Sum => '+',
            Modifier.Substract => '-',
            Modifier.Divide => '/',
            Modifier.Multiply => '*',
            Modifier.Percentage => '%',
            _ => null
        };
    }
    
    public char? GetPriceModifier()
    {
        return PriceModifier switch
        {
            Modifier.Set => '=',
            Modifier.Sum => '+',
            Modifier.Substract => '-',
            Modifier.Divide => '/',
            Modifier.Multiply => '*',
            Modifier.Percentage => '%',
            _ => null
        };
    }
}