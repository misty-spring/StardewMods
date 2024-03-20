using ItemExtensions.Additions;
using ItemExtensions.Models;
using ItemExtensions.Models.Contained;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shops;

namespace ItemExtensions.Events;

public static class Assets
{
    private static IModHelper Helper => ModEntry.Help;
    private static string Id => ModEntry.Id;
    public static void OnReload(object sender, AssetsInvalidatedEventArgs e)
    {
        if (e.NamesWithoutLocale.Any(a => a.Name.Equals($"Mods/{Id}/Data")))
        {
            var objectData = Helper.GameContent.Load<Dictionary<string, ItemData>>($"Mods/{Id}/Data");
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
        
        if (e.NamesWithoutLocale.Any(a => a.Name.Equals($"Mods/{Id}/MixedSeeds")))
        {
            //get menu actions
            var seeds = Helper.GameContent.Load<Dictionary<string, List<MixedSeedData>>>($"Mods/{Id}/MixedSeeds");
            Parser.MixedSeeds(seeds);
        }
        
        
        if (e.NamesWithoutLocale.Any(a => a.Name.Equals($"Mods/{Id}/Resources")))
        {
            var clumps = Helper.GameContent.Load<Dictionary<string, ResourceData>>($"Mods/{Id}/Resources");
            Parser.Resources(clumps);
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
        
        if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
        {
            e.Edit(asset =>
            {
                var dictionary = asset.AsDictionary<string, ObjectData>();
                foreach (var (itemId, data) in ModEntry.Ores)
                {
                    if(data.Width > 1 || data.Height > 1)
                        continue;
                    
                    var objectData = new ObjectData
                    {
                        Name = data.Name ?? itemId,
                        DisplayName = "[LocalizedText Strings\\Objects:Stone_Node_Name]",
                        Description = "[LocalizedText Strings\\Objects:Stone_Node_Description]",
                        Type = "Litter",
                        Category = -999,
                        Price = 0,
                        Texture = data.Texture,
                        SpriteIndex = data.SpriteIndex,
                        Edibility = -300,
                        IsDrink = false,
                        Buff = null,
                        GeodeDropsDefaultItems = false,
                        GeodeDrops = null,
                        ArtifactSpotChances = null,
                        ExcludeFromFishingCollection = true,
                        ExcludeFromShippingCollection = true,
                        ExcludeFromRandomSale = true,
                        ContextTags = data.ContextTags,
                        CustomFields = data.CustomFields
                    };
                    if(dictionary.Data.TryAdd(itemId, objectData) == false)
                        dictionary.Data[itemId] = objectData;
                }
            });
        }
        
        if (e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
        {
            e.Edit(asset =>
            {
                var dictionary = asset.AsDictionary<string, ShopData>();
                foreach (var pair in ModEntry.Shops)
                {
                    if(dictionary.Data.TryGetValue(pair.Key, out var shopData) == false)
                        continue;

                    foreach (var data in shopData.Items)
                    {
                        //existing entries are ignored
                        if (data.CustomFields.ContainsKey(ModKeys.ExtraTradesKey))
                            continue;
                        
                        try
                        {
                            //find match
                            var match = pair.Value.First(p => p.Key.Equals(data.Id, StringComparison.Ordinal)).Value;

                            var raw = match.Aggregate("", (current, trade) => current + $"{trade.QualifiedItemId} {trade.Count} ");
                            var items = raw.Remove(raw.Length - 1, 1);

                            data.CustomFields.Add(ModKeys.ExtraTradesKey, items);

                        }
                        catch (Exception _)
                        {
                            continue;
                        }
                    }
                }
            });
        }
        
        //item actions / object behavior
        if (e.NameWithoutLocale.IsEquivalentTo($"Mods/{Id}/Data", true))
        {
            e.LoadFrom(
                () => new Dictionary<string, ItemData>(),
                AssetLoadPriority.Low);
        }
        
        if (e.NameWithoutLocale.IsEquivalentTo($"Mods/{Id}/EatingAnimations", true))
        {
            e.LoadFrom(DefaultContent.GetAnimations, AssetLoadPriority.Low);
        }
        
        //item actions / object behavior
        if (e.NameWithoutLocale.IsEquivalentTo($"Mods/{Id}/MenuActions", true))
        {
            e.LoadFrom(
                () => new Dictionary<string, List<MenuBehavior>>(),
                AssetLoadPriority.Low);
        }
        
        if (e.NameWithoutLocale.IsEquivalentTo($"Mods/{Id}/MixedSeeds", true))
        {
            e.LoadFrom(
                () => new Dictionary<string, List<MixedSeedData>>(),
                AssetLoadPriority.Low);
        }
        
        //resources
        if (e.NameWithoutLocale.IsEquivalentTo($"Mods/{Id}/Resources", true))
        {
            e.LoadFrom(
                () => new Dictionary<string, ResourceData>(),
                AssetLoadPriority.Low);
        }
        
        //shops
        if (e.NameWithoutLocale.IsEquivalentTo($"Mods/{Id}/Shops", true))
        {
            e.LoadFrom(
                () => new Dictionary<string, Dictionary<string, List<ExtraTrade>>>(),
                AssetLoadPriority.Low);
        }

        if (e.NameWithoutLocale.IsEquivalentTo($"Mods/{Id}/Textures/Drink", true))
        {
            e.LoadFromModFile<Texture2D>("assets/Drink.png", AssetLoadPriority.Low);
        }
    }

    internal static void WriteTemplates(object sender, GameLaunchedEventArgs e)
    {
        if(Helper.Data.ReadJsonFile<ItemData>("Templates/Item/Model.json") is not null)
            return;
        
        ModEntry.Mon.Log("Writing file templates to mod folder...", LogLevel.Info);
        Helper.Data.WriteJsonFile("Templates/Item/Model.json", new ItemData());
        Helper.Data.WriteJsonFile("Templates/Item/LightData.json", new LightData());
        Helper.Data.WriteJsonFile("Templates/Item/OnBehavior.json", new OnBehavior());
        Helper.Data.WriteJsonFile("Templates/MixedSeeds.json", new Dictionary<string, List<MixedSeedData>>
        {
            { "ItemId", new(){ new MixedSeedData()}}
        });
        Helper.Data.WriteJsonFile("Templates/Resources/Model.json", new Dictionary<string, ResourceData>
        {
            { "ItemId", new() }
        });
        Helper.Data.WriteJsonFile("Templates/Resources/ExtraSpawn.json", new List<ExtraSpawn>()
        {
            new()
        });
        Helper.Data.WriteJsonFile("Templates/MenuBehavior.json", new Dictionary<string, List<MenuBehavior>>
        {
            { "QualifiedItemId", new(){ new MenuBehavior()}}
        });
        Helper.Data.WriteJsonFile("Templates/EatingAnimation.json", new Dictionary<string,FarmerAnimation>
        {
            {"NameOfAnimation", new() { Animation = new[]{ new FarmerFrame() }, Food = new FoodAnimation()}}
        });
        var extraTradeTemplate = new Dictionary<string, Dictionary<string, List<ExtraTrade>>>
        {
            {
                "ShopID",
                new()
                {
                    {
                        "TradeEntryId", new List<ExtraTrade> { new("ItemToTrade", 1) }
                    }
                }
            }
        };
        Helper.Data.WriteJsonFile("Templates/ExtraTrade.json", extraTradeTemplate);
    }
}