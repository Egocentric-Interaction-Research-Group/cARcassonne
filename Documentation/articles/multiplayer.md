# Barriers to 3+ players in cARcassonne

## Meeple Spawning
Currently Meeples for 2 players are pre-defined as game objects in the `TableAnchor` hierarchy (`TableAnchor1 > Game > Table > Meeples`).
This needs to be re-designed if more than 2 players are going to play at a time! Also, `MeepleScript.SetMeepleOwner` appears to have 2 players hardcoded into it.
