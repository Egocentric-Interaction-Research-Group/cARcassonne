using System;
using Carcassonne.State;
using Carcassonne.Tiles;
using Photon.Pun;
using UnityEngine;

namespace Carcassonne.Controllers
{
    /// <summary>
    /// Maintains the UI of a tile in play. Responsible for maintaining the position, rotation, etc. of a tile in the
    /// Unity UI.
    /// </summary>
    public class TileUIControllerScript : MonoBehaviourPun
    {
        [SerializeField]
        internal GameControllerScript gameControllerScript;
        
        /// <summary>
        /// Position of the current tile in Unity coordinates
        /// </summary>
        public Vector2 position = new Vector2();

        private TileScript tile => gameControllerScript.state.Tiles.Current;

        public Vector2 RaycastPosition()
        {
            RaycastHit hit;
            var layerMask = 1 << 8;

            Physics.Raycast(tile.gameObject.transform.position, tile.gameObject.transform.TransformDirection(Vector3.down), out hit,
                Mathf.Infinity, layerMask);

            var local = gameControllerScript.table.transform.InverseTransformPoint(hit.point);

            return new Vector2(local.x, local.z);
        }

        public Vector2Int BoardPosition(Vector2 raycastPosition)
        {
            var localPosition = gameControllerScript.stackScript.basePositionTransform.localPosition;
            var position = new Vector2Int(); 
            
            if (raycastPosition.x - localPosition.x > 0)
            {
                position.x = (int) ((raycastPosition.x - localPosition.x) * gameControllerScript.scale + 1f) / 2 + GameRules.BoardSize / 2;
            }
            else
            {
                position.x = (int) ((raycastPosition.x - localPosition.x) * gameControllerScript.scale - 1f) / 2 + GameRules.BoardSize / 2;
            }

            if (raycastPosition.y - localPosition.z > 0)
            {
                position.y = (int) ((raycastPosition.y - localPosition.z) * gameControllerScript.scale + 1f) / 2 + GameRules.BoardSize / 2;
            }
            else
            {
                position.y = (int) ((raycastPosition.y - localPosition.z) * gameControllerScript.scale - 1f) / 2 + GameRules.BoardSize / 2;
            }

            return position;
        }
    }
}