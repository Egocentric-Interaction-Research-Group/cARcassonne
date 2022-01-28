using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Grid
{
    public class RotationEvent: UnityEvent<int>{}
    public class CellEvent: UnityEvent<Vector2Int>{}
    public class PlaceEvent: UnityEvent<Vector2Int, int>{}
    
    public class GridPosition : MonoBehaviourPun
    {
        public UnityEngine.Grid grid;
        public Vector2Int cell;
        public int rotation;
        
        #region Events

        public UnityEvent<Vector2Int> OnChangeCell = new CellEvent();
        public UnityEvent<int> OnChangeRotation = new RotationEvent();
        public UnityEvent<Vector2Int,int> OnPlace = new PlaceEvent();

        #endregion

        
        /// <summary>
        /// Move tile according to the direction.
        /// </summary>
        /// <param name="direction">Direction to move tile in tile coordinates.</param>
        public void MoveToRPC(Vector2Int direction)
        {
            // throw new NotImplementedException();
            // var boardDirection = new Vector3(direction.x, 0, direction.y) * Coordinates.BoardToUnityScale;
            // Debug.Log($"Moving to {direction} ({boardDirection})");
            // photonView.RPC("MoveTile", RpcTarget.All, boardDirection);
            photonView.RPC("MoveTo", RpcTarget.All, (Vector2) direction);
        }
        
        [PunRPC]
        public void MoveTo(Vector2 direction)
        {
            cell = Vector2Int.RoundToInt(direction);
            transform.position = grid.GetCellCenterWorld((Vector3Int) cell);
            
            OnChangeCell.Invoke(cell);
        }

        public void RotateToRPC(int rotation)
        {
            throw new NotImplementedException();
        }
    }
}