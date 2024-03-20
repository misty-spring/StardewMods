using Microsoft.Xna.Framework;
using StardewValley;

namespace ItemExtensions.Models;

public class LightData
{
    public int R { get; set; } = 1;
    public int G { get; set; } = 1;
    public int B { get; set; } = 1;
    public float Transparency { get; set; } = 0.9f;
    public string Hex { get; set; } = null;
    public float Size { get; set; } = 1.2f;

    public Color GetColor()
    {
        if (!string.IsNullOrWhiteSpace(Hex))
        {
            var color = Utility.StringToColor(Hex) ?? Color.White;
            return color * Transparency;
        }
        
        return new Color(R, G, B) * Transparency;
    }
}