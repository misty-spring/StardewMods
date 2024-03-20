# LightData

Light data can be used in two ways: in custom resources, or as object extensibility (in mods' `/Data` file).

## Contents

* [Format](#format)
* [Example](#example)

---

## Format

`LightData` follows the following format:


| name         | type     | Required | description                                                           |
|--------------|----------|----------|-----------------------------------------------------------------------|
| Size         | `float`  | Yes      | Glow size. Default 1.2                                                |
| Hex          | `string` | No       | Hex code for light color, takes priority over RGB.                    |
| R            | `int`    | No       | Red value for light. Requires G, B.                                   |
| G            | `int`    | No       | Green value for color. Requires R, B.                                 |
| B            | `int`    | No       | Blue value for color. Requires G, R.                                  |
| Transparency | `float`  | No       | Transparency, any value between 1.0 and 0.x (can't be 0). Default 0.9 |

## Example

Here, we make a resource that glows.

```jsonc
{
  "Action": "EditData",
  "Target": "Mods/mistyspring.ItemExtensions/Resources",
  "Entries": {
    "{{ModId}}_GemStone": {
      "Name": "{{ModId}}_GemStone",
      "SpriteIndex": 1,
      "Texture": "Mods\\{{ModId}}\\Objects",
      "Health": 10,
      "Sound": "hammer",
      "BreakingSound": "stoneCrack",
      "ItemDropped": "(O){{ModId}}_Gem",
      "Tool": "Pickaxe",
      "MinToolLevel": 1, //if less than copper, it'll give the "needs upgrade" msg
      "MinDrops": 1,
      "MaxDrops": 3,
      "ExtraItems": [
        {
          "Id": "(O)107", //dinosaur egg
          "Chance": 0.9 //50%
        },
        {

        }
      ],
      "Debris": "(O)84", //"stone"
      "ContextTags": [
        "color_purple"
      ]
    }
  }
}
```
