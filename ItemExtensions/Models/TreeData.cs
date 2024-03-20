using ItemExtensions.Models.Contained;

namespace ItemExtensions.Models;

public class TreeData
{
    public List<ExtraSpawn> OnShake { get; set; }
    public List<ExtraSpawn> OnFall { get; set; }
}