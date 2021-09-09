using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public GameObject MeeplePrefab;
    public List<Player> players = new List<Player>();


    public void CreatePlayer(int id, string name, Material playerMat, GameObject photonUser)
    {
        players.Add(new Player(id, name, MeeplePrefab, playerMat, photonUser));
        // players.AddLast(new Player(name, color));
    }

    public List<Player> GetPlayers()
    {
        return players;
    }

    public Player GetPlayer(int index)
    {
        index = 2;
        return players[index];
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

        public Player(int id, string name, GameObject MeeplePrefab, Material playerMat, GameObject photonUser)
        {
            this.id = id;
            playerName = name;
            mat = playerMat;
            mat.name = playerName;
            this.photonUser = photonUser;

            score = 0;
            //this.meeples = generateMeeples(this, MeeplePrefab);
        }

        public Material GetMaterial()
        {
            return mat;
        }

        public int getID()
        {
            return id;
        }

        public string GetPlayerName()
        {
            return playerName;
        }

        public void SetPlayerName(string playerName)
        {
            this.playerName = playerName;
        }

        public int GetPlayerScore()
        {
            return score;
        }

        public void SetPlayerScore(int playerScore)
        {
            score = playerScore;
        }

        public void addScore(int scoreToAdd)
        {
            score = score + scoreToAdd;
        }

        public Color32 GetPlayerColor()
        {
            return playerColor;
        }

        public void SetPlayerColor(Color32 playerColor)
        {
            this.playerColor = playerColor;
        }

        public int GetFreeMeeples()
        {
            var amount = meeples.Length;
            foreach (var item in meeples)
                if (item.GetComponent<MeepleScript>().free != true)
                    amount -= 1;
            return amount;
        }

        /// <summary>
        ///     Generates 8 meeples for the player.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="MeeplePrefab"></param>
        /// <returns></returns>
        private GameObject[] generateMeeples(Player player, GameObject MeeplePrefab)
        {
            var res = new GameObject[8];
            for (var i = 0; i < 8; i++)
            {
                var meeple = PhotonNetwork.Instantiate(MeeplePrefab.name, new Vector3(20, 1, 20), Quaternion.identity);
                meeple.GetComponent<MeepleScript>().createByPlayer(player);
                meeple.transform.parent = GameObject.Find("BaseTile").transform;
                res[i] = meeple;
            }

            return res;
        }
    }
}