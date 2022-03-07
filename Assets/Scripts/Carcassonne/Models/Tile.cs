using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Carcassonne.Models
{
    public class Tile : MonoBehaviour

    {
        #region Constants

        public const int SubtTileDimension = 3;

        #endregion

        // public Tile(int id, IDictionary<Vector2Int, Geography> geographies, TileSet set = TileSet.Base,
        //     bool shield = false)
        // {
        //     this.id = id;
        //     this.set = set;
        //     Geographies = geographies;
        //     Shield = shield;
        // }
        //
        // public Tile(int id, Geography north, Geography east, Geography south, Geography west, Geography center,
        //     TileSet set = TileSet.Base, bool shield = false)
        // {
        //     this.id = id;
        //     this.set = set;
        //     Geographies.Add(Vector2Int.up, north);
        //     Geographies.Add(Vector2Int.right, east);
        //     Geographies.Add(Vector2Int.down, south);
        //     Geographies.Add(Vector2Int.left, west);
        //     Geographies.Add(Vector2Int.zero, center);
        //
        //     Shield = shield;
        // }

        /// <summary>
        ///     The ID decides which type of tile this tile is. Refer to the ID graph for exact results.
        ///
        /// THIS IS ONLY NEEDED FOR Inspector-EDITED ID VALUES.
        /// </summary>
        // [Obsolete("Only here to support Inspector-defined IDs.", error:false)]
        // public int id;
        
        public int ID
        {
            get => m_id;

            set => m_id = SetupAttributes(value);
        }

        // Has been initialized with Geography.
        public bool IsReady { get; private set; }

        /// <summary>
        ///     Defines whether the tile is a member of the base set or one of the expansions or alternate tile sets.
        /// </summary>
        public TileSet set = TileSet.Base;

        public IDictionary<Vector2Int, Geography> Geographies { get; private set; } = new Dictionary<Vector2Int, Geography>();

        /// <summary>
        ///     The number of times the tile has been rotated by 90 degrees.
        /// </summary>
        public int Rotations
        {
            get { return m_Rotations; }
            private set { m_Rotations = value % 4; }
        }

        private int m_Rotations;
        private int m_id;

        /// <summary>
        ///     Public property detailing whether this tile has a shield
        /// </summary>
        public bool Shield { get; private set; } = false;

        /// <summary>
        ///     A dictionary of the side geographies, indexed by Vector2Int.{up,down,left,right}.
        /// </summary>
        public Dictionary<Vector2Int, Geography> Sides =>
            Geographies.Where(kvp => kvp.Key.sqrMagnitude == 1).ToDictionary(item => item.Key, item => item.Value);

        /// <summary>
        ///     The sub-tile matrix representation of this tile. The bottom corner (Left-Down) is 0,0 and the top (Right, Top is
        ///     (2,2).
        ///     This is done to match the representation used in the game. I don't know if it lines up with other image
        ///     representations.
        ///     Coordinates are represented [Horiz, Vert]
        /// </summary>
        public Geography[,] Matrix => GetMatrix();

        public Dictionary<Vector2Int, Geography> SubTileDictionary => getSubTileDictionary();

        /// <summary>
        /// This should only be called by the tile controller. All in-game rotations should use the tile controller's
        /// rotation function.
        /// </summary>
        /// <param name="times"></param>
        public void Rotate(int times = 1)
        {
            Debug.Assert(times < 4,
                "Tile should not be rotated more than 3 times. It will be back in its original position.");
            Debug.Log($"Rotating {times} times.");
            for (times %= 4; times > 0; times--)
            {
                Debug.Log($"Rotate {Rotations}.");
                var tmp = Geographies[Vector2Int.up];
                Geographies[Vector2Int.up] = Geographies[Vector2Int.left];
                Geographies[Vector2Int.left] = Geographies[Vector2Int.down];
                Geographies[Vector2Int.down] = Geographies[Vector2Int.right];
                Geographies[Vector2Int.right] = tmp;

                Rotations++;
            }
        }

        public void RotateTo(int rotations)
        {
            Debug.Assert(rotations < 4,
                $"Rotational position ({rotations}) should be less than 4.");
            var r = (rotations + 4 - Rotations) % 4;
            Rotate(r);
        }

        public override string ToString()
        {
            return $"{North.ToString()[0]}{East.ToString()[0]}{South.ToString()[0]}{West.ToString()[0]}";
        }

        private Geography[,] GetMatrix()
        {
            var matrix = new Geography[SubtTileDimension, SubtTileDimension];

            matrix[1, 0] = South;
            matrix[0, 1] = West;
            matrix[2, 1] = East;
            matrix[1, 2] = North;

            // Set corners and middle. This is not a great way of doing this, but it avoids repeating a bunch of work...
            switch (Center)
            {
                // If the middle is a simple geography, set that, and set the corners to grass.
                // TODO Check the logic here.
                case Geography.City:
                case Geography.Cloister:
                case Geography.Field:
                case Geography.Road:
                case Geography.Stream:
                case Geography.Village:
                    matrix[1, 1] = (Geography)Center;
                    matrix[0, 0] =
                        Geography
                            .Field; // NB: Grass here doesn't necessarily mean grass IF you were playing with Farmers. But because we are not considering farmers, it is an easy shortcut.
                    matrix[2, 0] = Geography.Field;
                    matrix[0, 2] = Geography.Field;
                    matrix[2, 2] = Geography.Field;
                    break;
                case Geography.CityRoad:
                case Geography.CityStream:
                case Geography.RoadStream:
                    matrix[1, 1] = Geography.Field;
                    matrix[0, 0] = matrix[0, 1] == matrix[1, 0] ? matrix[0, 1] : Geography.Field;
                    matrix[2, 0] = matrix[2, 1] == matrix[1, 0] ? matrix[2, 1] : Geography.Field;
                    matrix[0, 2] = matrix[0, 1] == matrix[1, 2] ? matrix[0, 1] : Geography.Field;
                    matrix[2, 2] = matrix[2, 1] == matrix[1, 2] ? matrix[2, 1] : Geography.Field;
                    break;
            }

            return matrix;
        }

        private Dictionary<Vector2Int, Geography> getSubTileDictionary()
        {
            var d = new Dictionary<Vector2Int, Geography>();
            for (var i = 0; i < SubtTileDimension; i++)
            for (var j = 0; j < SubtTileDimension; j++)
                d.Add(new Vector2Int(i, j) - Vector2Int.one, Matrix[i, j]);

            return d;
        }

        public Geography GetGeographyAt(Vector2Int direction)
        {
            if (Geographies.ContainsKey(direction)) return Geographies[direction];

            if (direction.x > 1 || direction.y > 1)
                throw new ArgumentOutOfRangeException(
                    "Direction should be in [-1,-1] - [1,1]." +
                    $"{direction} is out of range.");

            // Handle corner geographies
            var leftRight = GetGeographyAt(new Vector2Int(direction.x, 0));
            var upDown = GetGeographyAt(new Vector2Int(0, direction.y));
            if (leftRight.HasCity() && upDown.HasCity() && Center != null && ((Geography)Center).HasCity())
                return Geography.City;
            return Geography.Field;
        }

        public static Vector2Int[] Directions = new Vector2Int[]{Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.left};

        #region Computed Properties

        /// <summary>
        ///     These are closely related to the Up, Down, Left and Right geographies. When the tile is rotated the values shift to
        ///     correlate to the new rotation:
        ///     If Up is road, but the rotation is 1 then East gets the value of Up, since it's rotated 90 degrees clockwise. If
        ///     rotation is 0 then North is equal to Up.
        /// </summary>
        public Geography North => Geographies[Vector2Int.up];

        public Geography South => Geographies[Vector2Int.down];
        public Geography West => Geographies[Vector2Int.left];
        public Geography East => Geographies[Vector2Int.right];
        public Geography? Center => Geographies.ContainsKey(Vector2Int.zero) ? Geographies[Vector2Int.zero] : (Geography?) null;

        #endregion

        /// <summary>
        ///     Depending on the ID of the tile it recieves different attributes.
        ///     ID's in tiles are not unique and they share them with other tiles who also recieve the same attributes.
        /// </summary>
        /// <param name="id"></param>
        public static Dictionary<Vector2Int, Geography> GetGeographies(int id)
        {
            var Up = new Geography();
            var Down = new Geography();
            var Left = new Geography();
            var Right = new Geography();
            var Center = new Geography();
            
            if (id == 1 || id == 2 || id == 3 || id == 4 || id == 5 || id == 6 || id == 12 || id == 17 || id == 25 ||
                id == 26 || id == 27 || id == 28) Up = Geography.Field;
            if (id == 1 || id == 2 || id == 4 || id == 7 || id == 9 || id == 14 || id == 25 || id == 27)
                Right = Geography.Field;
            if (id == 1 || id == 3 || id == 7 || id == 8 || id == 12 || id == 13 || id == 15 || id == 17 || id == 18 ||
                id == 20 || id == 22 || id == 26) Down = Geography.Field;
            if (id == 1 || id == 2 || id == 7 || id == 10 || id == 13 || id == 14 || id == 15 || id == 18 || id == 25)
                Left = Geography.Field;
            if (id == 6 || id == 29 || id == 30) Up = Geography.Road;
            if (id == 3 || id == 5 || id == 6 || id == 8 || id == 10 || id == 11 || id == 30) Right = Geography.Road;
            if (id == 2 || id == 4 || id == 5 || id == 6 || id == 9 || id == 10 || id == 11 || id == 16 || id == 19 ||
                id == 21 || id == 23 || id == 28 || id == 29 || id == 31) Down = Geography.Road;
            if (id == 3 || id == 4 || id == 5 || id == 6 || id == 8 || id == 9 || id == 11 || id == 16 || id == 19)
                Left = Geography.Road;
            if (id == 7 || id == 8 || id == 9 || id == 10 || id == 11 || id == 13 || id == 14 || id == 15 || id == 16 ||
                id == 18 || id == 19 || id == 20 || id == 21 || id == 22 || id == 23 || id == 24 || id == 31 ||
                id == 32 ||
                id == 33) Up = Geography.City;
            if (id == 12 || id == 13 || id == 15 || id == 16 || id == 17 || id == 18 || id == 19 || id == 20 ||
                id == 21 ||
                id == 22 || id == 23 || id == 24 || id == 33) Right = Geography.City;
            if (id == 14 || id == 24 || id == 32) Down = Geography.City;
            if (id == 12 || id == 17 || id == 20 || id == 21 || id == 22 || id == 23 || id == 24) Left = Geography.City;
            if (id == 26 || id == 28 || id == 29 || id == 31 || id == 32) Right = Geography.Stream;
            if (id == 25 || id == 27 || id == 30 || id == 33) Down = Geography.Stream;
            if (id == 26 || id == 27 || id == 28 || id == 29 || id == 30 || id == 31 || id == 33)
                Left = Geography.Stream;
            if (id == 1 || id == 2 || id == 28) Center = Geography.Cloister;
            if (id == 3 || id == 4 || id == 8 || id == 9 || id == 10 || id == 29 || id == 30) Center = Geography.Road;
            if (id == 5 || id == 6 || id == 11) Center = Geography.Village;
            if (id == 7 || id == 14 || id == 15 || id == 32) Center = Geography.Field;
            if (id == 12 || id == 13 || id == 17 || id == 18 || id == 20 || id == 21 || id == 22 || id == 23 ||
                id == 24 ||
                id == 31) Center = Geography.City;
            if (id == 33) Center = Geography.CityStream;
            if (id == 16 || id == 19) Center = Geography.CityRoad;

            // Tile t = new Tile(id, Up, Right, Down, Left, Center, TileSet.Base, shield);
            //
            // return t;
            var geographies = new Dictionary<Vector2Int, Geography>();
            geographies[Vector2Int.down] = Down;
            geographies[Vector2Int.left] = Left;
            geographies[Vector2Int.right] = Right;
            geographies[Vector2Int.up] = Up;
            geographies[Vector2Int.zero] = Center;
            
            return geographies;
        }

        public static Dictionary<int, int> GetIDDistribution()
        {
            var distribution = new Dictionary<int, int>()
            {
                {1,4},
                {2,2},
                {3,8},
                {4,9},
                {5,4},
                {6,1},
                {7,5},
                {8,4},
                {9,3},
                {10,3},
                {11,3},
                {12,1},
                {13,3},
                {14,3},
                {15,2},
                {16,3},
                {17,2},
                {18,2},
                {19,2},
                {20,3},
                {21,1},
                {22,1},
                {23,2},
                {24,1},
            };
            
            return distribution;
        }

        // private void Awake()
        // {
        //     IsReady = false;
        // }
        //
        // // This is only needed if we are setting ID in the editor
        // private void Start()
        // {
        //     SetupAttributes(id);
        //     IsReady = true;
        // }

        private int SetupAttributes(int i)
        {
            Geographies = GetGeographies(i);
            Shield = GetShield(i);
            // id = ID;
            return i;
        }
        
        private static bool GetShield(int id)
        {
            if (id == 17 || id == 18 || id == 19 || id == 22 || id == 23 || id == 24)
                return true;

            return false;
        }
        
        /// <summary>
        ///     Describes the different set of game tiles (used in different versions of gameplay).
        /// </summary>
        public enum TileSet
        {
            Base,
            River
        }
	}
}