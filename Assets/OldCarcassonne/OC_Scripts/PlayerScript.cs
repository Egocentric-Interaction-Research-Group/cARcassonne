using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public List<Player> players = new List<Player>();

    public GameObject MeeplePrefab;




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
        private Material mat;
        private int score;
        private int id;
        private string playerName;
        private Color32 playerColor;
        public GameObject[] meeples;
        public GameObject photonUser;

        public Material GetMaterial()
        {
            return mat;
        }
        public Player(int id, string name, GameObject MeeplePrefab, Material playerMat, GameObject photonUser)
        {
            this.id = id;
            this.playerName = name;
            mat = playerMat;
            mat.name = playerName;
            this.photonUser = photonUser;

            this.score = 0;
            //this.meeples = generateMeeples(this, MeeplePrefab);
        }

        public int getID()
        {
            return id;
        }
       
        public string GetPlayerName()
        {
            return playerName;
        }
        public void SetPlayerName(String playerName)
        {
            this.playerName = playerName;
        }

        public int GetPlayerScore()
        {
            return score;
        }
        public void SetPlayerScore(int playerScore)
        {
            this.score = playerScore;
        }
        public void addScore(int scoreToAdd)
        {
            this.score = this.score + scoreToAdd;
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
            int amount = meeples.Length;
            foreach (var item in meeples)
            {
                if(item.GetComponent<MeepleScript>().free != true)
                {
                    amount -= 1;
                }
            }
            return amount;
        }
        
        /// <summary>
        /// Generates 8 meeples for the player.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="MeeplePrefab"></param>
        /// <returns></returns>
        private GameObject[] generateMeeples(Player player, GameObject MeeplePrefab)
        {
            GameObject[] res = new GameObject[8];
            for (int i = 0; i < 8; i++)
            {
                GameObject meeple = PhotonNetwork.Instantiate(MeeplePrefab.name, new Vector3(20, 1, 20), Quaternion.identity);
                meeple.GetComponent<MeepleScript>().createByPlayer(player);
                meeple.transform.parent = GameObject.Find("BaseTile").transform;
                res[i] = meeple;

            }
            return res;
        }
    }


}
