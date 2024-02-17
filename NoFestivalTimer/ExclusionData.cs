namespace NoFestivalTimer;

public class ExclusionData
{
    public bool IgnoreTimer { get; set; }
    public int OnScore { get; set; }
    public bool Props { get; set; }

    public ExclusionData()
    {
    }

    public ExclusionData(bool ignoreTimer, int minScore, bool useProps)
    {
        IgnoreTimer = ignoreTimer;
        OnScore = minScore;
        Props = useProps;
    }
}