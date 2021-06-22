using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpdateNumberOfPlayers : MonoBehaviour
{
    // Start is called before the first frame update
    int numberOfPlayers;
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
        numberOfPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
        GetComponent<TextMeshPro>().text = numberOfPlayers.ToString();
    }
}
