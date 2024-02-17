namespace DynamicDialogues.Models;

///<summary>Class which holds animation information (if used for dialogues).</summary>
internal class AnimationData
{
    public bool Enabled { get; set; }
    public string Frames { get; set; }
    public int Interval { get; set; } // milliseconds for each frame

    public AnimationData()
    {
    }

    public AnimationData(AnimationData a)
    {
        Enabled = a.Enabled;
        Frames = a.Frames;
        Interval = a.Interval;
    }
}