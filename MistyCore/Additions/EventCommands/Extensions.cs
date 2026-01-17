using StardewValley;
using StardewValley.GameData.FarmAnimals;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace MistyCore.Additions.EventCommands;

internal static class Extensions
{
    /// <summary>
    /// Add if/else conditions to event-making. 
    /// Format: if \"QUERY\"#action#alternative
    /// </summary>
    /// <param name="event">Event</param>
    /// <param name="args">Parameters to use.</param>
    /// <param name="context">Event context.</param>
    public static void IfElse(Event @event, string[] args, EventContext context)
    {
        var fullArg = "";
        foreach (var part in args)
        {
            fullArg += part;
            if (args[^1] != part)
                fullArg += " ";
        }

        var rawArgs = fullArg.Replace("if ", "").Split("##");
        var condition = rawArgs[0];

        if (GameStateQuery.CheckConditions(condition))
        {
            //make rawArgs[1] the next command
            @event.InsertNextCommand(rawArgs[1]);
        }
        else if (rawArgs.Length >= 2)
        {
            //same as above, but for rawArgs[2]
            @event.InsertNextCommand(rawArgs[2]);
        }
        @event.CurrentCommand++;
    }
    
    /// <summary>
    /// Append another event to the current one (like forks, but less hard to understand).
    /// </summary>
    /// <param name="event">Event</param>
    /// <param name="args">Parameters to use.</param>
    /// <param name="context">Event context.</param>
    public static void Append(Event @event, string[] args, EventContext context)
    {
        if (args.Length < 2)
        {
            @event.LogCommandErrorAndSkip(args, "append must state an event string (e.g 'append myEvent')");
            return;
        }

        //get event to append
        var subEvent = args[1];

        // get specific string. used to get it with @event.exitLocation.Name but that might cause bugs
        var path = $"Data/Events/{Game1.currentLocation.Name}:{subEvent}";

        var events = Game1.content.LoadString(path);

        if (events == path)
        {
            @event.LogCommandErrorAndSkip(args, "Found no event with that key. Skipping...");
            return;
        }

        var commandParsed = events.Split('/');//.split('\\');

        //if theres a single command, append
        if (commandParsed.Length == 1)
        {
            @event.InsertNextCommand(commandParsed[0]);
            @event.CurrentCommand++;
            return;
        }

        // based off Event's InsertNextCommand
        var eventCommands = ModEntry.Help.Reflection.GetField<string[]>(@event, "eventCommands");
        var index = ModEntry.Help.Reflection.GetField<int>(@event, "currentCommand").GetValue();
        var commands = eventCommands.GetValue().ToList();

        foreach (var subcommand in commandParsed)
        {
            index++;
            if (index <= commands.Count)
            {
                commands.Insert(index, subcommand);
            }
            else
            {
                commands.Add(subcommand);
            }
        }
        eventCommands.SetValue(commands.ToArray());
        @event.CurrentCommand++;
    }

    public static void Foreach(Event @event, string[] args, EventContext context)
    {
        /* element options: npc, animal, crop, object, monster
         * example: foreach animal in farm ## pet
         * what you can do varies by element
         */
        
        if (args.Length < 6)
        {
            context.LogErrorAndSkip("Command doesn't have enough arguments.");
            return;
        }
        
        var element = args[1];
        var locationName = args[3];
        var location = Game1.getLocationFromName(locationName);
        if (location == null)
        {
            ModEntry.Log($"Location {locationName} seems to not exist. Defaulting to current location...");
            location = context.Location;
        }
        
        var fullArg = "";
        foreach (var part in args)
        {
            fullArg += part;
            if (args[^1] != part)
                fullArg += " ";
        }

        var action = fullArg.Replace(" ## ","##").Split("##", StringSplitOptions.RemoveEmptyEntries)[1];
        var actionSplit = action.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var pureAction = actionSplit[0];
        var value = actionSplit.Length > 1 ? actionSplit[1] : "null";

        switch (element.ToLower())
        {
            case "npc":
                ForEachNpc(pureAction, value, location, @event.farmer);
                break;
            case "animal":
                ForEachAnimal(pureAction, value, location, @event.farmer);
                break;
            case "crop":
                ForEachCrop(pureAction, value, location);
                break;
            case "object":
                ForEachObject(pureAction, value, location, @event.farmer);
                break;
            case "monster":
                ForEachMonster(pureAction, value, location, @event.farmer);
                break;
        }
        @event.currentCommand++;
    }

