using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class MeepleControllerScript : MonoBehaviourPun
{
    
    [SerializeField]
    internal GameControllerScript gameControllerScript;
    [HideInInspector] public List<MeepleScript> MeeplesInCity;
    public float fMeepleAimX; //TODO Make Private
    public float fMeepleAimZ; //TODO Make Private

    public MeepleControllerScript(GameControllerScript gameControllerScript)
    {
        this.gameControllerScript = gameControllerScript;
    }

    public void DrawMeepleRPC()
    {
        if (PhotonNetwork.LocalPlayer.NickName == (gameControllerScript.currentPlayer.getID() + 1).ToString()) gameControllerScript.photonView.RPC("DrawMeeple", RpcTarget.All);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}