using Carcassonne.State;
using JetBrains.Annotations;
using Photon.Pun;
using UnityEngine;

namespace Carcassonne
{
    public class TileControllerScript : MonoBehaviourPun
    {
        [SerializeField]
        internal GameControllerScript gameControllerScript;
        public ParticleSystem drawTileEffect;
        public TileState tiles;
        
        [CanBeNull]
        public GameObject currentTile
        {
            get
            {
                if (tiles.Current is null)
                    return null;
                return tiles.Current.gameObject;
            }
            set => tiles.Current = value.GetComponent<TileScript>();
        }

        public GameObject drawTile;
        // Tile Spawn position has to be on a grid with the base tile.
        public GameObject tileSpawnPosition;
        public float fTileAimX;
        public float fTileAimZ;

        public TileControllerScript(GameControllerScript gameControllerScript)
        {
            this.gameControllerScript = gameControllerScript;
        }
        
        /// <summary>
        /// Called on Tile:Manipulation Started (set in Unity Inspector)
        /// </summary>
        public void ChangeCurrentTileOwnership()
        {
            if (currentTile.GetComponent<PhotonView>().Owner.NickName != (gameControllerScript.currentPlayer.getID() + 1).ToString())
                currentTile.GetComponent<TileScript>().transferTileOwnership(gameControllerScript.currentPlayer.getID());
        }

        public void ActivateCurrentTile()
        {
            System.Diagnostics.Debug.Assert(currentTile != null, nameof(currentTile) + " != null");
            
            currentTile.GetComponentInChildren<MeshRenderer>().enabled = true;
            currentTile.GetComponentInChildren<Collider>().enabled = true;
            currentTile.GetComponentInChildren<Rigidbody>().useGravity = true;
            currentTile.transform.parent = gameControllerScript.table.transform;
            currentTile.transform.rotation = gameControllerScript.table.transform.rotation;
            currentTile.transform.position = tileSpawnPosition.transform.position;
            gameControllerScript.smokeEffect.Play();
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
        /// Perform a rotation of a tile, if in the correct phase. Always sets tile to the closest 90 degree angle greater than now.
        /// </summary>
        /// <param name="gameControllerScript"></param>
        [PunRPC]
        public void RotateTile()
        {
            //TODO Why are we checking the phase anyways? I added NewTurn because this was causing the check for valid new piece to fail.
            if (gameControllerScript.gameState.phase == Phase.TileDrawn || gameControllerScript.gameState.phase == Phase.NewTurn)
            {
                tiles.Current.Rotate();
                
                currentTile.transform.Rotate(0.0f, 90.0f, 0.0f, Space.Self);
            }
            else
            {
                Debug.LogWarning($"Tile not rotated because call came in {gameControllerScript.gameState.phase} and rotation is only valid during TileDrawn and NewTurn.");
            }
        }

        /// <summary>
        /// Reset tile rotation internal state. WARNING: This only deals with the internal state. It does not rotate the tile in the view.
        /// </summary>
        public void ResetTileRotation()
        {
            tiles.Current.Rotate(0);
        }

        /// <summary>
        /// Move tile according to the direction.
        /// </summary>
        /// <param name="direction">Direction to move tile in tile coordinates.</param>
        public void MoveTileRPC(Vector2Int direction)
        {
            var boardDirection = new Vector3(direction.x, 0, direction.y) * 0.033f;
            Debug.Log($"Moving to {direction} ({boardDirection})");
            photonView.RPC("MoveTile", RpcTarget.All, boardDirection);
        }

        [PunRPC]
        public void MoveTile(Vector3 direction)
        {
            tiles.Current.transform.position += direction;
            
            gameControllerScript.CurrentTileRaycastPosition();
        }

        public void RotateDegreesRPC()
        {
            photonView.RPC("RotateDegrees", RpcTarget.All);
        }

        [PunRPC]
        public void RotateDegrees()
        {
            var angles = currentTile.transform.localEulerAngles;
            var rotation = GetRotationFromAngle(angles.y);
            
            // Set the snap angle and snap the piece
            angles.y = rotation * 90;
            currentTile.transform.localEulerAngles = angles;
            
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

        public Vector3 BoardToUnity(Vector2Int board)
        {
            return new Vector3((board.x - GameRules.BoardSize / 2) * 0.033f, 0, (board.y - GameRules.BoardSize / 2) * 0.033f);
        }
    }
}