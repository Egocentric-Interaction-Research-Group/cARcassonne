using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Carcassonne.Models;
using JetBrains.Annotations;
using UnityEngine;

namespace Carcassonne.State
{
    public class TileIdComparer : IComparer<Tile>
    {
        public int Compare(Tile x, Tile y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            if (x.ID - y.ID != 0)
                return x.ID - y.ID;
            
            return x.gameObject.GetInstanceID() - y.gameObject.GetInstanceID();
        }
    }

    public class TileState : IGamePieceState<Tile>
    {
        public Stack<Tile> Remaining { get; set; }
        [CanBeNull] public Tile Current { get; set; }
        public Tile[,] Played => CalculatePlayed();
        
        public List<Tile> Discarded { get; set; }
        
        public Dictionary<Vector2Int, Tile> Placement = new Dictionary<Vector2Int, Tile>();

        public Vector2Int? lastPlayedPosition => Placement.SingleOrDefault(kvp => kvp.Value == Current).Key;

        private static readonly TileIdComparer tileIdComparer = new TileIdComparer();
        
        /// <summary>
        /// Dictionary of <Tile, Vector2Int?> with entries sorted by tile ID.
        /// </summary>
        public SortedDictionary<Tile, Vector2Int?> TilePlacement
        {
            get
            {
                // Add all placed tiles to the sorted dict
                var d = new SortedDictionary<Tile, Vector2Int?>(
                    Placement.Keys.ToDictionary(p => Placement[p],p => (Vector2Int?)p),
                    tileIdComparer);
                
                if( Current != null && !d.ContainsKey(Current) )
                    d.Add(Current, null);

                foreach (var tile in Remaining)
                {
                    d.Add(tile, null);
                }
                
                //TODO Should I differentiate between discarded and not picked? It doesn't happen very often...
                foreach (var tile in Discarded)
                {
                    d.Add(tile, null);
                }

                return d;
            }
        }

        public RectInt Limits => CalculateLimits();

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

        /// <summary>
        /// Right now, this is a fixed-dimension matrix. That could change. It could grow with the board.
        /// </summary>
        /// <returns></returns>
        private Geography?[,] CalculateMatrix()
        {
            // Create a new matrix that is 3 * (xmax-xmin) x 3 * (ymax - ymin);
            var geographyMatrix = new Geography?[3 * (GameRules.BoardLimits.xMax - GameRules.BoardLimits.xMin),
                3 * (GameRules.BoardLimits.yMax - GameRules.BoardLimits.yMin)];

            foreach (var kvp in Placement)
            {
                var position = kvp.Key + Vector2Int.FloorToInt(GameRules.BoardLimits.center);
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

            if (Placement.Count == 0) return lim;
            
            var xValues = Placement.Keys.Select(cell => cell.x);
            var yValues = Placement.Keys.Select(cell => cell.y);
            
            lim.xMin = xValues.Min();
            lim.xMax = xValues.Max();
            lim.yMin = yValues.Min();
            lim.yMax = yValues.Max();
            
            return lim;
        }

        //TODO I think this is wrong. It uses a static board size, which does not map to the cell positioning.
        private Tile[,] CalculatePlayed()
        {
            var played = new Tile[GameRules.BoardLimits.width, GameRules.BoardLimits.height];

            foreach (var tilePosition in Placement)
            {
                var position = tilePosition.Key + Vector2Int.FloorToInt(GameRules.BoardLimits.center);
                var tile = tilePosition.Value;

                played[position.x, position.y] = tile;
            }
                
            return played;
        }

        public TileState()
        {
            Placement = new Dictionary<Vector2Int, Tile>();
            Remaining = new Stack<Tile>();
            Discarded = new List<Tile>();
        }
        
        // private void Awake()
        // {
        //     Placement = new Dictionary<Vector2Int, Tile>();
        //     _remaining = new List<Tile>();
        // }
        //
        // private void OnEnable()
        // {
        //     Placement = new Dictionary<Vector2Int, Tile>();
        //     _remaining = new List<Tile>();
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