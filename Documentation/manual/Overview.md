# Overview

## Codebase

The code is divided into 5 main assemblies.
3 of these (@Carcassonne.Models, @Carcassonne.State, and @Carcassonne.Controllers) form the basic, re-usable code for the Carcassonne game.
The other two (@Carcassonne.AR and @Carcassonne.AI) contain implementation-specific code that relies on the other 3 libraries/assemblies to produce different working versions of the game.

@Carcassonne.AR is the main Hololens-playable version of the game, while @Carcassonne.AI encompasses environments for training gameplay AI agents.

## Models

The @Carcassonne.Models represent basic game entities in the Carcassonne boardgame.
They are designed to be Unity-specific, but implementation-independent, meaning that they are not dependent on, for example, networked play libraries (@Photon) or AI libraries (@MLAgents).
They are meant to be used as the basis for many different game interfaces with different needs.

The main models are @Carcassonne.Models.Player, @Carcassonne.Models.Tile, @Carcassonne.Models.Meeple.
These classes represent the players and game pieces and store information about their underlying properties (e.g. @Carcassonne.Models.Tile.Rotations) and state (@Carcassonne.Models.Player.score).

Also included in the @Carcassone.Models assembly are a set of Enums and helper classes.
