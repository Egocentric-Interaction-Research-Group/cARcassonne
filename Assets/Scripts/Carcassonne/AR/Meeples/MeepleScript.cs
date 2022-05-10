using System.Linq;
using Carcassonne.AR;
using Carcassonne.Controllers;
using Carcassonne.Models;
using Carcassonne.State;
using MRTK.Tutorials.MultiUserCapabilities;
using Photon.Pun;
using PunTabletop;
using UI.Grid;
using UnityEngine;

namespace Carcassonne.Meeples
{
    public class MeepleScript : MonoBehaviourPun, IPunInstantiateMagicCallback
    {
        // Start is called before the first frame update
        // public Material[] materials = new Material[5];
        // public Vector2Int direction;
        // public meepleScript.Geography geography;
        // public bool free;
        //
        // public int x, z;

        // #region Legacy
        //
        // private const bool LegacyDepricationError = false;
        //
        // // [Obsolete("This property is obsolete. Find this in the game state instead.", LegacyDepricationError)]
        // // public Vector2Int direction => GameObject.Find("GameController").GetComponent<GameControllerScript>().gameController.
        // //     state.Meeples.Placement.Single(kvp => kvp.Value.Meeple == this).Value.Direction;
        // //
        // // [Obsolete("This property is obsolete. Find this in the game state instead.", LegacyDepricationError)]
        // // public Vector2Int position => GameObject.Find("GameController").GetComponent<GameControllerScript>().gameController.state
        // //     .Meeples.Placement.Single(kvp => kvp.Value.Meeple == this).Key;
        //
        // // [Obsolete("This property is obsolete. Find this in the game state instead.", LegacyDepricationError)]
        // // public Geography geography => GameObject.Find("GameController").GetComponent<GameControllerScript>().gameController.state.
        // //     meeples.Played[position.x, position.y].GetGeographyAt(direction);
        // //
        // // [Obsolete("This property is obsolete. Find this in the game state instead.", LegacyDepricationError)]
        // // public bool free => !GameObject.Find("GameController").GetComponent<GameControllerScript>().gameController.state.Meeples
        // //     .InPlay.Contains(meeple);
        //
        // // [Obsolete("This property is obsolete. Find this in the game state instead.", LegacyDepricationError)]
        // // public int x => position.x;
        // // public int z=> position.y;
        //
        // #endregion
        
        

        private Player _player;
        public Player player
        {
            get => _player;
            set => _player = SetPlayer(value);
        }

        public Meeple meeple => GetComponent<Meeple>();

        private void Start()
        {
            // free = true;
            // x = 0;
            // z = 0;
            // id = 1;
        }

        // public void OnSnapMeeple()
        // {
        //     GameObject.Find("GameController").GetComponent<MeepleControllerScript>().SetMeepleSnapPos();
        // }
        
        /// <summary>
        /// Sets a meeple as being placed at a specific point.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="direction"></param>
        /// <param name="geography"></param>
        // public void assignAttributes(int x, int z, Vector2Int direction, meepleScript.Geography geography)
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
        //     case Vector2Int.up:
        //         this.x = x;
        //         this.z = z + .5f;
        //         break;
        //     case Vector2Int.right:
        //         this.x = x + .5f;
        //         this.z = z;
        //         break;
        //     case Vector2Int.down:
        //         this.x = x;
        //         this.z = z - .5f;
        //         break;
        //     case Vector2Int.left:
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
        private Player SetPlayer(Player p)
        {
            // if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
            //     if (tag == "Meeple 1")
            //     {
            //         // Debug.Log("PLATER: " + p.Name);
            //         // Debug.Log("ÄGARE INNAN: " + photonView.Owner.NickName);
            //         photonView.TransferOwnership(PhotonNetwork.PlayerList[1]);
            //         // Debug.Log("ÄGARE EFTER: " + photonView.Owner.NickName);
            //     }
            //
            // return p;
            
            photonView.TransferOwnership(p.GetComponent<PhotonUser>().player);
            GetComponent<Meeple>().player = p;
            return p;
        }

        public static MeepleScript Get(Meeple m)
        {
            return m.GetComponent<MeepleScript>();
        }
        
        #region PUN

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            var state = FindObjectOfType<GameState>();
            var meeple = GetComponent<Meeple>();
            var p = FindObjectsOfType<Player>().ToList().Single(p=> p.id == (int)info.photonView.InstantiationData[0]);
            // var meepleController = FindObjectOfType<MeepleController>();
            var arMeepleController = FindObjectOfType<MeepleControllerScript>();
            
            SetPlayer(p);
            
            transform.SetParent(arMeepleController.parent.transform);
            gameObject.name = $"Meeple {arMeepleController.MeepleCount}";

            // Setup Grid
            var gridPosition = GetComponent<GridPosition>();
            if (gridPosition)
                Debug.Log("GridPosition object found: " + gridPosition.name);
            else
                Debug.LogWarning("No GridPosition object could be found");
            gridPosition.grid = arMeepleController.meepleGrid;

            var confirmButton = arMeepleController.confirmButton;
            if (confirmButton)
                Debug.Log("ConfirmButton object found: " + confirmButton.name);
            else
                Debug.LogWarning("No ConfirmButton object could be found");
            
            gridPosition.OnChangeCell.AddListener(i => confirmButton.OnMeepleChange());
            
            
            GetComponent<TableBoundaryEnforcerScript>().spawnPos = GameObject.Find("MeepleDrawPosition");
            
            gameObject.SetActive(false);
            arMeepleController.MeepleCount++;
        }
        #endregion

    }
}