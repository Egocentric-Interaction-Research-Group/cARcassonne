using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Carcassonne.State
{
    [CreateAssetMenu(fileName = "TileState", menuName = "States/TileState")]
    public class TileState : ScriptableObject, IGamePieceState<TileScript>//,TileScript.Geography>
    {
        public List<TileScript> Remaining => _remaining;
        [CanBeNull] public TileScript Current { get; set; }
        public TileScript[,] Played => _played; 
        
        public Vector2 lastPlayedPosition;

        private List<TileScript> _remaining;
        private TileScript[,] _played;

        /// <summary>
        /// The position of the bottom-left corner of the representation returned by Matrix in Subtile space.
        /// </summary>
        /// <remarks>
        /// For example, if the lower-leftmost city is found on a tile that is at position (x=10,y=15),
        /// MatrixOrigin would return (30,45). This can be added to the positions found in Matrix so that the
        /// data from Matrix line up with the bounding boxes returned by City.BoundingBox.
        /// </remarks>
        public Vector2Int MatrixOrigin => CalculateLimits().min * 3;

        /// <summary>
        /// The subtile matrix representation of the board. The bottom corner (Bottom-Left) is 0,0 and the top corner
        /// (top-right) is (3*x',3*y'), where x' and y' are the vertical and horizontal dimensions of the played tiles.
        /// This is done to match the representation used in the game. I don't know if it lines up with other image representations.
        /// Coordinates are represented [Horiz, Vert]
        /// </summary>
        public Geography?[,] Matrix => CalculateMatrix();

        private Geography?[,] CalculateMatrix()
        {
            // Find min and max indices for x and 
            // Are these things that TileScript could be tracking? BoardLimits? or something?
            var l = CalculateLimits();
            int xmin = l.xMin, xmax = l.xMax, ymin = l.yMin, ymax = l.yMax;
            
            Debug.Log($"Found Limits. Tiles found from ({xmin},{ymin}) to ({xmax},{ymax})");
            
            // Create a new matrix that is 3 * (xmax-xmin) x 3 * (ymax - ymin);
            var GeographyMatrix = new Geography?[3 * (xmax - xmin), 3 * (ymax - ymin)];
            
            for (var i = 0; i < xmax - xmin; i++)
            {
                for (int j = 0; j < ymax - ymin; j++)
                {
                    for (int a = 0; a < 3; a++)
                    {
                        for (int b = 0; b < 3; b++)
                        {
                            if (Played[i + xmin, j + ymin] is null)
                            {
                                GeographyMatrix[i * 3 + a, j * 3 + b] = null;
                            }
                            else
                            {
                                GeographyMatrix[i * 3 + a, j * 3 + b] = Played[i + xmin, j + ymin].Matrix[a, b];
                            }
                        }
                    }
                }
            }
            
            return GeographyMatrix;
        }

        private RectInt CalculateLimits()
        {
            RectInt lim = new RectInt();
            for (var i = 0; i < Played.GetLength(0); i++)
            {
                for (var j = 0; j < Played.GetLength(1); j++)
                {
                    if (Played[i, j] != null)
                    {
                        if (lim.size == Vector2Int.zero)
                        {
                            lim = new RectInt(i, j, 1, 1);
                        } else
                        {
                            if (i <  lim.xMin){ lim.xMin = i;}
                            if (i >= lim.xMax){ lim.xMax = i + 1;}
                            if (j <  lim.yMin){ lim.yMin = j;}
                            if (j >= lim.yMax){ lim.yMax = j + 1;}
                        }
                    }
                }
            }

            return lim;
        }

        private void Awake()
        {
            _played = new TileScript[GameRules.BoardSize, GameRules.BoardSize];
            _remaining = new List<TileScript>();
        }

        private void OnEnable()
        {
            _played = new TileScript[GameRules.BoardSize, GameRules.BoardSize];
            _remaining = new List<TileScript>();
        }

        public override string ToString()
        {
            var s = ""; 
            for (var j = Matrix.GetLength((1)) - 1; j >= 0; j--) // Prints from the top-left, so y coordinate needs to start from the top.
            {
                for (var i = 0; i < Matrix.GetLength(0); i++)
                {
                    s += $" | ({i},{j}): {Matrix[i,j], 8}";
                }
                s += $" |\n";
            }
            return s;
        }
    }
}