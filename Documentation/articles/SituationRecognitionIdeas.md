# Feature Finding, Posthoc Clustering

Proposal: A "situation" is a turning point in the game.

What if the deep learning system's main task is to extract features, representations of the board.
Then, after the fact, it combs through gameplays and finds important situations, groups them, and then labels them.

# Graph Representation

Building on a reading of https://distill.pub/2021/gnn-intro/.

The representation could be a graph matrix (as a tensor) with each layer representing different graph properties.
One layer can represent (left/right/up/down) adjacency, another can represent meeples, another can represent nodal connections...
