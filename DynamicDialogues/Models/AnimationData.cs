// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace DynamicDialogues.Models;

///<summary>Class which holds animation information (if used for dialogues).</summary>
public class AnimationData
{
    public bool Enabled { get; set; }
    public string Frames { get; set; }
    public int Interval { get; set; } // milliseconds for each frame

    public AnimationData()
    {
    }
}