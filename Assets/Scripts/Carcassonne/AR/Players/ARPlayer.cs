using System.Linq;
using Carcassonne.Models;
using Carcassonne.State;
using Photon.Pun;
using UnityEngine;

namespace Carcassonne.Players
{
    public class ARPlayer : MonoBehaviourPun, IPunInstantiateMagicCallback
    {
        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            var id = GetComponent<PhotonView>().CreatorActorNr; 
            GetComponent<Player>().id = id;
        
            Debug.Log($"Created new player with ID {id}.");
        
        }

        // [PunRPC]
        // public void SetPlayerID(int id)
        // {
        //     GetComponent<Player>().id = id;
        // }
    }
}