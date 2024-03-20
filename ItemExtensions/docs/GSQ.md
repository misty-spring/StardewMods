# Game state queries

-------------

## Tool upgrades

`mistyspring.ItemExtensions_ToolUpgrade <player> <tool> [min] [max] [recursive]`

Checks if the given tool is in the upgrade range. Must be in inventory- if `recursive`, it'll search on chests too.

Min defaults to 0, Max defaults to 4 and recursive is default false.

### Values
| name | equivalent  |
|------|-------------|
| 0    | No upgrades |
| 1    | Copper      |
| 2    | Iron        |
| 3    | Gold        |
| 4    | Iridium     |

### Example

`mistyspring.ItemExtensions_ToolUpgrade Any Axe 1 3 true`

This will check if any player has an axe between copper and gold.

### Caveats
- If `recursive` is false and there's no `<tool>` in the player inventory, it'll return false.
- `recursive` checks for the tool's last user. If none of the existing axes have target as last user, it'll *also* return false.

-------------

## Inventory Items

`mistyspring.ItemExtensions_InInventory <player> <qualifiedId> [min] [max] [quality]`

Checks if the given item is in inventory. 

Optionally: you can state a minimum, maximum, and min. quality.