# Trigger actions

## mistyspring.mistycore_AddExp

Adds experience points to the specified player(s).

Use: `mistyspring.mistycore_AddExp Current Combat 50`

## mistyspring.mistycore_addItemHoldUp

Use: `mistyspring.mistycore_addItemHoldUp <item> [recipe] [quantity] [quality]`

The quality goes like this:
- 0 - normal 
- 1 - silver
- 2 - gold
- 4 - iridium

To avoid bugs, this trigger action only applies for the current player.

### Example 

`mistyspring.mistycore_addItemHoldUp (O)69 false 10 1`

This would add 10 (O)69 with the silver quality.