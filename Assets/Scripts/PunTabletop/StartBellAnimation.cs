using Carcassonne.AR;
using Carcassonne.Controllers;
using Carcassonne.State;
using Photon.Pun;
using UnityEngine;

namespace PunTabletop
{
    public class StartBellAnimation : MonoBehaviourPun
    {
        public GameObject gameController;
        void Start()
        {
        
        }

        public void StartAniRPC()
        {
            if(gameController.GetComponent<GameControllerScript>().gameController.state.phase == Phase.TileDown || 
               gameController.GetComponent<GameControllerScript>().gameController.state.phase == Phase.MeepleDown)
            {
                if(PhotonNetwork.LocalPlayer.NickName == (gameController.GetComponent<GameControllerScript>().currentPlayer.id + 1).ToString())
                {
                    photonView.RPC("StartAni", RpcTarget.All);
                }
            }
        }

        [PunRPC]
        public void StartAni()
        {
            Animation animation = gameObject.GetComponent<Animation>();
            AudioSource bellAudio = gameObject.GetComponent<AudioSource>();
            animation.Play();
            bellAudio.Play();
        }
    }
}
