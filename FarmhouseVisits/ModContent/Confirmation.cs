using StardewValley;
using static FarmhouseVisits.ModEntry;

namespace FarmhouseVisits.ModContent;

public static class Confirmation
{
    //if user wants confirmation for NPC to come in
    internal static void AskToEnter()
    {
        Actions.CheckSafeShutdown();
        
        //knock on door
        DelayedAction.playSoundAfterDelay("stoneStep",300,Game1.player.currentLocation);
        DelayedAction.playSoundAfterDelay("stoneStep",600,Game1.player.currentLocation);
        DelayedAction.playSoundAfterDelay("stoneStep",900,Game1.player.currentLocation);

        //get name, place in question string
        var displayName = Visitor.displayName;
        var formattedQuestion = string.Format(TL.Get("Visit.AllowOrNot"), displayName);

        var res = Game1.player.currentLocation.createYesNoResponses();
        Game1.player.currentLocation.createQuestionDialogue(formattedQuestion, res, EntryAfterQuestion);
    }
    private static void CancelVisit()
    {
        var name = Visitor.Name;
        TodaysVisitors.Add(name);
        if (Config.RejectionDialogue)
        {
            Actions.DrawDialogue(Visitor, Values.GetDialogueType(Visitor,DialogueType.Rejected));
        }

        SetNoVisitor();
    }
    private static void Proceed()
    {
        if (VContext.CustomVisiting)
        {
            Actions.AddCustom(Visitor, PlayerHome, VContext.CustomData, true);
        }
        else
        {
            Actions.AddToFarmHouse(Visitor, PlayerHome, true);
        }
    }
    private static void EntryAfterQuestion(Farmer who, string whichAnswer)
    {
        if (whichAnswer == "Yes") 
            Proceed();
        else 
            CancelVisit();
    }

}