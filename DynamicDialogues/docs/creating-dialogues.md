## Contents
* [Adding dialogues](#adding-dialogues)
  * [Conditions](#conditions) 
  * [Template](#template)
* [Examples](#examples) 
  * [From-To (time condition)](#using-from-to)
  * [Remove dialogue if NPC leaves](#using-clearonmove)
  * [Overriding dialogue](#using-override)
  * [Adding an animation](#using-animation)


## Adding dialogues

To add dialogues, edit `mistyspring.dynamicdialogues/Dialogues/<namehere>`. 
Each dialogue has a unique key to ensure multiple patches can exist.

| name          | required | description                                                                                                                                                         |
|---------------|----------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Time          | (\*)     | Time to set dialogue at.                                                                                                                                            |
| From          | (\*\*)   | Min. time to apply dialogue at.                                                                                                                                     |
| To            | (\*\*)   | Max time to apply dialogue at                                                                                                                                       |
| Location      | (\*)     | Name of the map the NPC has to be in.                                                                                                                               |
| Dialogue      | yes      | The text to display.                                                                                                                                                |
| ClearOnMove   | No       | If `true` and dialogue isn't read, it'll disappear once the NPC moves.                                                                                              |
| Override      | No       | Removes any previous dialogue.                                                                                                                                      |
| Force         | No       | Will show this NPC's dialogue even if you're not in the location.                                                                                                   |
| IsBubble      | No       | `true`/`false`. Makes the dialogue a bubble over their head.                                                                                                        |
| Jump          | No       | If `true`, NPC will jump.                                                                                                                                           |
| Shake         | No       | Shake for the milliseconds stated (e.g Shake 1000 for 1 second).                                                                                                    |
| Emote         | No       | Will display the emote at that index ([see list of emotes](https://docs.google.com/spreadsheets/d/18AtLClQPuC96rJOC-A4Kb1ZkuqtTmCRFAKn9JJiFiYE/edit#gid=693962458)) |
| FaceDirection | No       | Changes NPC's facing direction. allowed values: `0` to `3` or `up`,`down`,`left`,`right`.                                                                           |
| Animation     | No       | Animates the character briefly.                                                                                                                                     |
| Conditions    | No       | Conditions for the dialogue to trigger.                                                                                                                             |
| TriggerAction | No       | ID of the trigger action to call.                                                                                                                                   |

`*`= You need either a time or location (or both) for the dialogue to load.

`**` = Mutually exclusive with "Time" field. Use this if you need a dialogue to show up *only* when the player is present.

### Conditions

Conditions are of the type `PlayerConditions`, all values are optional.                  |

For an example, see [here](#using-player-conditions).

### Template
```
"nameForPatch": {
          "Time": ,
          "From": ,
          "To": ,
          "Location": ,
          "Dialogue": ,
          "Override": ,
          "Force": ,
          "ClearOnMove": ,
          "IsBubble": ,
          "Emote": ,
          "Shake": ,
          "Jump": ,
          "Conditions": ,
          "TriggerAction": ,
        },
```

Just remove any fields you won't be using.

**Note:** If you don't want the dialogue to appear every day, use ContentPatcher's "When" field.

------------

## Examples

### Using From-To

From-To will only apply the changes when the player is present, and when the time fits the given range.
The time can be anywhere between 610 and 2550. 

_"Why not earlier/later?"_: The mod adds dialogue when time changes. 
- When a day starts (6 am), no time change has occurred yet. 
- At 2600 the day ends, so you wouldn't get to see the dialogues (most they'd do is load, and immediately get discarded by the game).


**Example**

Let's say you want Willy to jump and say "Aye!" *only* between 610 - 8am, when at the beach. The patch would look like this:

```
"fishEscaped": {

          "From": 610,
          "To": 800,
          "Location": "Beach",
          "Dialogue": "Aye!",
          "IsBubble": true,
          "Jump": true,
        },
```

So, if the player enters the beach (between the specified time), Willy will do that.

------------


### Using ClearOnMove
This option is specific to "box" dialogues (ones you have to click to see). If used with `"IsBubble": true`, it won't do anything.
Basically, it will remove a dialogue if the NPC moves. This is useful if you need the dialogue to disappear once the npc changes locations / to avoid "out of context" messages.

**Example:**
This makes Leah say something at Pierre's. If she starts walking (e.g to exit), the dialogue will be removed.
```
"pricesWentUp": {

          "Location": "SeedShop",
          "Dialogue": "Hi, @. Buying groceries too?#$b#...Did the prices go up?$2",
          "ClearOnMove": true,
        },
```
------------

### Using Override
This option is for "box" dialogues (ones you have to click to see). If used with `"IsBubble": true`, it won't do anything.
It will force the dialogue to be added- regardless of the current dialogue. Useful if you want the NPC to have a dialogue mid schedule <u>animation</u>.

**Note:** This will remove any previous dialogue, so use with caution.

**Example:**
If you want Emily to say something when she's working at Gus', you'll need to use Override. (Otherwise, the dialogue will get "buried" under the schedule one).
```
"SaloonTime": {
          "Location": "Saloon",
          "Dialogue": "Did you come buy something?",
          "Override": true,
        },
```
------------

### Using Animation

"Animation" will animate the character once.
This will work as long as the character isn't moving already. 
(e.g: if you try to make Harvey animate during aerobics, it won't work- because he's already "moving". Similarly, if a character is walking somewhere the animation won't be applied (since this would mess up the entire sprite).

`*` You must set "Enabled" to `true` inside animations. This can have any animation, even vanilla ones.

`**` You must use a valid frame- if you choose a frame that doesn't exist, it will cause errors (this is part of in-game errors / something i can't do anything about.) 
- Explanation: Since some mods add *extra* animations to sprites, the framework has no way of knowing what the max frame is.

| name     | description                                        |
|----------|----------------------------------------------------|
| Enabled  | Whether to enable animations.                      |
| Frames   | The frames for the animation, separated by spaces. |
| Interval | Milliseconds to show every frame for.              |

Frames start at 0, from the top left (and continue to the right, then the next row. Located in the game's `Content/Characters` folder).
If you need help, see [here](https://stardewvalleywiki.com/Modding:NPC_data#Overworld_sprites).


**Example:** 
When at the beach, Alex will momentarily play with the grid-ball. Each frame will play for 150 ms.

```
"gridBall": {
          "Location": "Beach",
          "Dialogue": "This is fun.",
          "IsBubble": true,
          "Animation": 
          {
            "Enabled": true,
            "Frames": "16 17 18 19 20 21 22 22 22 22 16 17 18 19 20 21 22 23 23 23",
            "Interval": 150,
          }
}
```
