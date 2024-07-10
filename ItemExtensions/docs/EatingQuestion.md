# Eating question

You can customize the dialogue that confirms eating/drinking.

# Contents
* [Format](#format)
* [Example](#example)

--------------------

### Format

For this, you must edit Data's `FoodDialogue`:

| name            | type     | required | description                                   |
|-----------------|----------|----------|-----------------------------------------------|
| ConsumeQuestion | `string` | No       | Question prompt when eating.                  |
| CannotConsume   | `string` | No       | Dialogue shown if you can't consume the item. |
| Yes             | `string` | No       | Text for confirming.                          |
| No              | `string` | No       | Text for refusing.                            |

Any value that you don't specify will default to the original (e.g, If you don't specify Yes/No, the dialogue will show regular Yes/No responses.)

*= You can customize Yes and leave No as default, vice versa, or edit both/none.

**= If `ConsumeQuestion` has a `{0}`, it'll be replaced by the item name. (e.g: if you edit carrot's question, "Eat the {0}?" will display as "Eat the carrot?")

### Example

Here is an example of a custom animation.

```jsonc
{
  "Action": "EditData",
  "Target": "Mods/mistyspring.ItemExtensions/Data",
  "Entries": {
    "(O)608": {
      "FoodDialogue": {
	  "ConsumeQuestion":"Eat this thing? ({0})",
	  "CannotConsume":"You can't eat it",
	  "No":"Nah" 
      }
    }
  }
}
```

In-game, it will look like this:

![Image of a videogame dialogue. The character holds up a pumpkin pie, with a question that reads "Eat this thing? Pumpkin pie", and the answers "Yes" or "Nah".](https://mistyspring.neocities.org/Files/Other/github_stardewmods_itemextensions_eatingquestion.png)

And if it fails, the game will show "You can't eat it".
