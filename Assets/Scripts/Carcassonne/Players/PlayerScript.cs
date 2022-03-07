using System;
using System.Collections.Generic;
using Carcassonne.AI;
using Carcassonne.AR;
using Carcassonne.Controllers;
using Carcassonne.Meeples;
using Carcassonne.Models;
using Carcassonne.State;
using ExitGames.Client.Photon.StructWrapping;
using MRTK.Tutorials.MultiUserCapabilities;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

namespace Carcassonne.Players
{

    /// <summary>
    /// The Gameplay AI team will likely have to extract a base class from this so that there are Players who do not have
    /// PhotonUsers.
    /// </summary>
    [RequireComponent(typeof(Player))]
    public class PlayerScript : MonoBehaviour
    {
        public int id => player.id;
        // private Material mat;
        public List<Meeple> meeples => meepleState.MeeplesForPlayer(player);
        // public GameObject photonUser => gameObject;
        // private Color32 playerColor;
        // private string playerName;
        public int score => player.score;
        public bool IsLocal => GetComponent<PhotonUser>().IsLocal;
        public string Name => player.name;
        
        public GameObject ai;

        private GameState state;

        private void Awake()
        {
        }

        public MeepleState meepleState => state.Meeples;

        public Player player => GetComponent<Player>();
        // public PhotonPlayer photonPlayer;
        
        /// <summary>
        /// Set up the player.
        /// 
        /// Within this function, the player creates its own Meeples. I *think* this means each client will instantiate
        /// its own meeples and therefore own its own meeples (in PUN), but I'm not 100% sure of that, so this should be
        /// tested.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="photonPlayer"></param>
        public void OnGameStart()//string name)
        {
            state = FindObjectOfType<GameState>();
            Debug.Assert(state != null, "State is null in PlayerScript");
            
            Debug.Log($"Game started for Player {id}");
            // this.id = id;
            player.name = GetComponent<PhotonUser>().username; //name;
            // mat = playerMat;
            // mat.name = playerName;
            // this.photonPlayer = photonPlayer;

            if (player.isAI)
            {
                GameObject aiObj = Instantiate(ai, transform);
                aiObj.GetComponent<CarcassonneAgent>().wrapper.player = GetComponent<Player>();
                aiObj.SetActive(true);
            }
        }
        
        // public int Score
        // {
        //     get { return score; }
        //     set { score = value; }
        // }

        public int AmountOfFreeMeeples()
        {
            return meeples.Count;
        }

        public static PlayerScript Get(Player p)
        {
            return p.GetComponent<PlayerScript>();
        }
    }
}
