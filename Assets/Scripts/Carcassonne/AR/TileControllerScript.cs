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

        public void OnDraw(Tile tile)
        {
            tile.transform.SetParent(tileGrid.transform);
            tile.gameObject.SetActive(true);
            tile.GetComponent<BoxCollider>().enabled = true;
            tile.GetComponent<GridKeyboardMovable>().enabled = true;
            tile.GetComponent<GridKeyboardRotatable>().enabled = true;
            tile.GetComponent<Rigidbody>().useGravity = true;
            tile.GetComponent<ObjectManipulator>().enabled = true;
            tile.GetComponent<Rigidbody>().isKinematic = false;
            tile.GetComponentInChildren<MeshRenderer>().enabled = true;

            tile.GetComponent<GridPosition>().MoveToRPC(new Vector2Int(startingPosition, startingPosition));
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
    }
}