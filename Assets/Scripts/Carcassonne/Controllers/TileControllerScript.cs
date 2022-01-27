using System.Numerics;
using Carcassonne.State;
using Carcassonne.Utilities;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Tilemaps;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Carcassonne.Controllers
{
    /// <summary>
    /// Maintains the state of a tile that is in play. Responsible for maintining rotation and location of tile and
    /// setting the state when the tile is placed.
    /// </summary>
    public class TileControllerScript : MonoBehaviourPun
    {
        [SerializeField]
        internal GameControllerScript gameControllerScript;

        [SerializeField]
        public Tilemap tilemap;
        
        public GameState state;
        public TileState tiles => state.Tiles;
        
        /// <summary>
        /// Position of the current tile in board coordinates.
        /// </summary>
        public Vector2Int position = new Vector2Int();
        
        [Header("Effects and Positioning")]
        public ParticleSystem drawTileEffect;
        // Tile Spawn position has to be on a grid with the base tile.
        public GameObject tileSpawnPosition;
        public GameObject drawTile;
        
        public TileControllerScript(GameControllerScript gameControllerScript)
        {
            this.gameControllerScript = gameControllerScript;
        }

        /// <summary>
        /// Perform a rotation of a tile, if in the correct phase. Always sets tile to the closest 90 degree angle greater than now.
        /// </summary>
        /// <param name="gameControllerScript"></param>
        [PunRPC]
        public void RotateTile()
        {
            //TODO Why are we checking the phase anyways? I added NewTurn because this was causing the check for valid new piece to fail.
            if (gameControllerScript.state.phase == Phase.TileDrawn || gameControllerScript.state.phase == Phase.NewTurn)
            {
                tiles.Current.Rotate();
                
                tiles.Current.gameObject.transform.Rotate(0.0f, 90.0f, 0.0f, Space.Self);
            }
            else
            {
                Debug.LogWarning($"Tile not rotated because call came in {gameControllerScript.state.phase} and rotation is only valid during TileDrawn and NewTurn.");
            }
        }

        /// <summary>
        /// Reset tile rotation internal state. WARNING: This only deals with the internal state. It does not rotate the tile in the view.
        /// </summary>
        public void ResetTileRotation()
        {
            tiles.Current.Rotate(0);
        }

        [PunRPC]
        public void MoveTile(Vector3 direction)
        {
            tiles.Current.transform.position += direction;
            
            gameControllerScript.tileUIController.position = gameControllerScript.tileUIController.RaycastPosition();
            gameControllerScript.tileControllerScript.position = gameControllerScript.tileUIController.BoardPosition(gameControllerScript.tileUIController.position);
        }

        #region Photon
        /// <summary>
        /// Called on Tile:Manipulation Started (set in Unity Inspector)
        /// </summary>
        public void ChangeCurrentTileOwnership()
        {
            if (tiles.Current.gameObject.GetComponent<PhotonView>().Owner.NickName != (gameControllerScript.currentPlayer.id + 1).ToString())
                tiles.Current.transferTileOwnership(gameControllerScript.currentPlayer.id);
        }
        

        public void KeyboardRotateRPC()
        {
            
        }
        
    
        /// <summary>
        /// Initiate a tile rotation across the network, if the local player has control of the tile.
        /// </summary>
        /// <param name="gameControllerScript"></param>
        public void RotateTileRPC()
        {
            if (gameControllerScript.CurrentPlayerIsLocal)
                photonView.RPC("RotateTile", RpcTarget.All);
        }

        /// <summary>
        /// Move tile according to the direction.
        /// </summary>
        /// <param name="direction">Direction to move tile in tile coordinates.</param>
        public void MoveTileRPC(Vector2Int direction)
        {
            var boardDirection = new Vector3(direction.x, 0, direction.y) * Coordinates.BoardToUnityScale;
            Debug.Log($"Moving to {direction} ({boardDirection})");
            photonView.RPC("MoveTile", RpcTarget.All, boardDirection);
        }
        
        public void RotateDegreesRPC()
        {
            photonView.RPC("RotateDegrees", RpcTarget.All);
        }

        #endregion
        
        #region UI

        public void ActivateCurrent()
        {
            System.Diagnostics.Debug.Assert(tiles.Current.gameObject != null, nameof(tiles.Current.gameObject) + " != null");
            
            var tileObject = tiles.Current.gameObject; 
            tileObject.transform.parent = gameControllerScript.table.transform;
            tileObject.transform.rotation = gameControllerScript.table.transform.rotation;
            tileObject.transform.position = tileSpawnPosition.transform.position;
            
            ActivateCurrentPhysics();
        }
        
        private void ActivateCurrentPhysics()
        {
            tiles.Current.gameObject.GetComponentInChildren<MeshRenderer>().enabled = true;
            tiles.Current.gameObject.GetComponentInChildren<Collider>().enabled = true;
            tiles.Current.gameObject.GetComponentInChildren<Rigidbody>().useGravity = true;
            gameControllerScript.smokeEffect.Play();
        }
        
        [PunRPC]
        public void RotateDegrees()
        {
            var angles = tiles.Current.gameObject.transform.localEulerAngles;
            var rotation = GetRotationFromAngle(angles.y);
            
            // Set the snap angle and snap the piece
            angles.y = rotation * 90;
            tiles.Current.gameObject.transform.localEulerAngles = angles;
            
            // Set the internal model
            tiles.Current.Rotate(rotation);
            
            Debug.Log($"Tile {tiles.Current} transformed to {angles.y} (Rotation {rotation}).");
        }

        /// <summary>
        /// Calculates a rotation number (0-3) from an Euler angle in the y axis.
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        private int GetRotationFromAngle(float angle)
        {
            int rotate = (int)(angle) / 90;
            if (angle % 90 > 45)
                rotate += 1;
            return rotate % 4;
        }

        #endregion
    }
}