namespace ItemExtensions.Models.Internal;

public interface IWorldChangeData
{
    int ReduceBy { get; set; }
    
    string PriceChange { get; set; }
    internal double ActualPrice { get; set; }
    internal Modifier PriceModifier { get; set; }
    
    string QualityChange { get; set; }
    internal int ActualQuality { get; set; }
    internal Modifier QualityModifier { get; set; }
    
    public int TextureIndex { get; set; }
    
    string ChangeMoney { get; set; }
    string Health { get; set; }
    string Stamina { get; set; }
    
    Dictionary<string, int> AddItems { get; set; }
    Dictionary<string, int> RemoveItems { get; set; }
    
    string PlayMusic { get; set; }
    string PlaySound { get; set; }
    
    string AddQuest { get; set; }
    string AddSpecialOrder { get; set; }
    
    string RemoveQuest { get; set; }
    string RemoveSpecialOrder { get; set; }
    
    List<string> AddContextTags { get; set; }
    List<string> RemoveContextTags { get; set; }
    Dictionary<string,string> AddModData { get; set; }
    
    string Condition { get; set; }
    string TriggerAction { get; set; }
    
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