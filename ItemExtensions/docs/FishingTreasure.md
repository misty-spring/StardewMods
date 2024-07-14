# Extra fishing treasure

Extra fishing treasure can be customized by patching `Mods/mistyspring.ItemExtensions/Treasure`

# Contents

* [Format](#format)
* [Examples](#examples)

--------------------

## Format

Custom treasures have the same fields as [ExtraSpawns](https://github.com/misty-spring/StardewMods/blob/main/ItemExtensions/docs/ExtraSpawns.md).

Just like ExtraSpawns, they **also** have the [same fields as any spawnable item](https://stardewvalleywiki.com/Modding:Item_queries#Item_spawn_fields).

In addition, it has these fields:

| name             | type       | Required | description                              |
|------------------|------------|----------|------------------------------------------|
| Rod              | `string[]` | No       | Allowed rod IDs.                         |
| Bait             | `string[]` | No       | Allowed bait types.                      |
| Tackle           | `string[]` | No       | Allowed tackles.                         |
| RequireAllTackle | `bool`     | No       | If all tackles must be in use.           |
| Bobber           | `int`      | No       | Bobber type.                             |
| MinAttachments   | `int`      | No       | Minimum attachments this tool must have. |
| MaxAttachments   | `int`      | No       | Maximum attachments this tool must have. |


## Example

Here, we add a new treasure drop.
Every time a treasure box is opened, there'll be a 5% chance for `(O)SomeItem` to be added to the loot.

```jsonc
{
  "Action": "EditData",
  "Target": "Mods/mistyspring.ItemExtensions/Treasure",
  "Entries": {
    "MyCustomLoot": {
        "ItemId": "(O)SomeItem",
        "Chance": 0.05,
        "MinStack": 2,
        "MaxStack": 4
    }
  }
}
```

In another example, we'll use rod conditions:

```jsonc
{
  "Action": "EditData",
  "Target": "Mods/mistyspring.ItemExtensions/Treasure",
  "Entries": {
    "AnotherLoot": {
        "ItemId": "(O)TheItem",
        "Chance": 0.01,
        "Rod": [ "FiberglassRod", "IridiumRod" ], //can spawn when using *any* of these rods
        "Bobber": 0 //and the default bobber
    }
  }
}
```

Here, TheItem will have a 1% chance of spawning. Aditionally, it will only spawn when using the default bobber and a fiberglass or iridium rod.
