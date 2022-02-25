using System;
using Carcassonne.Controllers;
using Carcassonne.Tiles;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace UI.Grid
{
    public class CellEvent: UnityEvent<Vector2Int>{}
    public class PlaceEvent: UnityEvent<Vector2Int>{}
    
    /// <summary>
    /// Manages the grid-based positioning of the parent GameObject in a PUN-based
    /// multiplayer game.
    ///
    /// Allows for positioning and rotation of an object within the grid framework.
    /// </summary>
    public class GridPosition : MonoBehaviourPun
    {
        public UnityEngine.Grid grid;
        public Vector2Int cell;

        #region Events

        [Tooltip("A tile has entered a new cell.")]
        public UnityEvent<Vector2Int> OnChangeCell = new CellEvent();

        [Tooltip("A tile has been released from being picked up and placed on a cell.")]
        public UnityEvent<Vector2Int> OnPlace = new PlaceEvent();

        #endregion

        
        /// <summary>
        /// Move tile according to the direction.
        /// </summary>
        /// <param name="direction">Direction to move tile in tile coordinates.</param>
        public void MoveToRPC(Vector2Int direction)
        {
            photonView.RPC("MoveTo", RpcTarget.All, (Vector2) direction);
        }
        
        [PunRPC]
        public void MoveTo(Vector2 cell)
        {
            var oldCell = this.cell;
            this.cell = Vector2Int.RoundToInt(cell);
            transform.position = grid.GetCellCenterWorld((Vector3Int) this.cell);
            
            if( oldCell != this.cell )
            {
                OnChangeCell.Invoke(this.cell);
            }
        }
    }
}