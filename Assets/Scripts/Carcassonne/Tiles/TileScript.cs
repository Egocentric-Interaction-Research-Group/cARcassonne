using System;
using System.Collections.Generic;
using Carcassonne.Controllers;
using Photon.Pun;
using UI.Grid;
using UnityEngine;

namespace Carcassonne.Tiles
{
    public class TileScript : MonoBehaviourPun
    {

        public const int SubtTileDimension = 3; 
    
        /// <summary>
        ///     Describes the different set of game tiles (used in different versions of gameplay).
        /// </summary>
        public enum TileSet
        {
            Base,
            River
        }

        /// <summary>
        ///     The ID decides which type of tile this tile is. Refer to the ID graph for exact results.
        /// </summary>
        public int id;

        /// <summary>
        ///     How many times the tile has been rotated. In standard the rotation is 0, and rotated 4 times it returns to 0.
        /// </summary>
        public int rotation;

        /// <summary>
        ///     The vIndex of the tile. Is applied when placed on the board
        /// </summary>
        // public int vIndex;

        public GameObject northCollider, southCollider, westCollider, eastCollider;

        /// <summary>
        ///     The list of textures. All tile instances have a reference of all the textures so it can assign it to itself
        ///     depending on the tile ID
        /// </summary>
        public Texture[] textures;

        /// <summary>
        ///     These are closely related to the Up, Down, Left and Right geographies. When the tile is rotated the values shift to
        ///     correlate to the new rotation:
        ///     If Up is road, but the rotation is 1 then East gets the value of Up, since it's rotated 90 degrees clockwise. If
        ///     rotation is 0 then North is equal to Up.
        /// </summary>
        public Geography North, South, West, East, Center;

        /// <summary>
        ///     A list of the sides in clockwise order, starting from North.
        /// </summary>
        // public Geography[] Sides => new[] { North, East, South, West };
        public Dictionary<Vector2Int, Geography> Sides => new Dictionary<Vector2Int, Geography>
        {
            {Vector2Int.up, North},
            {Vector2Int.right, East},
            {Vector2Int.down, South},
            {Vector2Int.left, West} 
        };

        /// <summary>
        ///     Defines whether the tile is a member of the base set or one of the expansions or alternate tile sets.
        /// </summary>
        public TileSet tileSet = TileSet.Base;

        /// <summary>
        ///     Decides whether this tile has a shield or not
        /// </summary>
        private bool shield;

        /// <summary>
        /// Public property detailing whether this tile has a shield
        /// </summary>
        public bool Shield => shield;

        /// <summary>
        ///     Geography locations set to different local directions.
        /// </summary>
        private Geography Up, Down, Left, Right;
        
        private void Start()
        {
            // This is just so we have a static name for the Tile representing it's sides ar rotation == 0. 
            Up = North;
            Right = East;
            Down = South;
            Left = West;
        }

        /// <summary>
        ///     Simple getter for the centerGeography
        /// </summary>
        /// <returns>The center geography</returns>
        public Geography getCenter()
        {
            return Center;
        }

        private Geography[,] matrix = new Geography[3, 3];
        
        /// <summary>
        /// The sub-tile matrix representation of this tile. The bottom corner (Left-Down) is 0,0 and the top (Right, Top is (2,2).
        /// This is done to match the representation used in the game. I don't know if it lines up with other image representations.
        /// Coordinates are represented [Horiz, Vert]
        /// </summary>
        public Geography[,] Matrix
        {
            get => matrix;
            // private set => matrix = value;
        }
        
        private void UpdateMatrix()
        {
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
                    matrix[1, 1] = Center;
                    matrix[0, 0] = Geography.Field; // NB: Grass here doesn't necessarily mean grass IF you were playing with Farmers. But because we are not considering farmers, it is an easy shortcut.
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
            
            // Log debug statement here.
        }

        public Dictionary<Vector2Int, Geography> SubTileDictionary => getSubTileDictionary();

        private Dictionary<Vector2Int, Geography> getSubTileDictionary()
        {
            Dictionary<Vector2Int, Geography> d = new Dictionary<Vector2Int, Geography>();
            for (var i = 0; i < SubtTileDimension; i++)
            {
                for (var j = 0; j < SubtTileDimension; j++)
                {
                    d.Add(new Vector2Int(i, j) - Vector2Int.one, Matrix[i,j]);
                }
            }

            return d;
        }

        public Geography getGeographyAt(Vector2Int direction)
        {
            if (direction == Vector2Int.up) return North;
            if (direction == Vector2Int.down) return South;
            if (direction == Vector2Int.right) return East;
            if (direction == Vector2Int.left) return West;
            if (direction == Vector2Int.zero) return Center;

            if (direction.x > 1 || direction.y > 1)
            {
                throw new ArgumentOutOfRangeException(
                    "Direction should be in [-1,-1] - [1,1]." +
                    $"{direction} is out of range.");
            }
            
            // Handle corner geographies
            var leftRight = getGeographyAt(new Vector2Int(direction.x, 0));
            var upDown = getGeographyAt(new Vector2Int(0, direction.y));
            if (leftRight.HasCity() && upDown.HasCity() && Center.HasCity())
            {
                return Geography.City;
            }
            else
            {
                return Geography.Field;
            }
        }

