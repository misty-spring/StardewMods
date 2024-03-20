using ItemExtensions.Additions;
using ItemExtensions.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace ItemExtensions;

public static class Assets
{
    private static IModHelper Helper => ModEntry.Help;
    private static string Id => ModEntry.Id;
    public static void OnReload(object sender, AssetsInvalidatedEventArgs e)
    {
        if (e.NamesWithoutLocale.Any(a => a.Name.Equals($"Mods/{Id}/Items")))
        {
            var objectData = Helper.GameContent.Load<Dictionary<string, ItemData>>($"Mods/{Id}/Items");
            Parser.ObjectData(objectData);
        }
        
        if (e.NamesWithoutLocale.Any(a => a.Name.Equals($"Mods/{Id}/EatingAnimations")))
        {
            //get menu actions
            var animations = Helper.GameContent.Load<Dictionary<string, FarmerAnimation>>($"Mods/{Id}/EatingAnimations");
            Parser.EatingAnimations(animations);
        }

        if (e.NamesWithoutLocale.Any(a => a.Name.Equals($"Mods/{Id}/MenuActions")))
        {
            //get menu actions
            var itemActionsRaw = Helper.GameContent.Load<Dictionary<string, List<MenuBehavior>>>($"Mods/{Id}/MenuActions");
            Parser.ItemActions(itemActionsRaw);
        }
        
        if (e.NamesWithoutLocale.Any(a => a.Name.Equals($"Mods/{Id}/Shops")))
        {
            //get menu actions
            var shopExtensionRaw = Helper.GameContent.Load<Dictionary<string, Dictionary<string, List<ExtraTrade>>>>($"Mods/{Id}/Shops");
            Parser.ShopExtension(shopExtensionRaw);
        }
    }

    public static void OnRequest(object sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo("Strings/UI"))
        {
            e.Edit(asset =>
            {
                var dictionary = asset.AsDictionary<string, string>();
                dictionary.Data.Add("ItemHover_Requirements_Extra", ModEntry.Help.Translation.Get("ItemHover_Requirements_Extra"));
            });
        }
        
        if (e.NameWithoutLocale.IsEquivalentTo($"Mods/{Id}/EatingAnimations", true))
        {
            e.LoadFrom(
                () => new Dictionary<string, FarmerAnimation>(),
                AssetLoadPriority.Low);
        }
        
        //item actions / object behavior
        if (e.NameWithoutLocale.IsEquivalentTo($"Mods/{Id}/Items", true))
        {
            e.LoadFrom(
                () => new Dictionary<string, ItemData>(),
                AssetLoadPriority.Low);
        }
        
        //item actions / object behavior
        if (e.NameWithoutLocale.IsEquivalentTo($"Mods/{Id}/MenuActions", true))
        {
            e.LoadFrom(
                () => new Dictionary<string, List<MenuBehavior>>(),
                AssetLoadPriority.Low);
        }
        
        //item actions / object behavior
        if (e.NameWithoutLocale.IsEquivalentTo($"Mods/{Id}/Shops", true))
        {
            e.LoadFrom(
                () => new Dictionary<string, Dictionary<string, List<ExtraTrade>>>(),
                AssetLoadPriority.Low);
        }
    }

    public static void WriteTemplates()
    {
        if(Helper.Data.ReadJsonFile<ItemData>("Templates/Item/Model.json") is not null)
            return;
        
        ModEntry.Mon.Log("Writing file templates to mod folder...", LogLevel.Info);
        Helper.Data.WriteJsonFile("Templates/Item/Model.json", new ItemData());
        Helper.Data.WriteJsonFile("Templates/Item/LightData.json", new LightData());
        Helper.Data.WriteJsonFile("Templates/Item/ResourceData.json", new ResourceData());
        Helper.Data.WriteJsonFile("Templates/Item/OnBehavior.json", new OnBehavior());
        Helper.Data.WriteJsonFile("Templates/MenuBehavior.json", new Dictionary<string, List<MenuBehavior>>()
        {
            { "QualifiedItemId", new(){ new MenuBehavior()}}
        });
        Helper.Data.WriteJsonFile("Templates/EatingAnimation.json", new Dictionary<string,FarmerAnimation>()
        {
            {"NameOfAnimation", new() { Food = new FoodAnimation()}}
        });
        var extraTradeTemplate = new Dictionary<string, Dictionary<string, List<ExtraTrade>>>
        {
            {
                "ShopID",
                new()
                {
                    {
                        "QualifiedItemID", new List<ExtraTrade> { new("ItemToTrade", 1) }
                    }
                }
            }
        };
        Helper.Data.WriteJsonFile("Templates/ExtraTrade.json", extraTradeTemplate);
    }
}