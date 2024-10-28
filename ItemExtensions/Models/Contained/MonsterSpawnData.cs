using ItemExtensions.Models.Items;
using Microsoft.Xna.Framework;

namespace ItemExtensions.Models.Contained;

public class MonsterSpawnData
{
    public string Name { get; set; } = null;
    public int Health { get; set; } = -1;
    public bool Hardmode { get; set; }
    public Vector2 Distance { get; set; } = new();
    public bool ExcludeOriginalDrops { get; set; }
    public List<ExtraSpawn> ExtraDrops { get; set; } = new();
    public Color? Color { get; set; }
    public bool FollowPlayer { get; set; } = true;
    public bool HideShadow { get; set; }
    public bool RangedAttacks { get; set; } = true;
    public int GracePeriod { get; set; }
}