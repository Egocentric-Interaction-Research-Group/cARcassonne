# Overview

## Codebase

The code is divided into 5 main assemblies.
3 of these (@Carcassonne.Models, @Carcassonne.State, and @Carcassonne.Controllers) form the basic, re-usable code for the Carcassonne game.
The other two (@Carcassonne.AR and @Carcassonne.AI) contain implementation-specific code that relies on the other 3 libraries/assemblies to produce different working versions of the game.

@Carcassonne.AR is the main Hololens-playable version of the game, while @Carcassonne.AI encompasses environments for training gameplay AI agents.