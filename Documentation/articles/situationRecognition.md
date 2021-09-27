# Situation Recognition

## Overview

The goal of the project is to be able to recognize basic situations occurring in the board game Carcassonne. For example, we might try to detect when a city, road, or cloister has been completed.

The main task will be to build an AI model to detect important situations in a game. The specific situations will depend on how the project develops, but one might, for example, attempt to detect when different game elements (roads, cities, cloisters) are completed and determine who gets points (and how many) for the element.

### Goals

In order of importance. Goals further down the list are secondary and only to be taken on if success is achieved in the first goal(s).

1. *[Possible Starting Point]* Perform situation detection using conventional (non-AI) methods (if this seems easier than using AI)
1. Produce an AI able to detect a specific simple game situation (e.g. a city is completed)
1. Produce an AI able to detect a more complex game situation (e.g. a player is attempting to merge two cities)
1. *[Stretch goal]* Produce a AI able to detect and classify general, unlabelled "situations" in game scenarios.

### Remember

This project is part of a larger codebase. The job is not just to make something that works. It is also to develop something that is organized and well-documented so that the next people who come along can add/modify/enhance it. Please make sure to comment code using [standard documentation formats](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags).

David will spend some time trying to get proper documentation building and unit testing enabled on the project, so think about how the code will be tested as well!

## Resources

### Game State

Vincent Bons has written about preparing Carcassonne for machine learning at his [website](https://wingedsheep.com/programming-carcassonne/), but this also contains some good ideas about how to represent the state of the game.
A modified version of his data representation is detailed at the [State Representation](states.md) document.

### Situation Recognition

Here's a short list of some resources on *situation recognition*. Treat this as a starting point. There are plenty more articles and approaches on this, so it would probably be helpful to take a little time to do some deeper reading.

* I suspect this is a little more formal than we will want to be here, but it's a good starting point: [A method for automatic situation recognition in collaborative multiplayer Serious Games](https://www.researchgate.net/publication/282560289_A_method_for_automatic_situation_recognition_in_collaborative_multiplayer_Serious_Games)
* I haven't given this a close read, but it looks interesting on first inspection: [Situation Recognition with Graph Neural Networks
](https://www.researchgate.net/publication/319135212_Situation_Recognition_with_Graph_Neural_Networks)
* There's a piece of software called EventShop that a lot seems to be written about. It is about 5 years old (at least) and doesn't seem to be in active development, but it might be worth giving it a look.
  * [Situation Recognition Using EventShop](https://www.springer.com/gp/book/9783319305356) is a book that is available through the MAU library
  * [A research website dealing with EventShop](http://slnlab.ics.uci.edu/research.html)
  * [EventShop Github page](https://github.com/Eventshop)