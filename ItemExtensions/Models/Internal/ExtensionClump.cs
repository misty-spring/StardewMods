using ItemExtensions.Additions;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Internal;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using static ItemExtensions.Patches.ObjectPatches;
// ReSharper disable PossibleLossOfFraction

namespace ItemExtensions.Models.Internal;

public sealed class ExtensionClump : ResourceClump
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);
    public string ResourceId { get; set; }
    public ResourceData Data { get; set; }
    private LightSource LightSource { get; set; }

    public ExtensionClump(string id, ResourceData data, Vector2 position, int remainingHealth = -1) : base()
    {
        width.Value = data.Width;
        height.Value = data.Height;
        parentSheetIndex.Value = data.SpriteIndex;
        Tile = position;
        textureName.Value = data.Texture;
        health.Value = remainingHealth > 0 ? remainingHealth : data.Health;
        
        Data = data;
        ResourceId = id;
        
        modData.Add(ModKeys.IsCustomClump, "true");
        modData.Add(ModKeys.CustomClumpId, id);
        //modData.Add(ModKeys.LightSize, data.Light.Size);
        //modData.Add(ModKeys.LightColor, data.Light.GetColor());
        
        loadSprite();
    }
    
    public ExtensionClump(string id, Vector2 position, int remainingHealth = -1) : base()
    {
        if (ModEntry.BigClumps.TryGetValue(id, out var data) == false)
        {
            Log("Clump not found.");
            return;
        }
        
        width.Value = data.Width;
        height.Value = data.Height;
        parentSheetIndex.Value = data.SpriteIndex;
        Tile = position;
        textureName.Value = data.Texture;
        health.Value = remainingHealth > 0 ? remainingHealth : data.Health;
        
        Data = data;
        ResourceId = id;
        
        modData.Add(ModKeys.IsCustomClump, "true");
        modData.Add(ModKeys.CustomClumpId, id);
        
        loadSprite();
    }

    public override void OnAddedToLocation(GameLocation location, Vector2 tile)
    {
        base.OnAddedToLocation(location,tile);

        if (Data.Light is null) 
            return;
        
        var fixedPosition = new Vector2(tile.X + width.Value / 2, tile.Y * height.Value / 2);
        LightSource = new LightSource(4, fixedPosition, Data.Light.Size, Data.Light.GetColor());
    }
    
    /// <summary>
    /// Actions to do on tool action or bomb explosion.
    /// </summary>
    /// <param name="t">Tool used</param>
    /// <param name="damage">Damage made</param>
    /// <param name="tileLocation">Location of hit</param>
    /// <returns>If the clump must be destroyed.</returns>
    public override bool performToolAction(Tool t, int damage, Vector2 tileLocation)
    {
        if (ModEntry.BigClumps.TryGetValue(ResourceId, out var resource) == false)
            return false;

        if (!ToolMatches(t, resource))
            return false;

        //set vars
        int parsedDamage;
        
        if (t is MeleeWeapon w)
        {
            var middle = (w.minDamage.Value + w.maxDamage.Value) / 2;
            parsedDamage = middle / 10;
        }
        else
        {
            parsedDamage = damage;
        }

        if (parsedDamage <= 0)
            return false;
        
        //if health data doesn't exist, idk if it can Not exist but just in case
        try
        {
            _ = health.Value;
        }
        catch(Exception)
        {
            health.Set(resource.Health);
        }

        if (damage < resource.MinToolLevel)
        {
            Location.playSound("clubhit", tileLocation);
            Location.playSound("clank", tileLocation);
            Game1.drawObjectDialogue(string.Format(ModEntry.Help.Translation.Get("CantBreak"), t.DisplayName));
            Game1.player.jitterStrength = 1f;
            return false;
        }

        if(!string.IsNullOrWhiteSpace(resource.Sound))
            Location.playSound(resource.Sound, tileLocation);
        
        health.Value -= parsedDamage;

        if (health.Value <= 0.0)
        {
            //create notes
            DoResourceDrop(t, resource);
            RemoveLight();
            
            if(resource.ActualSkill >= 0)
                t.getLastFarmerToUse().gainExperience(resource.ActualSkill, resource.Exp);
            
            if (resource.AddHay > 0)
            {
                AddHay(resource, Location, tileLocation);
            }
            
            return true;
        }
        
        if(Data.Shake)
            shakeTimer = 100;
        return false;
    }
    
    /// <summary>
    /// Drops resources associated to clump.
    /// </summary>
    /// <param name="t"></param>
    /// <param name="resource"></param>
    private void DoResourceDrop(Tool t, ResourceData resource)
    {
        if (Data.SecretNotes && Location.HasUnlockedAreaSecretNotes(t.getLastFarmerToUse()) && Game1.random.NextDouble() < 0.05)
        {
            var unseenSecretNote = Location.tryToCreateUnseenSecretNote(t.getLastFarmerToUse());
            if (unseenSecretNote != null)
                Game1.createItemDebris(unseenSecretNote, Tile * 64f, -1, Location);
        }
            
        var num2 = Game1.random.Next(resource.MinDrops, resource.MaxDrops);
            
        if(!string.IsNullOrWhiteSpace(resource.Debris))
            CreateRadialDebris(Game1.currentLocation, resource.Debris, (int)Tile.X, (int)Tile.Y, Game1.random.Next(1, 6), false);

        if (!string.IsNullOrWhiteSpace(resource.ItemDropped))
        {
            if (Game1.IsMultiplayer)
            {
                Game1.recentMultiplayerRandom = Utility.CreateRandom(Tile.X * 1000.0, Tile.Y);
                for (var index = 0; index < Game1.random.Next(2, 4); ++index)
                    CreateItemDebris(resource.ItemDropped, num2, (int)Tile.X, (int)Tile.Y, Location);
            }
            else
            {
                CreateItemDebris(resource.ItemDropped, num2, (int)Tile.X, (int)Tile.Y, Location);
            }
        }

        var chance = Game1.random.NextDouble();
        if (resource.ExtraItems != null)
        {
            foreach (var item in resource.ExtraItems)
            {
                if(GameStateQuery.CheckConditions(item.Condition, Location, t.getLastFarmerToUse()) == false)
                    continue;
                
                //if it has a condition, check first
                if (chance > item.Chance)
                    continue;

                Log("Chance and condition mstch. Spawning extra item(s)...");
                
                var context = new ItemQueryContext(Location, t.getLastFarmerToUse(), Game1.random);
                var itemQuery = ItemQueryResolver.TryResolve(item, context, item.Filter, item.AvoidRepeat);
                foreach (var result in itemQuery)
                {
                    var parsedItem = ItemRegistry.Create(result.Item.QualifiedItemId, result.Item.Stack, result.Item.Quality);
                    var x = Game1.random.ChooseFrom(new[] { 64f, 0f, -64f });
                    var y = Game1.random.ChooseFrom(new[] { 64f, 0f, -64f });
                    var vector = new Vector2((int)Tile.X, (int)Tile.Y) * 64;
                    Location.debris.Add(new Debris(parsedItem,vector, vector + new Vector2(x, y)));
                }
            }
        }

        if(!string.IsNullOrWhiteSpace(resource.BreakingSound))
            Location.playSound(resource.BreakingSound);
    }
    
    /// <summary>
    /// Removes LightSource from map.
    /// </summary>
    private void RemoveLight()
    {
        if (LightSource is null) 
            return;
        
        var id = LightSource.Identifier;
        Location.removeLightSource(id);
    }
    
    public override bool isPassable(Character c = null)
    {
        if (isTemporarilyInvisible)
            return true;
        
        if (c is null)
            return false;

        if (Data.SolidHeight == Data.Height && Data.SolidWidth == Data.Width)
        {
            return getBoundingBox().Contains(c.Position) == false;
        }
        else
        {
            var extraX = Data.Width - Data.SolidWidth;
            var extraY = Data.Height - Data.SolidHeight;
            var trueX = (int)(Tile.X + extraX);
            var trueY = (int)(Tile.Y + extraY);
            
            var source = new Rectangle(trueX * 64, trueY * 64, Data.SolidWidth * 64, Data.SolidHeight * 64);
            return source.Contains(c.Position) == false;
        }
    }
}