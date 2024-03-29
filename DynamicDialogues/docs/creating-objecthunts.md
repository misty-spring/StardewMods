# Object hunts
`objectHunt <key>`

Lets you create object hunts, akin to egg hunt and haley's event.


## Contents

* [Format](#format)
    * [AfterSequenceBehavior](#aftersequencebehavior)
* [Example](#example)
    * [Template](#template)
* [Using the command](#using-the-command)



----------


## Format

Object hunts use a custom model:

| name         | type                    | Required | description                                  |
|--------------|-------------------------|----------|----------------------------------------------|
| Timer        | `int`                   | false    | A time limit for the hunt.                   |
| OnFailure    | `AfterSequenceBehavior` | false    | Behavior if player fails (requires `Timer`). |
| OnSuccess    | `AfterSequenceBehavior` | false    | Behavior when player completes the hunt.     |
| CanPlayerRun | `bool`                  | false    | Whether the player can run. Default true     |
| Objects      | `List<ObjectData>`      | true     | The list of objects to use in the hunt.      |

### AfterSequenceBehavior

The parameters for OnFailure/OnSuccess are the same:

| name          | type     | Required | description                       |
|---------------|----------|----------|-----------------------------------|
| Mail          | `string` | false    | Mail to send.                     |
| ImmediateMail | `bool`   | false    | If the mail should be sent today. |
| Flag          | `string` | false    | Flag to set.                      |
| Energy        | `int`    | false    | Stamina to add/reduce.            |
| Health        | `int`    | false    | Health to add/reduce.             |

### Example
To add your own object hunt, patch `mistyspring.dynamicdialogues/Commands/objectHunt`.

Here, we create the data for object hunt "simple_test".
```json
{
  "Action": "EditData",
  "Target": "mistyspring.dynamicdialogues/Commands/objectHunt",
  "Entries": {
    "simple_test": {
      "CanPlayerRun": true,
      "OnSuccess": {
        "Energy": "-50"
      },
      "Objects": [
        {
          "ItemId": "(O)541",
          "X": 20,
          "Y": 6
        },
        {
          "ItemId": "(O)543",
          "X": 21,
          "Y": 6
        }
      ]
    }
  }
}
```

Then, we call it in an event with `objectHunt simple_test`

Items 541 and 543 will be placed in the map, and you'll lose 50 energy upon completing the search.

### Template

```jsonc
"your_hunt_key_here": {
    "Timer": 0,
    "CanPlayerRun": true,
    "OnSuccess": {
        "Mail":"",
        "ImmediateMail":"",
        "Flag":"",
        "Energy": "",
        "Health":""
    },
    "OnFailure": {
        "Mail":"",
        "ImmediateMail":"",
        "Flag":"",
        "Energy": "",
        "Health":""
    },
    "Objects": [
        {
            "ItemId": "(O)",
            "X": "",
            "Y": ""
        },
        {
            "ItemId": "(O)",
            "X": "",
            "Y": ""
        },
        {
            "ItemId": "(O)",
            "X": "",
            "Y": ""
        }
    ]
}
```

### Using the command

In your event, just add `objectHunt <your addition's key>` and it'll begin.