        /// <summary>
        ///     The method used to rotate the tile. In essence it just cycles the rotation between 1 and 3 (and returns to 0 when
        ///     rotated after 3), and switches the north east south west values clockwise.
        /// </summary>
        public void Rotate()
        {
            rotation++;
            if (rotation > 3) rotation = 0;
            var res = North;
            North = West;
            West = South;
            South = East;
            East = res;

            var temp = northCollider.transform.position;
            northCollider.transform.position = westCollider.transform.position;
            westCollider.transform.position = southCollider.transform.position;
            southCollider.transform.position = eastCollider.transform.position;
            eastCollider.transform.position = temp;
            
            // TODO rotate matrix view as well
            UpdateMatrix();
        }

        public void Rotate(int position)
        {
            Debug.Assert(position < 4);

            while (rotation != position)
            {
                Rotate();
            }
        }

        #region UnityEvents

        private void Awake()
        {
            UpdateMatrix();
        }

        #endregion
        
        #region GameObjectManipulations

        
        /// <summary>
        /// Called on Tile:Manipulation Ended (set in Unity Inspector)
        /// </summary>
        public void SetCorrectRotation()
        {
            GameObject.Find("GameController").GetComponent<GameControllerScript>().tileControllerScript.RotateDegreesRPC();
        }


        public void DisableGravity()
        {
            GetComponent<Rigidbody>().useGravity = false;
        }

        public void EnableGravity()
        {
            GetComponent<Rigidbody>().useGravity = true;
        }
        
        /// <summary>
        /// Called on Tile:Manipulation Ended (set in Unity Inspector)
        /// </summary>
        // public void SetSnapPosForCurrentTile()
        // {
        //     GameObject.Find("GameController").GetComponent<GameControllerScript>().SetCurrentTileSnapPosition();
        // }

        public void transferTileOwnership(int currentPlayerID)
        {
            photonView.TransferOwnership(PhotonNetwork.PlayerList[currentPlayerID]);
        }

        #endregion

        public override string ToString()
        {
            return $"{Up.ToString()[0]}{Right.ToString()[0]}{Down.ToString()[0]}{Left.ToString()[0]}";
        }

        #region Debug

        public void DebugRotate(int rotation)
        {
            Debug.Log($"Rotated Tile {rotation}");
        }
        
        public void DebugCell(Vector2Int cell)
        {
            Debug.Log($"Tile cell changed {cell}");
        }
        
        public void DebugPlace(Vector2Int cell, int rotation)
        {
            Debug.Log($"Placed Tile {cell}, {rotation}");
        }
        

        #endregion
    }
    

    public static class Extensions
    {
        public static bool HasCity(this Geography geography) => (geography & Geography.City) == Geography.City;
        public static bool HasRoad(this Geography geography) => (geography & Geography.Road) == Geography.Road;
        public static bool HasCityOrRoad(this Geography geography) => (geography & Geography.City) == Geography.City ||
                                                                      (geography & Geography.Road) == Geography.Road;
    
        /// <summary>
        /// Get the simple feature that the subtile represents if a meeple is on it. For example, if a subtile is
        /// CityRoad, a meeple placed on the subtile is on the city.
        /// </summary>
        /// <param name="geography"></param>
        /// <returns></returns>
        public static Geography Simple(this Geography geography)
        {
            // If it is a compound geography with a city, its simple representation is City
            if (geography.HasCity()) return Geography.City;
            
            // If it is a compound geography with a road that DOESN'T have a city, its simple representation is a Road
            if (geography.HasRoad()) return Geography.Road;

            return geography;
        }

        /// <summary>
        /// Is this subtile a feature. Only cities, roads, and cloisters are features.
        /// </summary>
        /// <param name="geography"></param>
        /// <returns></returns>
        public static bool IsFeature(this Geography geography)
        {
            if ((geography & (Geography.City | Geography.Road | Geography.Cloister)) == geography) return true;

            return false;
        }
    }

    /// <summary>
    ///     Geography decides what is contained within each direction. If there is a road going out to the right and the
    ///     rotation is 0 then east will become "road".
    ///
    ///     Represented as a bitmask so that combination tiles (CityRoad) can be tested as City & X == City,
    ///     which returns True for X in [City, CityStream, CityRoad].
    /// </summary>
    [Flags] public enum Geography
    {
        Cloister,
        Village,
        Road,
        Field,
        City,
        Stream,
        CityStream = City + Stream,
        RoadStream = Road + Stream,
        CityRoad = City + Road
    }
}