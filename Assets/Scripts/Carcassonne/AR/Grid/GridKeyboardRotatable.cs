using Photon.Pun;
using UnityEngine.InputSystem;

namespace UI.Grid
{
    public class GridKeyboardRotatable : MonoBehaviourPun
    {
        
        private GridOrientation orientation => GetComponent<GridOrientation>();

        // private int Orientation
        // {
        //     get { return orientation.orientation; }
        //     // set { position.orientation = value; }
        // }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if( keyboard != null && photonView.IsMine)
            {
                if (keyboard.rKey.wasReleasedThisFrame) orientation.OrientToRPC((orientation.direction + 1) % 4 );
            }
        }
    }
}