using Carcassonne.AR.Buttons;
using Carcassonne.Controllers;
using Carcassonne.Models;
using Carcassonne.State;
using Microsoft.MixedReality.Toolkit.UI;
using Photon.Pun;
using UI.Grid;
using UnityEngine;

namespace Carcassonne.AR
{
    public class TileControllerScript : MonoBehaviourPun
    {
        private static int startingPosition = -10;

        public Grid tileGrid;

        public Transform tileParent;
        public Tile tilePrefab;
        public int startingTileID;
        public byte tileGroup;
        public ConfirmButton confirmButton;

        private TurnController turnController => GetComponent<TurnController>();

        public void OnDraw(Tile tile)
        {
            Debug.Log("TileControllerScript: Drawing tile.");
            tile.transform.SetParent(tileGrid.transform);
            tile.gameObject.SetActive(true);
            tile.GetComponent<Rigidbody>().isKinematic = false;
            tile.GetComponent<Rigidbody>().useGravity = true;
            tile.GetComponentInChildren<MeshRenderer>().enabled = true;
            tile.GetComponent<BoxCollider>().enabled = true;
            
            if(turnController.IsLocalTurn()){
                Debug.Log("TileControllerScript: Local turn. Sending RPC Draw command.");
                
                photonView.RPC("RPCDraw", RpcTarget.Others); 
                tile.GetComponent<GridPosition>().MoveToRPC(new Vector2Int(startingPosition, startingPosition));

                if (turnController.IsLocalHumanTurn())
                {
                    Debug.Log("TileControllerScript: Local human turn. Enabling keyboard move, rotate, and hand manipulation.");
                    tile.GetComponent<GridKeyboardMovable>().enabled = true;
                    tile.GetComponent<GridKeyboardRotatable>().enabled = true;
                    tile.GetComponent<ObjectManipulator>().enabled = true;
                    
                    tile.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
                }
            }
        }

        public void OnPlace(Tile tile, Vector2Int cell)
        {
            tile.GetComponent<BoxCollider>().enabled = false;
            tile.GetComponent<GridKeyboardMovable>().enabled = false;
            tile.GetComponent<GridKeyboardRotatable>().enabled = false;
            tile.GetComponent<Rigidbody>().useGravity = false;
            tile.GetComponent<ObjectManipulator>().enabled = false;
            tile.GetComponent<Rigidbody>().isKinematic = true;
        }

        [PunRPC]
        public void PlaceTile()
        {
            var tileController = GetComponent<TileController>();
            var state = GetComponent<GameState>();
            var tile = state.Tiles.Current;
            var gridPosition = tile.GetComponent<GridPosition>();

            // Invoke the TileController's place call.
            tileController.Place(gridPosition.cell);
        }
        
        public void OnRotate(){
        }
        public void OnDiscard(){
        }
        public void OnPlace(){
        }
        public void OnInvalidPlace(){
        }
        
        
        #region RPC
        
        [PunRPC]
        public void RPCDraw()
        {
            GetComponent<TileController>().Draw();
        }
        public void RPCRotate(){
        }
        public void RPCDiscard(){
        }
        public void RPCPlace(){
        }
        public void RPCInvalidPlace(){
        }
        #endregion
    }
}