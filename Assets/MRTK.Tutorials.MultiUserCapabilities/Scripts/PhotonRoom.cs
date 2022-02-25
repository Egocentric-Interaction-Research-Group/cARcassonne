using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace MRTK.Tutorials.MultiUserCapabilities
{
    public class PhotonRoom : MonoBehaviourPunCallbacks, IInRoomCallbacks
    {
        public static PhotonRoom Room;

        [SerializeField] private GameObject photonUserPrefab = default;
        [SerializeField] private GameObject tableAnchor = default;
        //[SerializeField] private GameObject roverExplorerPrefab = default;
        //[SerializeField] private GameObject cubePrefab = default;
        //[SerializeField] private Transform roverExplorerLocation = default;

        // private PhotonView pv;
        private Player[] photonPlayers;
        private int playersInRoom;
        private int myNumberInRoom;

        // private GameObject module;
        // private Vector3 moduleLocation = Vector3.zero;

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);
            photonPlayers = PhotonNetwork.PlayerList;
            playersInRoom++;
        }

        private void Awake()
        {
            if (Room == null)
            {
                Room = this;
            }
            else
            {
                if (Room != this)
                {
                    Destroy(Room.gameObject);
                    Room = this;
                }
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            PhotonNetwork.AddCallbackTarget(this);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private void Start()
        {
            // pv = GetComponent<PhotonView>();
            
            // Allow prefabs not in a Resources folder
            if (PhotonNetwork.PrefabPool is DefaultPool pool)
            {
                if (photonUserPrefab != null) pool.ResourceCache.Add(photonUserPrefab.name, photonUserPrefab);

                //if (roverExplorerPrefab != null) pool.ResourceCache.Add(roverExplorerPrefab.name, roverExplorerPrefab);
            }
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();

            photonPlayers = PhotonNetwork.PlayerList;
            playersInRoom = photonPlayers.Length;
            myNumberInRoom = playersInRoom;
            PhotonNetwork.NickName = myNumberInRoom.ToString();

            StartGame();
        }

        private void StartGame()
        {
            CreatPlayer();

            // if (!PhotonNetwork.IsMasterClient)
            // {
            //     return;
            // }
            // else
            // {
                CreateInteractableObjects();
            // }

            if (TableAnchor.Instance != null)
            {
               
            }


        }

        private void CreatPlayer()
        {
            var player = PhotonNetwork.Instantiate(photonUserPrefab.name, Vector3.zero, Quaternion.identity);
        }

        private void CreateInteractableObjects()
        {
            //var position = roverExplorerLocation.position;
            //var positionOnTopOfSurface = new Vector3(position.x, position.y + roverExplorerLocation.localScale.y / 2,
            //    position.z);

            var go = PhotonNetwork.Instantiate(tableAnchor.name, Vector3.zero, Quaternion.identity);

            //go.transform.SetParent(GameObject.Find("SharedPlayground").transform);
            //var go1 = PhotonNetwork.Instantiate(cubePrefab.name, positionOnTopOfSurface + new Vector3(0, 1, 0),
            //    roverExplorerLocation.rotation);
        }
    }
}
