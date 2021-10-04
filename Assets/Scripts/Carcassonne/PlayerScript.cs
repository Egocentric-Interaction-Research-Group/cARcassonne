using System.Collections.Generic;
using UnityEngine;

namespace Carcassonne
{
    public class PlayerScript : MonoBehaviour
    {
        public GameObject MeeplePrefab;
        public List<Player> players = new List<Player>();

        public void CreatePlayer(int id, string name, Material playerMat, GameObject photonUser)
        {
            players.Add(new Player(id, name, playerMat, photonUser));
            // players.AddLast(new Player(name, color));
        }

        public List<Player> GetPlayers()
        {
            return players;
        }

        public class Player
        {
            private readonly int id;
            private readonly Material mat;
            public GameObject[] meeples;
            public GameObject photonUser;
            private Color32 playerColor;
            private string playerName;
            private int score;

            public Player(int id, string name, Material playerMat, GameObject photonUser)
            {
                this.id = id;
                playerName = name;
                mat = playerMat;
                mat.name = playerName;
                this.photonUser = photonUser;

                score = 0;
            }

            public int getID()
            {
                return id;
            }

            public int GetPlayerScore()
            {
                return score;
            }

            public void SetPlayerScore(int playerScore)
            {
                score = playerScore;
            }
        }
    }
}