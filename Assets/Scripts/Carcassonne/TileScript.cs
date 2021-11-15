using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Carcassonne
{
    public class TileScript : MonoBehaviourPun
    {
    
        /// <summary>
        ///     Describes the different set of game tiles (used in different versions of gameplay).
        /// </summary>
        public enum TileSet
        {
            Base,
            River
        }
    
        /// <summary>
        ///     Geography decides what is contained within each direction. If there is a road going out to the right and the
        ///     rotation is 0 then east will become "road"
        /// </summary>
        public enum Geography
        {
            Cloister,
            Village,
            Road,
            Grass,
            City,
            Stream,
            CityStream,
            RoadStream,
            CityRoad
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
        ///     Meeple ID (to be used/fixed later)
        /// </summary>
        public int meeple; //Använd senare

        /// <summary>
        ///     The vIndex of the tile. Is applied when placed on the board
        /// </summary>
        public int vIndex;

        public GameObject northCollider, southCollider, westCollider, eastCollider;

        public bool northOcupied, southOcupied, eastOcupied, westOcupied, centerOcupied; //TODO Fix Spelling

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
        /// The matrix representation of this tile. The bottom corner (Left-Down) is 0,0 and the top (Right, Top is (2,2).
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
                case Geography.Grass:
                case Geography.Road:
                case Geography.Stream:
                case Geography.Village:
                    matrix[1, 1] = Center;
                    matrix[0, 0] = Geography.Grass; // NB: Grass here doesn't necessarily mean grass IF you were playing with Farmers. But because we are not considering farmers, it is an easy shortcut.
                    matrix[2, 0] = Geography.Grass;
                    matrix[0, 2] = Geography.Grass;
                    matrix[2, 2] = Geography.Grass;
                    break;
                case Geography.CityRoad:
                case Geography.CityStream:
                case Geography.RoadStream:
                    matrix[1, 1] = Geography.Grass;
                    matrix[0, 0] = matrix[0, 1] == matrix[1, 0] ? matrix[0, 1] : Geography.Grass;
                    matrix[2, 0] = matrix[2, 1] == matrix[1, 0] ? matrix[2, 1] : Geography.Grass;
                    matrix[0, 2] = matrix[0, 1] == matrix[1, 2] ? matrix[0, 1] : Geography.Grass;
                    matrix[2, 2] = matrix[2, 1] == matrix[1, 2] ? matrix[2, 1] : Geography.Grass;
                    break;
            }
            
            // Log debug statement here.
        }

        private void Awake()
        {
            UpdateMatrix();
        }

        public bool IsOccupied(PointScript.Direction direction)  //TODO Fix naming (spelling)
        {
            switch (direction)
            {
                case PointScript.Direction.NORTH:
                    return northOcupied;
                case PointScript.Direction.SOUTH:
                    return southOcupied;
                case PointScript.Direction.EAST:
                    return eastOcupied;
                case PointScript.Direction.WEST:
                    return westOcupied;
                default:
                    return centerOcupied;
            }
        }


        public void occupy(PointScript.Direction direction)
        {
            if (direction == PointScript.Direction.NORTH) northOcupied = true;
            if (direction == PointScript.Direction.SOUTH) southOcupied = true;
            if (direction == PointScript.Direction.EAST) eastOcupied = true;
            if (direction == PointScript.Direction.WEST) westOcupied = true;
            if (direction == PointScript.Direction.CENTER) centerOcupied = true;
            if (Center == getGeographyAt(direction) && direction != PointScript.Direction.CENTER ||
                Center == Geography.City)
            {
                if (getGeographyAt(PointScript.Direction.NORTH) == getGeographyAt(direction)) northOcupied = true;
                if (getGeographyAt(PointScript.Direction.EAST) == getGeographyAt(direction)) eastOcupied = true;
                if (getGeographyAt(PointScript.Direction.SOUTH) == getGeographyAt(direction)) southOcupied = true;
                if (getGeographyAt(PointScript.Direction.WEST) == getGeographyAt(direction)) westOcupied = true;
            }

            if (Center == Geography.City && getGeographyAt(direction) == Geography.City)
                centerOcupied = true;
            else if (Center == Geography.Road && getGeographyAt(direction) == Geography.Road) centerOcupied = true;
        }

        /// <summary>
        ///     Returns the tile geography at a specific direction.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Geography getGeographyAt(PointScript.Direction direction)
        {
            if (direction == PointScript.Direction.NORTH) return North;
            if (direction == PointScript.Direction.SOUTH) return South;
            if (direction == PointScript.Direction.EAST) return East;
            if (direction == PointScript.Direction.WEST)
                return West;
            return Center;
        }

        // public geography[] getGeography()
        // {
        //     var geographies = new geography[4] {North, South, East, West};
        //     return geographies;
        // }

        /// <summary>
        ///     Depending on the ID of the tile it recieves different attributes.
        ///     ID's in tiles are not unique and they share them with other tiles who also recieve the same attributes.
        /// </summary>
        /// <param name="id"></param>
        public void AssignAttributes(int id)
        {
            rotation = 0;
            this.id = id;
            if (id == 1 || id == 2 || id == 3 || id == 4 || id == 5 || id == 6 || id == 12 || id == 17 || id == 25 ||
                id == 26 || id == 27 || id == 28) Up = Geography.Grass;
            if (id == 1 || id == 2 || id == 4 || id == 7 || id == 9 || id == 14 || id == 25 || id == 27)
                Right = Geography.Grass;
            if (id == 1 || id == 3 || id == 7 || id == 8 || id == 12 || id == 13 || id == 15 || id == 17 || id == 18 ||
                id == 20 || id == 22 || id == 26) Down = Geography.Grass;
            if (id == 1 || id == 2 || id == 7 || id == 10 || id == 13 || id == 14 || id == 15 || id == 18 || id == 25)
                Left = Geography.Grass;
            if (id == 6 || id == 29 || id == 30) Up = Geography.Road;
            if (id == 3 || id == 5 || id == 6 || id == 8 || id == 10 || id == 11 || id == 30) Right = Geography.Road;
            if (id == 2 || id == 4 || id == 5 || id == 6 || id == 9 || id == 10 || id == 11 || id == 16 || id == 19 ||
                id == 21 || id == 23 || id == 28 || id == 29 || id == 31) Down = Geography.Road;
            if (id == 3 || id == 4 || id == 5 || id == 6 || id == 8 || id == 9 || id == 11 || id == 16 || id == 19)
                Left = Geography.Road;
            if (id == 7 || id == 8 || id == 9 || id == 10 || id == 11 || id == 13 || id == 14 || id == 15 || id == 16 ||
                id == 18 || id == 19 || id == 20 || id == 21 || id == 22 || id == 23 || id == 24 || id == 31 || id == 32 ||
                id == 33) Up = Geography.City;
            if (id == 12 || id == 13 || id == 15 || id == 16 || id == 17 || id == 18 || id == 19 || id == 20 || id == 21 ||
                id == 22 || id == 23 || id == 24 || id == 33) Right = Geography.City;
            if (id == 14 || id == 24 || id == 32) Down = Geography.City;
            if (id == 12 || id == 17 || id == 20 || id == 21 || id == 22 || id == 23 || id == 24) Left = Geography.City;
            if (id == 26 || id == 28 || id == 29 || id == 31 || id == 32) Right = Geography.Stream;
            if (id == 25 || id == 27 || id == 30 || id == 33) Down = Geography.Stream;
            if (id == 26 || id == 27 || id == 28 || id == 29 || id == 30 || id == 31 || id == 33) Left = Geography.Stream;
            if (id == 1 || id == 2 || id == 28) Center = Geography.Cloister;
            if (id == 3 || id == 4 || id == 8 || id == 9 || id == 10 || id == 29 || id == 30) Center = Geography.Road;
            if (id == 5 || id == 6 || id == 11) Center = Geography.Village;
            if (id == 7 || id == 14 || id == 15 || id == 32) Center = Geography.Grass;
            if (id == 12 || id == 13 || id == 17 || id == 18 || id == 20 || id == 21 || id == 22 || id == 23 || id == 24 ||
                id == 31) Center = Geography.City;
            if (id == 33) Center = Geography.CityStream;
            if (id == 16 || id == 19) Center = Geography.CityRoad;
            if (id == 17 || id == 18 || id == 19 || id == 22 || id == 23 || id == 24)
                shield = true;
            else
                shield = false;

            North = Up;
            East = Right;
            South = Down;
            West = Left;
            AssignTexture(id);
        }

        /// <summary>
        ///     This method assigns the texture correlating to the ID. If the tile has an ID = 1, then its material texture will be
        ///     replaced by the texture stored in the slot for ID 1.
        /// </summary>
        /// <param name="id">The tile ID</param>
        private void AssignTexture(int id)
        {
            var m_Renderer = GetComponentInChildren<Renderer>();
            m_Renderer.material.EnableKeyword("_MainTex");
            GetComponentInChildren<Renderer>().material.SetTexture("_MainTex", textures[id - 1]);
        }

        // public void resetRotation()
        // {
        //     rotation = 0;
        // }

        /// <summary>
        ///     The method used to rotate the tile. In essence it just cycles the rotation between 1 and 3 (and returns to 0 when
        ///     rotated after 3), and switches the north east south west values clockwise.
        /// </summary>
        public void Rotate()
        {
            rotation++;
            if (rotation > 3) rotation = 0;
            //this.transform.Rotate(0.0f, 90.0f, 0.0f, Space.Self);

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


        public void SetCorrectRotation()
        {
            GameObject.Find("GameController").GetComponent<GameControllerScript>().RotateDegreesRPC();
        }


        /// <summary>
        ///     Returns true if the tile has a shield.
        /// </summary>
        /// <returns>if the tile has a shield</returns>
        // public bool HasShield()
        // {
        //     return shield;
        // }

        public void DisableGravity()
        {
            GetComponent<Rigidbody>().useGravity = false;
            GameObject.Find("GameController").GetComponent<GameControllerScript>().SaveEulersOnManipRPC();
        }

        public void EnableGravity()
        {
            GetComponent<Rigidbody>().useGravity = true;
        }

        public void SetSnapPosForCurrentTile()
        {
            GameObject.Find("GameController").GetComponent<GameControllerScript>().SetCurrentTileSnapPosition();
        }

        public void transferTileOwnership(int currentPlayerID)
        {
            photonView.TransferOwnership(PhotonNetwork.PlayerList[currentPlayerID]);
        }
    }
}