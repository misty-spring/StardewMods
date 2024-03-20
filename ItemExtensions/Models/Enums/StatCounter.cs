namespace ItemExtensions.Models.Enums;

/// <summary>
/// What stat to count the resource towards.
/// See <see cref="StardewValley.Stats"/> for possible types
/// </summary>
public enum StatCounter
{
    None,
    Copper,             //Found
    Diamonds,
    GeodesBroken,
    Gold,
    Iridium,
    Iron,
    MysticStones,       //Crushed
    OtherGems,          //:OtherPreciousGemsFound
    PrismaticShards,    //Found
    Stone,              //Gathered
    Stumps,             //Chopped
    Seeds,              //Sown
    Weeds,              //Eliminated
    Any                 //ONLY USED FOR ITEMQUERY   
}