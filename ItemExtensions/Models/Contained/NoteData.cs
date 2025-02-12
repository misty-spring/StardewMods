namespace ItemExtensions.Models.Contained;

public class NoteData
{
    //mutually exclusive w/ everything else
    public string MailId { get; set; } = null;
    public string Image { get; set; } = null;
    public string LetterTexture { get; set; } = "0";
    public string Message { get; set; } = "";
    public string ImagePosition { get; set; } = "down";
    public string Condition { get; set; } = "TRUE";
    public List<string> AddFlags { get; set; } = new();
}