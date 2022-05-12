using System.Linq;
using Carcassonne.Models;
using Carcassonne.State;
using Photon.Pun;
using UnityEngine;

namespace Carcassonne.Players
{
    public class ARPlayer : MonoBehaviourPun//, IPunInstantiateMagicCallback
    {
        // public void OnPhotonInstantiate(PhotonMessageInfo info)
        // {
        //     var players = FindObjectsOfType<Player>().ToList();
        //     var id = players.Count - 1; // Zero-indexed
        //     GetComponent<Player>().id = id;
        //
        //     Debug.Log($"Created new player with ID {id}.");
        //
        // }

        [PunRPC]
        public void SetPlayerID(int id)
        {
            GetComponent<Player>().id = id;
        }
    }
}