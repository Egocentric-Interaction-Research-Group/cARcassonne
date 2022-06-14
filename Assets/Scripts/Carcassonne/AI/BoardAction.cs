using UnityEngine;

namespace Carcassonne.AI
{
    public enum ActionApproach
    {
        [InspectorName("Space-by-space")]
        [Tooltip("Action space: ??\nEach turn, the AI moves the tile in one direction for one square.")]
        SpaceBySpace,

        [InspectorName("Whole board")]
        [Tooltip("Action space size: ??\nEach turn, the AI tried to place the tile on any valid square on the board.")]
        Board,
        
        [InspectorName("Integrated")]
        [Tooltip("Action space size: [38400]\nEach turn, the AI tried to place the tile and meeple on any valid integrated position on the board.")]
        Integrated
    }

    public static class BoardAction
    {
        
    }
}