using Carcassonne.Models;
using Carcassonne.Controllers;
using Microsoft.MixedReality.Toolkit.UI;
using Photon.Pun;
using UnityEngine;

namespace Carcassonne.Tiles
{
    [RequireComponent(typeof(Tile))]
    public class TileScript : MonoBehaviourPun
    {
        public Tile tile => GetComponent<Tile>();
        public TileController tileController => FindObjectOfType<TileController>();
        
        /// <summary>
        ///     How many times the tile has been rotated. In standard the rotation is 0, and rotated 4 times it returns to 0.
        /// </summary>
        public int rotation => GetRotation();

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

        public void OnPlace(Tile tile, Vector2Int cell)
        {
            GetComponent<BoxCollider>().enabled = false;
            GetComponent<Rigidbody>().useGravity = false;
            GetComponent<ObjectManipulator>().enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
        }


        /// <summary>
        ///     The method used to rotate the tile. In essence it just cycles the rotation between 1 and 3 (and returns to 0 when
        ///     rotated after 3), and switches the north east south west values clockwise.
        /// </summary>
        public void Rotate()
        {
            tileController.Rotate();
            transform.Rotate(0, 90, 0);

            // var temp = northCollider.transform.position;
            // northCollider.transform.position = westCollider.transform.position;
            // westCollider.transform.position = southCollider.transform.position;
            // southCollider.transform.position = eastCollider.transform.position;
            // eastCollider.transform.position = temp;
        }

        public void Rotate(int rotations)
        {
            Debug.Assert(rotations < 4, $"Position ({rotations}) must be < 4");

            for (int i = 0; i < 4 && rotation != rotations; i++)
            {
                Rotate();
                Debug.Log($"Rotation: {rotation}, Position {rotations}");
            }
            
            Debug.Assert(rotation == rotations, $"The rotation ({rotation}) has not been changed to the specified position ({rotations})");
        }

        public void RotateTo(int orientation)
        {
            Debug.Assert(orientation < 4, $"Position ({orientation}) must be < 4");
            Debug.Log($"Rotating to orientation {orientation}");

            tileController.RotateTo(orientation);
            transform.rotation = Quaternion.Euler(0, orientation * 90, 0); /// It is possible that this is supposed to be in the X direction, not Y.
            
            Debug.Assert(rotation == orientation, $"The rotation ({rotation}) has not been changed to the specified position ({orientation})");
        }

        /// <summary>
        /// Get the rotation of the tile object by comparing the positioning of the North, South, East, and West colliders.
        /// </summary>
        /// <returns></returns>
        private int GetRotation()
        {
            var north2D = new Vector2(northCollider.transform.position.x, northCollider.transform.position.z);
            var south2D = new Vector2(southCollider.transform.position.x, southCollider.transform.position.z);

            var angle = Vector2.SignedAngle(Vector2.right, north2D-south2D);
            Debug.Log($"Angle between South ({south2D}, {southCollider.transform.position}) and North ({north2D}, {northCollider.transform.position}) {angle}");
            
            if(angle % 90 > 5 && angle % 90 < 85)
                Debug.LogWarning($"The tile is not square to the board. The angle between the North and South colliders is {angle} and should be a multiple of 90.");

            if (angle > 45 && angle <= 135)
                return 0;
            if (angle > -45 && angle <= 45)
                return 1;
            if (angle > -135 && angle <= -45)
                return 2;
            return 3;
        }

        #region GameObjectManipulations

        
        /// <summary>
        /// Called on Tile:Manipulation Ended (set in Unity Inspector)
        /// </summary>
        // public void SetCorrectRotation()
        // {
        //     // GameObject.Find("GameController").GetComponent<GameControllerScript>().tileController.RotateDegreesRPC();
        //     throw new NotImplementedException();
        // }
        //
        //
        // public void DisableGravity()
        // {
        //     GetComponent<Rigidbody>().useGravity = false;
        // }
        //
        // public void EnableGravity()
        // {
        //     GetComponent<Rigidbody>().useGravity = true;
        // }

        /// <summary>
        /// Called on Tile:Manipulation Ended (set in Unity Inspector)
        /// </summary>
        // public void SetSnapPosForCurrentTile()
        // {
        //     GameObject.Find("GameController").GetComponent<GameControllerScript>().SetCurrentTileSnapPosition();
        // }
        //
        // public void transferTileOwnership(int currentPlayerID)
        // {
        //     photonView.TransferOwnership(PhotonNetwork.PlayerList[currentPlayerID]);
        // }

        #endregion
        
        public override string ToString()
        {
            return tile.ToString();
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
}