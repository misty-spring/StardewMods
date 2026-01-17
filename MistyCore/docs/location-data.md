# Location data

## Contents
* [Adding critters](#adding-critters)
* [Custom backgrounds](#custom-backgrounds)
* [Custom hoe dirt](#custom-hoe-dirt)
* [Fishing conditions](#fishing-conditions)

<hr>

## Adding critters
To add critters, you must edit `Mods/mistyspring.mistycore/Locations/Critters`. This is a `Dictionary<string, Dictionary<string,CritterSpawnData>>>` where the data model consists of:

| name      | type            | required | description                                                |
|-----------|-----------------|----------|------------------------------------------------------------|
| X         | `int`           | Yes      | The X coordinate in the map.                               |
| Y         | `int`           | Yes      | The Y coordinate in the map.                               |
| Critter   | `CritterType`\* | Yes      | The critter type. See table below for allowed critters.    |
| Chance    | `float`         | No       | The chance for this critter to spawn. Ranges from 0 to 1.  |
| Condition | `string`        | No       | The GSQ for this critter to spawn.                         |
| Flip      | `bool`          | No       | Whether to flip the sprite. Doesn't work for all critters. |

\*= The CritterType can be `Butterfly`, `CalderaMonkey`, `CrabCritter`, `Crow`, `Firefly`, `Frog`, `Opossum`, `OverheadParrot`, `Owl`, `Rabbit`, `Seagull`, or `Squirrel`.

There are also some critter-specific fields. These are the following:

### Butterfly-specific fields

| name                 | type   | required | description                                         |
|----------------------|--------|----------|-----------------------------------------------------|
| IslandButterfly      | `bool` | No       | Whether it should be an island butterfly instead.   |
| ForceSummerButterfly | `bool` | No       | Whether to force the butterfly to be summer themed. |

### Seagull-specific fields

| name            | type              | required | description                                |
|-----------------|-------------------|----------|--------------------------------------------|
| SeagullBehavior | `SeagullBehavior` | No       | The seagull behavior. Defaults to Walking. |

SeagullBehavior has the following options: `Walking`, `FlyingAway`, `Swimming`, `Stopped`, and `FlyingToLand`.

### Example

To add a single critter:
```json
{
  "Action": "EditData",
  "Target": "Mods/mistyspring.mistycore/Locations/Critters",
  "Entries": {
    "{{ModId}}_MyCustomLocation": {
      "{{ModId}}.CrabCritter": {
        "X": 10,
        "Y": 10,
        "Critter": "CrabCritter",
        "Chance": 0.5,
        "Condition": "PLAYER_HAS_MAIL Current SomeCustomFlag"
      }
    }
  }
}
```

You can add multiple critters to the same location. Like this:
```json
{
  "Action": "EditData",
  "Target": "Mods/mistyspring.mistycore/Locations/Critters",
  "Entries": {
    "{{ModId}}_MyCustomLocation": [
      {
        "X": 10,
        "Y": 10,
        "Critter": "CrabCritter",
        "Chance": 0.5,
        "Condition": "PLAYER_HAS_MAIL Current SomeCustomFlag"
      },
      {
        "X": 12,
        "Y": 20,
        "Critter": "Crow",
        "Chance": 0.1
      }
    ]
  }
}
```
And so on. It's a list, so you can add as many as you want without limits.

## Custom backgrounds

To add custom backgrounds, edit `Mods/mistyspring.mistycore/Locations/Backgrounds`. This is a `Dictionary<string,BackgroundData>`, where the key is the location name and the value has the following options:

| name               | type     | required | description                                            |
|--------------------|----------|----------|--------------------------------------------------------|
| TexturePath        | `string` | Yes      | The path to the background texture.                    |
| Speed              | `int`    | No       | The higher the number, the slower it moves. Default 4. |
| HorizontalParallax | `bool`   | No       | Whether the texture should move. Default true.         |

### Example

```json
{
    "Action": "EditData",
    "Target": "Mods/mistyspring.mistycore/Locations/Backgrounds",
    "Entries": {
        "{{ModId}}_MyCustomMap": {
            "TexturePath": "LooseSprites/Cloudy_Ocean_BG_Night",
            "Speed": 8,
            "HorizontalParallax": true
        }
    }
}
```

## Custom hoe dirt

To add custom hoe dirt, edit `Mods/mistyspring.mistycore/Locations/HoeDirt`. This is a `Dictionary<string,string>`, where the key is the location name and the value is the texture path.

### Example

```json
{
  "Changes": [
    {
      "Action": "Load",
      "Target": "Mods/{{ModId}}/Textures/MyCustomHoeDirt",
      "FromFile": "assets/MyCustomHoeDirt.png"
    },
    {
      "Action": "EditData",
      "Target": "Mods/mistyspring.mistycore/Locations/HoeDirt",
      "Entries": {
        "{{ModId}}_MyCustomMap": "Mods/{{ModId}}/Textures/MyCustomHoeDirt"
      }
    }
  ]
}
```

## Fishing conditions

To add fishing overrides/conditions, you must edit `Mods/mistyspring.mistycore/Locations/FishingOverrides`. This is a data model that consists of:

| name              | type           | required | description                                            |
|-------------------|----------------|----------|--------------------------------------------------------|
| MissingRod        | `List<string>` | No       | The rod that must be missing.\*                        |
| MissingTackle     | `List<string>` | No       | The tackle that must be missing.\*                     |
| MissingBait       | `List<string>` | No       | The bait that must be missing.\*                       |
| PossibleOverrides | `List<string>` | No       | Possible items to give instead of the location's fish. |
| Condition         | `string`       | No       | The GSQ for this override to work.                     |
| Message           | `string`       | No       | The message to show.                                   |

\*= The way this works is by checking for a missing rod/tackle/bait. 
Example: If the player is fishing with normal bait, and you've set deluxe bait inside `MissingBait`, the override will happen. However, if the player is using deluxe bait, no override will happen.

### Example

```json
{
  "Action": "EditData",
  "Target": "Mods/mistyspring.mistycore/Locations/FishingOverrides",
  "Entries": {
    "{{ModId}}_MyCustomLocation": {
      "MissingRod": [
        "{{ModId}}_MyCustomRod" //requires a custom rod to fish here
      ],
      "PossibleOverrides": [ //if you're not using the custom rod, you will get these items instead
        "168",
        "169",
        "170",
        "171",
        "172"
      ],
      "Message": "You need a special rod to fish here..."
    }
  }
}
```