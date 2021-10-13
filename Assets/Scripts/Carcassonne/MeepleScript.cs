using Photon.Pun;
using UnityEngine;

namespace Carcassonne
{
    public class MeepleScript : MonoBehaviourPun
    {
        // Start is called before the first frame update
        public Material[] materials = new Material[5];
        public PointScript.Direction direction;
        public TileScript.Geography geography;
        public bool free;

        public int x, z;
        

        private PlayerScript _player;
        public PlayerScript player
        {
            get => _player;
            set => _player = SetPlayer(value);
        }

        private void Start()
        {
            free = true;
            x = 0;
            z = 0;
            // id = 1;
        }

        public void OnSnapMeeple()
        {
            GameObject.Find("GameController").GetComponent<GameControllerScript>().SetMeepleSnapPos();
        }

        public void assignAttributes(int x, int z, PointScript.Direction direction, TileScript.Geography geography)
        {
            this.direction = direction;
            this.geography = geography;

            this.x = x;
            this.z = z;

            /*
        switch (direction)
        {
            case PointScript.Direction.NORTH:
                this.x = x;
                this.z = z + .5f;
                break;
            case PointScript.Direction.EAST:
                this.x = x + .5f;
                this.z = z;
                break;
            case PointScript.Direction.SOUTH:
                this.x = x;
                this.z = z - .5f;
                break;
            case PointScript.Direction.WEST:
                this.x = x - .5f;
                this.z = z;
                break;
            default:
                this.x = x;
                this.z = z;
                break;
        }
        */
        }

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