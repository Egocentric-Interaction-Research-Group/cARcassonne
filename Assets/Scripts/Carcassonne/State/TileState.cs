using System;
using System.Collections.Generic;
using System.Linq;
using Carcassonne.Tile;
using JetBrains.Annotations;
using UnityEngine;

namespace Carcassonne.State
{
    public class TileIdComparer : IComparer<TileScript>
    {
        public int Compare(TileScript x, TileScript y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            
            return x.id - y.id;
        }
    }

    public class TileState : IGamePieceState<TileScript>
    {
        public List<TileScript> Remaining => _remaining;
        [CanBeNull] public TileScript Current { get; set; }
        public TileScript[,] Played => CalculatePlayed();
        
        public Dictionary<Vector2Int, TileScript> Placement = new Dictionary<Vector2Int, TileScript>();

        public Vector2 lastPlayedPosition;

        private List<TileScript> _remaining;

        private static readonly TileIdComparer tileIdComparer = new TileIdComparer();
        public SortedDictionary<TileScript, Vector2Int?> TilePlacement
        {
            get
            {
                // Add all placed tiles to the sorted dict
                var d = new SortedDictionary<TileScript, Vector2Int?>(
                    Placement.Keys.ToDictionary(p => Placement[p],p => (Vector2Int?)p),
                    tileIdComparer);

                foreach (var tile in Remaining)
                {
                    d.Add(tile, null);
                }

                return d;
            }
        }

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
            // Create a new matrix that is 3 * (xmax-xmin) x 3 * (ymax - ymin);
            var geographyMatrix = new Geography?[3 * (GameRules.BoardLimits.xMax - GameRules.BoardLimits.xMin),
                3 * (GameRules.BoardLimits.yMax - GameRules.BoardLimits.yMin)];

            foreach (var kvp in Placement)
            {
                var position = kvp.Key;
                var tile = kvp.Value;
                
                for (int a = 0; a < 3; a++)
                {
                    for (int b = 0; b < 3; b++)
                    {
                        geographyMatrix[position.x * 3 + a, position.y * 3 + b] =
                            tile.Matrix[a, b];
                    }
                }
            }
            
            return geographyMatrix;
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

        private TileScript[,] CalculatePlayed()
        {
            var played = new TileScript[GameRules.BoardSize, GameRules.BoardSize];

            foreach (var tilePosition in Placement)
            {
                var position = tilePosition.Key;
                var tile = tilePosition.Value;

                played[position.x, position.y] = tile;
            }
                
            return played;
        }

        public TileState()
        {
            Placement = new Dictionary<Vector2Int, TileScript>();
            _remaining = new List<TileScript>();
        }
        
        // private void Awake()
        // {
        //     Placement = new Dictionary<Vector2Int, TileScript>();
        //     _remaining = new List<TileScript>();
        // }
        //
        // private void OnEnable()
        // {
        //     Placement = new Dictionary<Vector2Int, TileScript>();
        //     _remaining = new List<TileScript>();
        // }

        public override string ToString()
        {
            var s = "";


            for (int i = 0 ; i < Matrix.GetLength(0) ; i++)
            {
                for (int j = 0; j < Matrix.GetLength(1); j++)
                {
                    s += $"{Matrix[i,j], 8}, ";
                }
                s += $"\n";
            }

            /*
            for (int j = Matrix.GetLength(1) - 1; j >= 0; j--) // Prints from the top-left, so y coordinate needs to start from the top.
            {
                for (int i = 0; i < Matrix.GetLength(0); i++)
                {
                    if (i == Matrix.GetLength(0) - 1)
                    {
                        if (Matrix[i, j] == null)
                        {
                            s += $"{"0",0}";
                        }
                        else
                            s += $"{Matrix[i, j],0}";
                    }
                    else
                    {
                        if (Matrix[i, j] == null)
                        {
                            s += $"{"0,",0}";
                        }
                        else
                            s += $"{Matrix[i, j],0},";
                    }
                }
                s += $"\n";

            }
            */
            return s;
        }
    }
}