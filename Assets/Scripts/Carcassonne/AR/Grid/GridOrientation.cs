using Carcassonne.AR.GamePieces;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace UI.Grid
{
    public class OrientationEvent: UnityEvent<int>{}
    
    public class GridOrientation : MonoBehaviourPun
    {

        [FormerlySerializedAs("OnChangeRotation")] [Tooltip("A tile's rotational position has changed.")]
        public UnityEvent<int> OnChangeOrientation = new OrientationEvent();

        public int direction => ARTile.rotation;
        private ARTile ARTile => GetComponent<ARTile>();

        public void OrientToRPC(int o)
        {
            Debug.Assert(o < 4, $"Orientation ({o}) should be less than 4"); 
            Debug.Log($"Orientating to {o}");
            photonView.RPC("OrientTo", RpcTarget.All, o);
        }

        [PunRPC]
        public void OrientTo(int o)
        {
            
            Debug.Log($"Orientating from {direction} to {o}");
            
            var oldOrientation = direction;
            
            ARTile.RotateTo(o);

            if(oldOrientation != direction)
            {
                OnChangeOrientation.Invoke(direction);
            }
        }
    }
}