namespace MyFirstARGame
{
    using Photon.Pun;
    using Photon.Realtime;
    using UnityEngine;

    /// <summary>
    /// Enables basic network functionality by connecting to the Photon server.
    /// </summary>
    public class NetworkLauncher : MonoBehaviourPunCallbacks
    {
        /// <summary>
        /// Gets the singleton instance of this class (<see cref="NetworkLauncher"/>).
        /// </summary>
        public static NetworkLauncher Singleton = null;

        /// <summary>
        /// Event handler for when a room has been joined.
        /// </summary>
        /// <param name="sender">The network launcher</param>
        public delegate void NetworkLauncherJoinedRoomEventHandler(NetworkLauncher sender);

        /// <summary>
        /// Event that is raised when the client has joined a room.
        /// </summary>
        public event NetworkLauncherJoinedRoomEventHandler JoinedRoom;

        private bool isJoinedToRoom;

        /// <summary>
        /// Gets a value indicating whether we have joined a room.
        /// </summary>
        public bool HasJoinedRoom => this.isJoinedToRoom;

        /// <summary>
        /// Gets the <see cref="NetworkCommunication"/> instance.
        /// </summary>
        public NetworkCommunication NetworkCommunication { get; private set; }

        private void Awake()
        {
            this.InitSingleton();
        }

        private void InitSingleton()
        {
            if (NetworkLauncher.Singleton == null)
            {
                NetworkLauncher.Singleton = this;
            }
            else
            {
                GameObject.Destroy(this.gameObject);
            }

            GameObject.DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            // Try to connect to the master server.
            PhotonNetwork.ConnectUsingSettings();
        }

        private void OnGUI()
        {
            // Creating a simple GUI on the phone screen to we can debug the connection.
            GUIStyle style = new()
            {
                fontSize = 30
            };

            if (this.isJoinedToRoom)
            {
                GUI.Label(new Rect(0, Screen.height / 2 + 300, 100, 100), "Joined to room!!!", style);
            }
            else
            {
                GUI.Label(new Rect(0, Screen.height / 2 + 300, 100, 100), "Not joined to room", style);
            }
        }

        public override void OnConnectedToMaster()
        {
            // If we enter here it means we are connected to the master server.
            Debug.Log("Connected to master");

            // To keep it simple, we will join a random room.
            PhotonNetwork.JoinRandomOrCreateRoom();
        }

        public override void OnJoinedRoom()
        {
            // Entering here means we are connected to a room.
            Debug.Log("Joined room");
            this.isJoinedToRoom = true;

            // First client spawns the network manager.
            if (PhotonNetwork.IsMasterClient)
            {
                this.NetworkCommunication = PhotonNetwork.Instantiate("NetworkManager", Vector3.zero, Quaternion.identity).GetComponent<NetworkCommunication>();
            }

            this.JoinedRoom?.Invoke(this);
        }

        public override void OnLeftRoom()
        {
            Debug.Log("Left room");
            this.isJoinedToRoom = false;
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            NetworkCommunication.UpdateForNewPlayer(newPlayer);
        }
    }
}