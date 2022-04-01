using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.Grid
{
    public class GridKeyboardMovable : MonoBehaviourPun
    {
        private GridPosition position => GetComponent<GridPosition>();
        private UnityEngine.Grid grid => position.grid;
        private Vector2Int cell
        {
            get { return position.cell; }
            set { position.cell = value; }
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if( keyboard != null && photonView.IsMine)
            {
                var direction = Vector2Int.zero;
                if (keyboard.jKey.wasPressedThisFrame) direction += Vector2Int.left;
                if (keyboard.lKey.wasPressedThisFrame) direction += Vector2Int.right;
                if (keyboard.iKey.wasPressedThisFrame) direction += Vector2Int.up;
                if (keyboard.kKey.wasPressedThisFrame) direction += Vector2Int.down;

                if (direction != Vector2Int.zero) position.MoveToRPC(cell + direction);
            }
        }
        

    }
}