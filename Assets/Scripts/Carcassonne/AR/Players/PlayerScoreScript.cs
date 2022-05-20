using System.Linq;
using Carcassonne.AR;
using Carcassonne.Models;
using Carcassonne.State;
using MRTK.Tutorials.MultiUserCapabilities;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace Carcassonne.Players
{
    public class PlayerScoreScript : MonoBehaviourPun
    {
        /// <summary>
        /// Zero-indexed player number
        /// </summary>
        public int playerNumber;
        public Materials materials;
        public GameState state;

        public int[] materialIndex = {0, 1, 2, 3};

        private TextMeshPro scoreText => transform.GetComponentsInChildren<TextMeshPro>()[0];
        private TextMeshPro playerText => transform.GetComponentsInChildren<TextMeshPro>()[1];

        private Player player => state.Players.All[playerNumber];

        private void Start()
        {
            state = FindObjectOfType<GameState>();
        }

        public void UpdateScore()
        {
            Debug.Assert(state != null, "State is null");

            if (playerNumber < state.Players.All.Count)
            {
                Debug.Log($"Updating the score for Player {playerNumber} of {state.Players.All.Count}");
                // Debug.Assert(state.Players.All.Count > playerNumber,
                //     $"Player number ({playerNumber}) is greater than the length of the player list ({state.Players.All.Count})");
                scoreText.text = "Score: " + player.score;
            }
        }

        public void NewTurn()
        {
            
        }

        public void ChangeMaterial()
        {
            var playerIndex = state.Players.All.IndexOf(state.Players.Current);
            GetComponentInChildren<MeshRenderer>().material = materials.playerMaterials[materialIndex[playerIndex]];
        }

        public void SetLocal()
        {
            playerText.text = $"Player {playerNumber} (You)";
        }

        /// <summary>
        /// Deactivate HUD if it is unused, update the score, and update the text if it is local.
        /// </summary>
        public void OnGameStart()
        {
            state = FindObjectOfType<GameState>(); // This isn't a great solution
            Debug.Assert(state != null, "State is null");

            Debug.Log($"PlayerScoreScript {playerNumber} got GameStart signal.");
            var nPlayers = PhotonNetwork.PlayerList.Length;

            if (playerNumber >= nPlayers)
            {
                this.gameObject.SetActive(false);
                return;
            }
            
            Debug.Log($"Updating score for player {playerNumber} of {nPlayers}.");
            UpdateScore();
            var player = state.Players.All.SingleOrDefault(p => p.id == playerNumber);
            if (player && player.GetComponent<PhotonUser>().IsLocal)
            {
                SetLocal();
            }
        }
    }
}