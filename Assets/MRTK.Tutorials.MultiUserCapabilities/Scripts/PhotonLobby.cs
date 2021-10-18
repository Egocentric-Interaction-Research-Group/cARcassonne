using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace MRTK.Tutorials.MultiUserCapabilities
{
    public class PhotonLobby : MonoBehaviourPunCallbacks
    {
        /// <summary>
        /// This version string is used to connect to Photon with.
        /// Clients that have different versions shouldn't be able to join the same room.
        /// </summary>
        [HideInInspector]
        public const string photonGameVersion = "0.0.0";
        
        //---- Exposed to Inspector --------------------------------------------------------------//

        [Tooltip("This must be set before playing.")]
        public bool offlineMode = false;

        [Header("Room Options")]

        [Tooltip("The name of the room to either join, or create if it doesn't exist. A room is "+
            "joined/created as soon as you start playing. Hitting the in-game start button only "+
            "takes you to the game table.")]
        public string roomName = "DevRoom";

        [Tooltip("If you create the room, it's the maximum number of players allowed in the room. " +
            "Otherwise, you can ignore it.")]
        public int maxPlayers = 5;

        //----------------------------------------------------------------------------------------//

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            GenericNetworkManager.OnReadyToStartNetwork += StartNetwork;
        }

        //---- Photon Callbacks ------------------------------------------------------------------//

        public override void OnConnectedToMaster()
        {
            if (PhotonNetwork.OfflineMode)
                PhotonNetwork.JoinRoom(roomName); // Just join a room directly if in offline mode.
            else
                PhotonNetwork.JoinLobby(TypedLobby.Default);
        }

        public override void OnJoinedLobby()
        {
            PhotonNetwork.JoinRoom(roomName);
        }

        public override void OnJoinedRoom()
        {
            Room room = PhotonNetwork.CurrentRoom;
            Debug.Log($"Current number of players in \"{room.Name}\": {room.PlayerCount}");
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.Log($"Failed to join room \"{roomName}\": [{returnCode}] {message}");

            CreateRoom();
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogWarning($"Failed to create room \"{roomName}\": [{returnCode}] {message}");
        }

        //---- Private Methods -------------------------------------------------------------------//

        /// <summary>
        /// Establishes a connection to Photon.
        /// </summary>
        private void StartNetwork()
        {
            string randomUserId = GenerateRandomID(length: 8);
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.AuthValues = new AuthenticationValues();
            PhotonNetwork.AuthValues.UserId = randomUserId;
            PhotonNetwork.GameVersion = photonGameVersion;
            PhotonNetwork.OfflineMode = offlineMode;

            if (offlineMode)
            {
                Debug.Log($"Starting in offline mode with game version {PhotonNetwork.GameVersion}, " +
                    $"and UserId \"{PhotonNetwork.AuthValues.UserId}\".");
            }
            else
            {
                Debug.Log($"Connecting to Photon with game version {PhotonNetwork.GameVersion}, "+
                    $"and UserId \"{PhotonNetwork.AuthValues.UserId}\".");
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        /// <summary>
        /// Creates and joins a new Photon room.
        /// </summary>
        private void CreateRoom()
        {
            Debug.Log($"Creating room \"{roomName}\".");

            if (maxPlayers < 1 || maxPlayers > byte.MaxValue)
            {
                Debug.LogError("Invalid maxPlayers.");
                return;
            }

            var roomOptions = new RoomOptions {IsVisible = true, IsOpen = true, MaxPlayers = (byte)maxPlayers};
            PhotonNetwork.CreateRoom(roomName, roomOptions);
        }

        /// <summary>
        /// Generates a string of random alpha-numerical characters.
        /// </summary>
        /// <param name="length">How long the resulting string will be.</param>
        /// <returns>A randomly generated string ID.</returns>
        private string GenerateRandomID(int length)
        {
            string validIDChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            int validCount = validIDChars.Length;
            char[] id = new char[length];

            for (int i = 0; i < length; i++)
            {
                id[i] = validIDChars[Random.Range(0, validCount)];
            }

            return new string(id);
        }
    }
}
