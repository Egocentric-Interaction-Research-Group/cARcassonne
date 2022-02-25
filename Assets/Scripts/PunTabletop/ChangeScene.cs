using Carcassonne.AR;
using Carcassonne.Controllers;
using Photon.Pun;
using UnityEngine;

namespace PunTabletop
{
    public class ChangeScene : MonoBehaviourPun
    {
        public delegate void ChangeSceneDelegate();
        public GameObject Game;

        private bool isPunEnabled;

        public bool IsPunEnabled
        {
            set => isPunEnabled = value;
        }
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void LoadSceneRPC()
        {
            photonView.RPC("LoadScene", RpcTarget.All);
        }

        [PunRPC]
        public void LoadScene()
        {
            Game.SetActive(true);
            // Game.GetComponentInChildren<GameControllerScript>().startGame = true;
            gameObject.SetActive(false);
        }


        public event ChangeSceneDelegate OnChangeScene;
    }
}
