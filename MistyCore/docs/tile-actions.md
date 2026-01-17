# Tile actions

## Contents
* [AddItem](#additem)
* [Question](#question)
* [ConditionalWarp](#conditional-warp)

<hr>

## AddItem

This works similarly to when you obtain the golden scythe. It consists of a `Dictionary<string, ItemAdditionData>` where the string is the key to search for, and the value has the following fields:

| field                          | type                   | required | description                                                       |
|--------------------------------|------------------------|----------|-------------------------------------------------------------------|
| FlagToSet                      | `string`               | Yes      | A flag to set on obtaining the item.                              |
| Items                          | `List<AddedItemData>`  | Yes      | The items to add.                                                 |
| Condition                      | `string`               | No       | A GameStateQuery for the tile action to trigger.                  |
| PlaySound                      | `string`               | No       | A sound to play.                                                  |
| TileChanges                    | `List<TileChangeData>` | No       | The tiles to change in the map. (Non-permanent)                   |
| TileRemovals                   | `List<TileChangeData>` | No       | The tiles to remove in the map. (Non-permanent)                   |
| TriggerActions                 | `List<string>`         | No       | Trigger actions to call.                                          |
| MessageToShowIfConditionsFail  | `string`               | No       | Message to show if the GSQ fails.                                 |
| IsLetter                       | `bool`                 | No       | Whether the above message should be a letter.                     |
| MessageToShowIfAlreadyReceived | `string`               | No       | The message to show if the item was already received.             |
| IsLetterReceived               | `bool`                 | No       | Whether the "received" message should be a letter.                |

### AddedItemData fields

| field           | type     | required | description                             |
|-----------------|----------|----------|-----------------------------------------|
| QualifiedItemId | `string` | Yes      | The qualified item id.                  |
| Stack           | `int`    | No       | The amount for the item. Defaults to 1. |
| IsRecipe        | `bool`   | No       | Whether the object is a recipe.         |
| Quality         | `int`    | No       | The item's quality.                     |

### TileChangeData fields

| field       | type     | description                                                               |
|-------------|----------|---------------------------------------------------------------------------|
| Layer       | `string` | The layer. Defaults to "Back".                                            |
| X           | `int`    | The X coordinate in the map.                                              |
| Y           | `int`    | The Y coordinate in the map.                                              |
| Index       | `int`    | The ID of the tile to change (can be seen in Tiled's tilesheet settings). |
| TileSheetId | `string` | The ID of the tilesheet.                                                  |

### Example

```json
{
  "Action": "EditData",
  "Target": "Mods/mistyspring.mistycore/TileActions/AddItem",
  "Entries": {
    "{{ModId}}_MyCustomAddition": {
      "Items": [
        {
          "QualifiedItemId":"(O)69",
          "Stack": 10,
          "Quality": 1
        },
        {
          "QualifiedItemId":"(O)420", //only the last item you add will be shown as held up
          "Stack": 1,
          "Quality": 1
        }
      ],
      "FlagToSet": "{{ModId}}_MyFlag", 
      "Condition": "PLAYER_HAS_SEEN_EVENT Current MyCustomEvent",
      "PlaySound": "parry",
      "TileChanges":[
        {
          "Layer": "Buildings",
          "X": 10,
          "Y": 10,
          "Index": 25,
          "TileSheetId": "z_my_custom_tilesheet"
        },
        {
          "Layer": "Front",
          "X": 10,
          "Y": 9,
          "Index": 24,
          "TileSheetId": "z_my_custom_tilesheet"
        }
      ],
      "TileRemovals": [
        {
          "Layer": "Buildings",
          "X": 11,
          "Y": 10
        }
      ],
      "TriggerActions": [
        "AddMoney 5000"
      ],
      "MessageToShowIfConditionsFail": "You can't do that.",
      "IsLetter": false,
      "MessageToShowIfAlreadyReceived": "There's nothing here.",
      "IsLetterReceived": false
    }
  }
}
```

## Question

A question dialogue with possible condition(s), which leads to trigger action(s) if yes is selected.

| field                         | type           | optional | description                                                       |
|-------------------------------|----------------|----------|-------------------------------------------------------------------|
| Question                      | `string`       | No       | An optional question dialogue.                                    |
| Yes                           | `string`       | Yes      | The text to show for the "Yes" option. Defaults to localized Yes. |
| No                            | `string`       | Yes      | The text to show for the "No" option. Defaults to localized No.   |
| Condition                     | `string`       | Yes      | A Game State Query.                                               |
| MessageToShowIfConditionsFail | `string`       | Yes      | The message to show if conditions fail.                           |
| IsLetter                      | `bool`         | Yes      | Whether to show the MessageToShowIfConditionsFail as a letter.    |
| TriggerActions                | `List<string>` | Yes      | The trigger actions to call.                                      |

### Example
```json

{
  "Action": "EditData",
  "Target": "Mods/mistyspring.mistycore/TileActions/Question",
  "Entries": {
    "{{ModId}}_MyCustomQuestion": {
      "Condition": "PLAYER_CURRENT_MONEY Current 5000",
      "Question": "Hey...I've got some fairy dust for sale. Are you interested?",
      "Yes": "Buy",
      "No": "No thanks",
      "MessageToShowIfConditionsFail": "Come back when you have more money.",
      "TriggerActions": [
        "AddItem (O)872 1",
        "AddMoney -5000"
      ]
    }
  }
}
```

## ConditionalWarp

The format is `command dataName location x y`, and uses almost the same fields as a Question:

| field                         | type     | optional | description                                                       |
|-------------------------------|----------|----------|-------------------------------------------------------------------|
| Question                      | `string` | Yes\*    | An optional question dialogue.                                    |
| Yes                           | `string` | Yes      | The text to show for the "Yes" option. Defaults to localized Yes. |
| No                            | `string` | Yes      | The text to show for the "No" option. Defaults to localized No.   |
| Condition                     | `string` | Yes\*    | A Game State Query.                                               |
| MessageToShowIfConditionsFail | `string` | Yes      | The message to show if conditions fail.                           |
| IsLetter                      | `bool`   | Yes      | Whether to show the MessageToShowIfConditionsFail as a letter.    |

\*= Either a condition or a question must be set.

You use it by setting a tile Action (in the Buildings layer) to `Action`: `mistyspring.mistycore_ConditionalWarp myCustomWarp MyCustomLocation x y`

### Example

```json
{
  "Action": "EditData",
  "Target": "Mods/mistyspring.mistycore/TileActions/ConditionalWarp",
  "Entries": {
    "myCustomWarp": {
      "Question": "Would you like to warp?",
      "Condition": "PLAYER_HAS_SEEN_EVENT Current myCustomEvent",
      "MessageToShowIfConditionsFail": "Dear @,^^I've set up a warp here. But before you can use it, you must first defeat me in battle. Meet me at SomeCustomLocation.^    - CustomNPCName",
      "IsLetter": true
    }
  }
}
```