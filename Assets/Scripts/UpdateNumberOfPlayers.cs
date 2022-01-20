using Photon.Pun;
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
        if (PhotonNetwork.CurrentRoom != null)
        {
            numberOfPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
            GetComponent<TextMeshPro>().text = numberOfPlayers.ToString();
        }
    }
}
