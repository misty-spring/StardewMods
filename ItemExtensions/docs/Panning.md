# Custom panning drops

The mod allows to add more "results" to panning.

## Contents

* [How to add](#how-to-add)
    * [Format](#format)
    * [Example](#example-1)

---

## How to add

You can add panning items by editing `mistyspring.ItemExtensions/Panning` . This uses the [ExtraSpawn]() model, in addition you can state a `MinUpgrade` (or `MaxUpgrade`). (For example, MinUpgrade 0 is the regular copper pan.)

### Format

PanningData has these fields:

| name        | type                  | Required | description                                  |
|-------------|-----------------------|----------|----------------------------------------------|
| ItemId      | `string`              | Yes      | Qualified item Id.                           |
| Chance      | `double`              | Yes      | Spawn chance: e.g, 0.5 for 50%.              |
| MinUpgrade  | `int`                 | No       | Minimum upgrade level. Default 0 (Copper)    |
| MaxUpgrade  | `int`                 | No       | Maximum upgrade level. Default -1 (No limit) |
| Condition   | `string`              | No       | Game State Query.                            |
| AvoidRepeat | `bool`                | No       | Advanced, for `ISpawnItemData`.              |
| Filter      | `ItemQuerySearchMode` | No       | Advanced, for `ISpawnItemData`.              |

Asides from this, they **also** have the [same fields as any spawnable item](https://stardewvalleywiki.com/Modding:Item_queries#Item_spawn_fields).

### Example

In this example, we add a new panning drop.

This will add MyCustomItem as a panning drop, with a 0.05% chance of being added.

```jsonc
{
  "Action": "EditData",
  "Target": "Data/Objects",
  "Entries": {
    "MyCustomSeeds": {
      //(...seed data)
      "CustomFields": {
        "mistyspring.ItemExtensions/MixedSeeds": "745 486 481"
      }
    }
  }
}
```

As a result, whenever you plant MyCustomSeeds, there's a 25% chance you'll get a strawberry seed (likewise for parsnip and blueberry).

This is because the mod takes all "possible" seeds:
```txt
MyCustomSeeds 745 472 481
```
And chooses one randomly.

## Via Mod

To add mixed seeds via the mod, edit `Mods/mistyspring.ItemExtensions/MixedSeeds` . The key must be the seed Id, and the value is a `MixedSeedData` list with all possible seeds.

### Format

`MixedSeedData` follows the following format:

| name      | type              | Required | description                                        |
|-----------|-------------------|----------|----------------------------------------------------|
| ItemId    | `string`          | Yes      | Id in Data/Objects.                                |
| Condition | `string`          | No       | A Game State Query.                                |
| HasMod    | `string`          | No       | Only add this seed if a mod is installed.          |
| Weight    | `int`             | No       | Default 1.                                         |

### Example

Here is an example of an edit. We add possible seeds to MyCustomSeed (the item **must** exist in Data/Objects).
This will add the following seeds to the roster: Strawberry seeds, Starfruit seeds, and Blueberry seeds.

```jsonc
{
  "Action": "EditData",
  "Target": "Mods/mistyspring.ItemExtensions/MixedSeeds",
  "Entries": {
    "MyCustomSeed": [
      {
        "ItemId": "745",
        "Weight": 3
      },
      {
        "ItemId": "486",
        "Condition": "YEAR 2"
      },
      {
        "ItemId": "481",
        "HasMod": "SomeMod.ForThisSeed"
      }
    ]
  }
}
```

This is what will happen:

- Strawberries will be "added" thrice.
- Starfruit seeds will only be available if it's year 2 or more.
- Blueberry seeds will only be available if you have `SomeMod.ForThisSeed`.

If all these conditions are met, the "possible seeds" would look like this:

```txt
MyCustomSeeds 745 745 745 486 481
```

But let's say it's Y1 and you don't have `SomeMod.ForThisSeed`. Instead, it'd look like this:

```txt
MyCustomSeeds 745 745 745
```

From this list, a random one will be chosen.

(If a condition for x seed doesn't match, the seed will simply be ignored / not added to list).

## Excluding the main seed

The main seed is added by default to the roster. If you don't want it to, add this to its object's custom fields:

```
 "mistyspring.ItemExtensions/AddMainSeed": "0"
```

(Likewise, if you want to add the main seed more than once, just change the number to that. E.g, set it to '2' to add it twice.)