    private static void ForEachNpc(string action, string value, GameLocation location, Farmer who)
    {
        foreach (var character in location.characters)
        {
            if (character.IsMonster)
                continue;

            switch (action)
            {
                case "invisible":
                    character.IsInvisible = bool.Parse(value);
                    break;
                case "shake":
                    character.shake(int.Parse(value));
                    break;
                case "sayHiTo":
                    var otherNpc = Utility.fuzzyCharacterSearch(value);
                    if (otherNpc is not null)
                        character.sayHiTo(otherNpc);
                    break;
                case "jump":
                    character.jump();
                    break;
                case "friendship":
                case "changeFriendship":
                    who.changeFriendship(int.Parse(value), character);
                    break;
            }
        }
    }

    private static void ForEachAnimal(string action, string value, GameLocation location, Farmer who)
    {
        foreach (var animal in location.Animals.Values)
        {
            switch (action)
            {
                case "pet":
                    animal.pet(who, bool.Parse(value));
                    break;
                case "growFully":
                    animal.growFully();
                    break;
                case "makeSound":
                    animal.makeSound();
                    break;
                case "digUpProduce":
                    var produceId = animal.GetProduceID(Game1.random, bool.Parse(value));
                    if (string.IsNullOrWhiteSpace(produceId) == false && animal.GetAnimalData().HarvestType == FarmAnimalHarvestType.DigUp)
                        animal.DigUpProduce(location, new Object(produceId, 1));
                    break;
                case "stop":
                case "halt":
                    animal.Halt();
                    break;
            }
        }
    }

    private static void ForEachCrop(string action, string value, GameLocation location)
    {
        foreach (var terrainFeature in location.terrainFeatures.Values)
        {
            if (terrainFeature is not HoeDirt hoeDirt)
                continue;
            
            if (hoeDirt.crop is null)
                continue;

            switch (action)
            {
                case "growFully":
                    hoeDirt.crop.growCompletely();
                    break;
                case "giantCrop":
                    hoeDirt.crop.TryGrowGiantCrop(bool.Parse(value));
                    break;
                case "rot":
                case "kill":
                    hoeDirt.crop.Kill();
                    break;
                case "harvest":
                    hoeDirt.crop.harvest((int)hoeDirt.Tile.X, (int)hoeDirt.Tile.Y, hoeDirt);
                    break;
            }
        }
    }

    private static void ForEachObject(string action, string value, GameLocation location, Farmer who)
    {
        foreach (var obj in location.Objects.Values)
        {
            switch (action)
            {
                case "setFlag":
                    obj.SetFlagOnPickup = value;
                    break;
                case "removeFlag":
                    obj.SetFlagOnPickup = null;
                    break;
                case "setQuality":
                    obj.Quality = int.Parse(value);
                    break;
                case "removeQuality":
                    obj.Quality = 0;
                    break;
            }
        }
    }

    private static void ForEachMonster(string action, string value, GameLocation location, Farmer who)
    {
        foreach (var character in location.characters)
        {
            if (character.IsMonster == false)
                continue;

            if (character is not Monster monster)
                break;
            
            switch (action)
            {
                case "damage":
                    monster.takeDamage(int.Parse(value), monster.TilePoint.X, monster.TilePoint.Y, false, 0, who);
                    break;
                case "kill":
                    monster.takeDamage(monster.Health + 1, monster.TilePoint.X, monster.TilePoint.Y, false, 0, who);
                    break;
            }
            
        }
    }
}
