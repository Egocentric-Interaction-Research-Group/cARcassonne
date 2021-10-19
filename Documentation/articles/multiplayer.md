# Barriers to 3+ players in cARcassonne

## Meeple Spawning
Currently Meeples for 2 players are pre-defined as game objects in the `TableAnchor` hierarchy (`TableAnchor1 > Game > Table > Meeples`).
This needs to be re-designed if more than 2 players are going to play at a time! Also, `MeepleScript.SetMeepleOwner` appears to have 2 players hardcoded into it.

## Network Lobby and Rooms [FIXED for now]
Before the fix, each client would join a random existing game. If it didn't exist, it created one. This resulted in everyone
always joining the same room.

The way it works now, is that you may modifiy exposed fields in the inspector of PhotonLobby in order to specify which room to
join (by name). If the room doesn't exist, it automatically creates one with the given name. When creating a room, the exposed
field 'maxPlayers' is used to set the maximum number of players allowed in the room.
An attempt to postpone the joining of a room seemed to be break more things than it was intended to solve, so for now
the room specified in the inspector is joined when starting the game. The in-game start button only takes you to the 
Carcassonne board, since you would already have joined a room on start-up.

> It would be ideal in the future, if we redo a lot of UI in order to provide a list of *selectable* open rooms, and a new button
for the option of creating a new room. As an alternative we might, instead of a separate button for a new room,
always have a *New Room* list item at the bottom of the list, which would reduce the workload somewhat, and is
perhaps even preferrable. This way, you would only join a lobby on start up. You would join a room when selected
in the GUI and you press the "Start Game" button. *Keep in mind*, though, that attempting to postpone the joining of rooms,
will most likely introduce an array of issues, so tread lightly. -- *Kasper Skott*
