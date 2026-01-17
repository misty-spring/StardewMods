using StardewValley;

namespace MistyCore.Additions.EventCommands;

internal static class Characters
{

    /// <summary>
    /// Resets the given character's name.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="args"></param>
    /// <param name="context"></param>
    public static void ResetName(Event @event, string[] args, EventContext context)
    {
        if (args.Length <= 1)
        {
            context.LogErrorAndSkip("Must state which NPC to reset name for.");
            return;
        }

        var who = args[1];
        var actor = @event.getActorByName(who);
        if (actor == null)
        {
            context.LogErrorAndSkip("no NPC found with name '" + who + "'");
            return;
        }
        try
        {
            var orig = Game1.characterData[actor.Name].DisplayName;
            actor.displayName = orig;
        }
        catch (Exception)
        {
            context.LogErrorAndSkip("Couldn't find character in NPC data.");
            return;
        }
        @event.CurrentCommand++;
    }

}
