namespace ItemExtensions.Models.Contained;

public class NoteData
{
    //mutually exclusive w/ everything else
    public string MailId { get; } = null;
    public string Image { get; } = null;
    public string LetterTexture { get; } = "0";
    public string Message { get; } = null;
    public string ImagePosition { get; } = "down";
}