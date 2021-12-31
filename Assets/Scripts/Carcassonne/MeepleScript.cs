using System;
using System.Linq;
using Photon.Pun;
using UnityEngine;

namespace Carcassonne
{
    public class MeepleScript : MonoBehaviourPun
    {
        // Start is called before the first frame update
        // public Material[] materials = new Material[5];
        // public Vector2Int direction;
        // public TileScript.Geography geography;
        // public bool free;
        //
        // public int x, z;

        #region Legacy

        private const bool LegacyDepricationError = false;

        [ObsoleteAttribute("This property is obsolete. Find this in the game state instead.", LegacyDepricationError)]
        public Vector2Int direction => GameObject.Find("GameController").GetComponent<GameControllerScript>().
            gameState.Meeples.Placement.Single(kvp => kvp.Value.Meeple == this).Value.Direction;

        [ObsoleteAttribute("This property is obsolete. Find this in the game state instead.", LegacyDepricationError)]
        public Vector2Int position => GameObject.Find("GameController").GetComponent<GameControllerScript>().gameState
            .Meeples.Placement.Single(kvp => kvp.Value.Meeple == this).Key;
        
        [ObsoleteAttribute("This property is obsolete. Find this in the game state instead.", LegacyDepricationError)]
        public Geography geography => GameObject.Find("GameController").GetComponent<GameControllerScript>().gameState.
            Tiles.Played[position.x, position.y].getGeographyAt(direction);
        
        [ObsoleteAttribute("This property is obsolete. Find this in the game state instead.", LegacyDepricationError)]
        public bool free => !GameObject.Find("GameController").GetComponent<GameControllerScript>().gameState.Meeples
            .InPlay.Contains(this);

        [ObsoleteAttribute("This property is obsolete. Find this in the game state instead.", LegacyDepricationError)]
        public int x => position.x;
        public int z=> position.y;

        #endregion
        

        private PlayerScript _player;
        public PlayerScript player
        {
            get => _player;
            set => _player = SetPlayer(value);
        }

        private void Start()
        {
            // free = true;
            // x = 0;
            // z = 0;
            // id = 1;
        }

        public void OnSnapMeeple()
        {
            GameObject.Find("GameController").GetComponent<GameControllerScript>().SetMeepleSnapPos();
        }
        
        /// <summary>
        /// Sets a meeple as being placed at a specific point.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="direction"></param>
        /// <param name="geography"></param>
        // public void assignAttributes(int x, int z, Vector2Int direction, TileScript.Geography geography)
        // {
        //     this.direction = direction;
        //     this.geography = geography;
        //
        //     this.x = x;
        //     this.z = z;
        //
        //     /*
        // switch (direction)
        // {
        //     case PointScript.North:
        //         this.x = x;
        //         this.z = z + .5f;
        //         break;
        //     case PointScript.East:
        //         this.x = x + .5f;
        //         this.z = z;
        //         break;
        //     case PointScript.South:
        //         this.x = x;
        //         this.z = z - .5f;
        //         break;
        //     case PointScript.West:
        //         this.x = x - .5f;
        //         this.z = z;
        //         break;
        //     default:
        //         this.x = x;
        //         this.z = z;
        //         break;
        // }
        // */
        // }

        //TODO Looks like this could be problematic for more than 2 users. Does this ownership mean meeple possession?
        private PlayerScript SetPlayer(PlayerScript p)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
                if (tag == "Meeple 1")
                {
                    Debug.Log("PLATER: " + p.photonUser.name);
                    // Debug.Log("ÄGARE INNAN: " + photonView.Owner.NickName);
                    photonView.TransferOwnership(PhotonNetwork.PlayerList[1]);
                    // Debug.Log("ÄGARE EFTER: " + photonView.Owner.NickName);
                }

            return p;
        }

    }
}