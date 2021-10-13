# Barriers to 3+ players in cARcassonne

## Meeple Spawning
Currently Meeples for 2 players are pre-defined as game objects in the `TableAnchor` hierarchy (`TableAnchor1 > Game > Table > Meeples`).
This needs to be re-designed if more than 2 players are going to play at a time! Also, `MeepleScript.SetMeepleOwner` appears to have 2 players hardcoded into it.

## Network Lobby and Rooms
There is currently no option for creating a new room or choosing an existing room. The way it works now, is that the `PhotonLobby`
automatically joins a random room at `PhotonLobby.OnConnectedToMaster`. This method is called when connected to the Photon master server,
which seemingly happens right away when you start the game, without having to press the in-game start button. When a room cannot be joined,
it creates a new random and joins that instead. This explains the current behavior: when starting the game, it tries to join a random room,
but there is none, so it creates a new room. When other players launch their game, they too attempt to join a random game, which would always
lead to the same room created by the first player.

A solution would be to modify `PhotonLobby.cs` so that it doesn't join a random game when the game initially starts.
You could then creating additional interface elements to provide the clients with a list of open rooms, and an option
to create a new room. Also, the in-game start button should call a method on `PhotonLobby.cs` in order to join the 
selected room or create and join a new room.

In other words, we need to redo a lot of UI in order to provide a list of *selectable* open rooms, and a new button
for the option of creating a new room. As an alternative we might, instead of a separate button for a new room,
always have a *New Room* list item at the bottom of the list, which would reduce the workload somewhat, and is
perhaps even preferrable.

> This is obviously not ideal in the long term, but for now, I'm wondering if it's possible to do this through the Unity Inspector Interface. Basically, have a public field that you can edit in the inspector that has the name (or ID) of a room to join and can also be set to a value that creates a new, empty room. That way, if we need to test multiplayer things, we can just agree on a room name to enter in Unity. This won't work so well for the Hololens, but we can set a room name that the Hololens uses and then anyone who wants to play with the Hololens, just loads that room...
