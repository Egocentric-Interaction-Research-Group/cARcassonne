using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Carcassonne.State
{
    public interface IGamePieceState<TGamePiece>//,TMatrix>
    {
        // public Stack<TGamePiece> Remaining { get; }
        [CanBeNull] public TGamePiece Current { get; set; }
        public TGamePiece[,] Played { get; }

        /// <summary>
        /// The position of the bottom-left corner of the representation returned by Matrix in Subtile space.
        /// </summary>
        /// <remarks>
        /// For example, if the lower-leftmost city is found on a tile that is at position (x=10,y=15),
        /// MatrixOrigin would return (30,45). This can be added to the positions found in Matrix so that the
        /// data from Matrix line up with the bounding boxes returned by City.BoundingBox.
        /// </remarks>
        public Vector2Int MatrixOrigin { get; }

        /// <summary>
        /// The subtile matrix representation of the board. The bottom corner (Bottom-Left) is 0,0 and the top corner
        /// (top-right) is (3*x',3*y'), where x' and y' are the vertical and horizontal dimensions of the played tiles.
        /// This is done to match the representation used in the game. I don't know if it lines up with other image representations.
        /// Coordinates are represented [Horiz, Vert]
        /// </summary>
        // TMatrix?[,] Matrix { get; }
    }
}