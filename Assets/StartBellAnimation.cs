using Photon.Pun;
using UnityEngine;

public class StartBellAnimation : MonoBehaviourPun
{
    public GameObject gameController;
    void Start()
    {
        
    }

    public void StartAniRPC()
    {
        if(gameController.GetComponent<GameControllerScript>().state == GameControllerScript.GameStates.TileDown || 
            gameController.GetComponent<GameControllerScript>().state == GameControllerScript.GameStates.MeepleDown)
        {
            if(PhotonNetwork.LocalPlayer.NickName == (gameController.GetComponent<GameControllerScript>().currentPlayer.getID() + 1).ToString())
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
