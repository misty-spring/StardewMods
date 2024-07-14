namespace DynamicDialogues;

public interface IApi
{
    bool TryGetDialogueFor(string who, string location, int startTime, out string text, int endTime = -1);
    bool TryGetCustomGreeting(string who, string npcToGreet, out string text);
}

public class Api : IApi
{
    public bool TryGetDialogueFor(string who, string location, int startTime, out string text, int endTime = -1)
    {
        text = string.Empty;
        
        if (ModEntry.Dialogues.TryGetValue(who, out var data) == false)
            return false;

        foreach (var entry in data)
        {
            if(entry.Location.Equals(location, StringComparison.OrdinalIgnoreCase) == false)
                continue;
            
            //if no from/to match OR if time doesn't match start
            if((entry.From != startTime || entry.To != endTime) && entry.Time != startTime)
                continue;

            text = entry.Dialogue;
            return true;
        }
        return false;
    }

    public bool TryGetCustomGreeting(string who, string npcToGreet, out string text)
    {
        text = null;
        
        if (ModEntry.Greetings.TryGetValue((who, npcToGreet), out var greeting) == false)
            return false;
        
        text = greeting;
        return true;
    }
}