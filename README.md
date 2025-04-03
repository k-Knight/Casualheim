# Casualheim
Makes Valheim more casual in several ways.

## Cancelling attacks
Allows a player to cancel their attack if the attack has not yet dealt damage by:
- blocking
- dodging
- jumping

Also allows to configure that severity of slowdown in movement and rotation that attacking applies to the player.

## Allow building in cleared dungeons
Provides a toggle to enable building in dungeons when all the enemies inside them are dead.

This may require creating a new world.

## Enemy health nerf
Allows a player to set the % of max HP that Ashlands enemies will have.
Settings are individual for every mob.
Generally makes so that the enemies are less spongy.

## Boss max health and regen
Allows to cancel health regeneration for bosses, as well as modify their health.

Also provides a setting `NumberOfPlayersMax` that will disable the nerf/buff if the number of players is bigger that this value.
Each boss has individual setting for max health and regen percentage.

## Easier skill curve
Makes required skill experience curve linear in Valheim. That is required experience for next level will follow arithmetic progression (as of right now the formula for required experience to get next skill level in Valheim grows exponentially).

Allows you to configure the speed of the arithmetic progression with a multiplier. Default is 1, this makes total amount of required exp to reach skill level 100 about 4 times less than in vanilla.

## Enemy stars chance
Allows to apply a multiplier to a chance the enemy will spawn with stars / have higher level (the ones that make enemies stronger).

## Death penalty adjustment
Allows to configure the amount of skill loss on death. Also allows to preserve current progress to next skill level.