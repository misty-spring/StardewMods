# Extra train drops

Extra train drops are calculated when a train passes by the valley.
Every two seconds, it checks every visible train cart and calculates possible drops.

# Contents

* [Format](#format)
* [Examples](#examples)

--------------------

## Format

Train drops have the same fields as [ExtraSpawns](https://github.com/misty-spring/StardewMods/blob/main/ItemExtensions/docs/ExtraSpawns.md), and a few train-specific options:


| name   | type                  | Required | description                                                                                                                           |
|--------|-----------------------|----------|---------------------------------------------------------------------------------------------------------------------------------------|
| Car    | `string`              | Yes      | Type of car: can be `Plain`, `Resource`, `Passenger` or `Engine`.                                                                     |
| Type\* | `string`              | Yes      | What the train carries: can be `Coal`, `Metal`, `Wood`, `Compartments`, `Grass`, `Hay`, `Bricks`, `Rocks`, `Packages`, or `Presents`. |

*=If Car is `Resource`, set to `None`.

Just like ExtraSpawns, they **also** have the [same fields as any spawnable item](https://stardewvalleywiki.com/Modding:Item_queries#Item_spawn_fields).


## Example

```jsonc
{
  "Action": "EditData",
  "Target": "Mods/mistyspring.ItemExtensions/Train",
  "Entries": {
    "MyCustomDrop": {
        "Car": "Resource",
        "Type": "Hay",
        "ItemId": "(O)SomeItem",
        "Chance": 0.1
    }
  }
}
```
