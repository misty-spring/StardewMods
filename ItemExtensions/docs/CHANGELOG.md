# CHANGELOG

## 1.11.1
- Added korean by yuuyeeyii
- Traded tools are re-enchanted
- API method updates

## 1.11.0
- Updated for 1.6.9
- If an enchanted item is traded, the game will give you a prismatic shard as compensation.
- Fixed bug about clumps not spawning in Qi's skull cavern.
- Fixed bug about ore's monsters spawning in the wrong coordinates.
- Added new API method

## 1.10.0
- Added `ChanceDropOnFront` for train drops.

## 1.9.1
- Fixed issue in resource's monster spawning

## 1.9.0
- Added AvoidItemIds (`List<string>`) for ISpawnItemData: you can now avoid certain IDs when spawning drops
- Fixed bug in spawn conditions/quantity
- Train drops: X axis will now vary a little

## 1.8.1
- Rolled back some API changes

## 1.8.0
- Added vanilla clumps to API's IsClump method.
- Added more conditions for fishing treasure's items
- Changed the `/Treasure` dictionary to have a `TreasureData` value.
- Small refactoring

## 1.7.0
- Removed incorrect seeds from `CropPatches.GetVanillaCropsForSeason`
- Fixed bug where tractor mod couldn't plant custom mixed seeds
- Can now customize eating text (by aceynk)

## 1.6.1
- Bug fix for treasure rewards
- Fixed bug where seeds couldn't be planted
- Updated french translation (by Caranud)
- Updated chinese translation (by Awassakura)

## 1.6.0
- Bug fix for train drops
- New OnBehaviors: OnAttached, OnDetached
- Support for custom fishing treasure
- API changes

## 1.5.1
- New API method
- Temptative bugfix for mixed seeds 

## 1.5.0
- Fixed mixed seed bug where crops would randomly change to another type
- Fixed mining bug where some weapons did 0 damage
- Added customizable train drops
- Can now spawn ores in mountain and volcano
- Added "frenzy" option for spawning, akin to mushroom levels
- Can now spawn the following in mines: trees, fruit trees, giant crops
- Can now force items to always have a specific quality
- Can now turn specific components on/off
- Can add non-fish to ponds.
- Nodes now have the "placeable" tag by default.

## 1.4.3
- Fixed tractor mod bug (final)

## 1.4.2
- Bug fixes

## 1.4.1
- Fixed a bug for clumps in mineshaft

## 1.4.0
- Bug fix for menu actions and multiplayer item drops
- Changed how menu item actions are stored

## 1.3.0
- Tractor mod compatibility
- Can now set a max days for resources via location's CustomFields
- Can now add extra Panning drops

## 1.2.2
- Bug fixes

## 1.2.1
- Bug fixes

## 1.2.0
- Implemented mineshaft spawning for resources

## 1.1.0
- Ores and clumps can now be spawned in mineshaft
- Changes to API
- Better compatibility for vanilla mixed seeds

## 1.0.0
Initial release.
