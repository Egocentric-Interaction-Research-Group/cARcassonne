# Gameplay AI

## Overview

The goal of this project is to develop an AI to play the board game Carcassonne. The AI should be able to learn how to play the game against other AI agents as well as against human players. It should have a tunable play difficulty and should be relatively transparent in its decision-making so that it is possible to understand why it is taking particular actions.

The ultimate goal is to integrate the AI into the augmented reality (AR) version of Carcassonne that is under development. For this, it would be beneficial for the agent to be developed using Unity’s ML-Agents toolkit (https://unity.com/products/machine-learning-agents). However, it is also possible to experiment with other versions of Carcassonne and alternative learning toolkits such as PyTorch and Tensorflow.

A secondary goal of this project is to generate a dataset of possible game moves and their utility within the game. In training the AI agents, we will record all of the game metadata in order to create a large database of Carcassonne gameplay actions to use in future studies.

### Goals

In order of importance. Goals further down the list are secondary and only to be taken on if success is achieved in the first goal(s).

1. Produce a working Carcassonne AI that plays in the cARcassonne game in Unity
1. Simulate AI-vs-AI games
1. Extend the AI to emulate different player strategies, levels of experience, etc.

### Remember

This project is part of a larger codebase. The job is not just to make something that works. It is also to develop something that is organized and well-documented so that the next people who come along can add/modify/enhance it. Please make sure to comment code using [standard documentation formats](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags).

David will spend some time trying to get proper documentation building and unit testing enabled on the project, so think about how the code will be tested as well!

## Resources

### Carcassonne AI

Vincent Bons has written about preparing Carcassonne for machine learning at his [website](https://wingedsheep.com/programming-carcassonne/).
A modified version of his data representation is detailed at the [State Representation](states.md) document.

### AI in Unity

Machine learning integration in Unity is often done using [ML-Agents](https://unity.com/products/machine-learning-agents). Detailed documentation for ML-Agents is [here](https://github.com/Unity-Technologies/ml-agents/blob/release_18_docs/docs/Readme.md)

### MRTK Input Simulation

* https://docs.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/features/input-simulation/input-simulation-service?view=mrtkunity-2021-05
