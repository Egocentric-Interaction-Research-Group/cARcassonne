# The New Game Architecture (January 2022)

## Layers

* Game State/Logic
* Game Actions
* Game View

## Game State/Logic (Model)

This covers non-visual aspects of the game.
Classes are not generally MonoBehaviours.
It holds the state of the board, the state of the deck.
It uses those states to calculate points, possible positions, etc.
It can be re-used by many different visual interfaces.

## Game Actions (Controller)

This is where things happen in the game.
Events are dispatched from here.
Actions are registered and passed to the model.
The AI Training and User Game Views utilize this layer to manipulate the game logic and state.

## Game View (View)

This covers aspects of the game in motion.
It includes visualizations, user manipulations, animations.