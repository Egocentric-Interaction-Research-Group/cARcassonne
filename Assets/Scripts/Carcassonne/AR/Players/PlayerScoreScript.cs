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
        public Materials materials;
        public GameState state;
        public GameControllerScript controller;

        public int[] materialIndex = {0, 1, 2, 3};

        private TextMeshPro scoreText => transform.GetComponentsInChildren<TextMeshPro>()[0];
        private TextMeshPro playerText => transform.GetComponentsInChildren<TextMeshPro>()[1];

        public Player player;

        private bool m_IsLocal;

        private void Start()
        {
            state = FindObjectOfType<GameState>();
            controller = state.GetComponent<GameControllerScript>();
        }

        public void UpdateScore()
        {
            Debug.Assert(state != null, "State is null");

            Debug.Log($"Updating the score for Player {player}");
            scoreText.text = "Score: " + player.score;
        }

        public void UpdateCurrentPlayer()
        {
            var IsCurrent = player == state.Players.Current;

            Debug.Log($"Updating player {player.id}: Current {IsCurrent}, Local {m_IsLocal}");
            
            var text = $"Player {player.id}";
            text += m_IsLocal ? " (You)" : "";
            text += IsCurrent ? " *" : "";
            playerText.text = text;
        }

        public void ChangeMaterial()
        {
            var playerIndex = state.Players.All.IndexOf(state.Players.Current);
            GetComponentInChildren<MeshRenderer>().material = materials.playerMaterials[materialIndex[playerIndex]];
        }

        public void SetLocal()
        {
            playerText.text = $"Player {player.id} (You)";
            m_IsLocal = true;
        }

        /// <summary>
        /// Deactivate HUD if it is unused, update the score, and update the text if it is local.
        /// </summary>
        public void OnGameStart()
        {
            state = FindObjectOfType<GameState>(); // This isn't a great solution
            Debug.Assert(state != null, "State is null");

            Debug.Log($"Updating score for player {player.id}.");
            UpdateScore();
            if (player && player.GetComponent<PhotonUser>() && player.GetComponent<PhotonUser>().IsLocal)
            {
                SetLocal();
            }
        }
    }
}