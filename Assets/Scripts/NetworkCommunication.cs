using System;
using System.Collections;
using System.Collections.Generic;

namespace MyFirstARGame
{
    using Photon.Pun;
    using Photon.Realtime;
    using UnityEngine;


    /// <summary>
    /// You can use this class to make RPC calls between the clients. It is already spawned on each client with networking capabilities.
    /// This is used to manage all object spawning event in the game across the players
    /// 
    /// </summary>
    public class NetworkCommunication : MonoBehaviourPun
    {
        [SerializeField] private Scoreboard scoreboard; // Network Manager Could view the Scoreboard

        [SerializeField] private GameObject mousePrefab;
        [SerializeField] private GameObject bulletPrefab;

        public float spawnRadius = 5.0f; // 5 meters of spawning range
        public float spawnInterval = 5.0f; // 5 seconds for spawning 

        // Gameobject list keeps tracking of resources and targets generated in the game
        private List<GameObject> mouseList; // List of Mouse to be shot in the game
        private List<GameObject> supplyList; // List of items that could recharge your bullet Supply
        private List<GameObject> manipulatorList; //  List of items that could disable the players
        private List<GameObject> gainerList; // List of items that has gaining effects




        private IEnumerator spawnPrefabRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(spawnInterval);
                // call spawning code here
                Network_SpawnMouseAroundOrigin(); // Will spawn the mouse every 5 seconds
                Network_SpawnBulletSupplyAroundOrigin(); // Will spawn Bullet Supply From the Sky in Every 5 seconds
            }
        }

        /// <summary>
        /// This function will spawn mouse Around the origin
        /// which is the location of the Image object in the scene
        /// </summary>
        private void Network_SpawnMouseAroundOrigin()
        {
            // get the world origin
            GameObject Image = GameObject.Find("ImageTarget");
            Vector3 worldOrigin = Image.transform.position;

            // generate prefab around the world origin in certain radius
            Vector2 randomPosition2D = Random.insideUnitCircle * spawnRadius;
            Vector3 randomPosition3D = new Vector3(randomPosition2D.x, 0.0f, randomPosition2D.y);
            randomPosition3D += worldOrigin;
            mouseList.Add(PhotonNetwork.Instantiate("Mouse", randomPosition3D, Quaternion.identity));
        }

        /// <summary>
        /// Bullet supply random generator
        /// Generate Bullet Supply randomly on the map
        /// 
        /// </summary>
        private void Network_SpawnBulletSupplyAroundOrigin()
        {
            // get the world origin
            GameObject Image = GameObject.Find("ImageTarget");
            Vector3 worldOrigin = Image.transform.position;

            // generate prefab around the world origin in certain radius
            Vector2 randomPosition2D = Random.insideUnitCircle * spawnRadius;
            Vector3 randomPosition3D = new Vector3(randomPosition2D.x, 0.0f, randomPosition2D.y);
            randomPosition3D += worldOrigin;
            randomPosition3D.y = 10.0f;
            supplyList.Add(PhotonNetwork.Instantiate("BulletSupply", randomPosition3D, Quaternion.identity));
        }


        /// <summary>
        /// <param name="PunRPC">This function is marked as RPC method</param>
        /// Calling this method to delete an Network Object by its viewID across the network
        /// </summary>
        /// <param name="viewID"></param>
        [PunRPC]
        public void Network_DestroyObject(int viewID)
        {
            PhotonView targetView = PhotonView.Find(viewID);
            if (targetView.IsMine)
            {
                PhotonNetwork.Destroy(targetView.gameObject);
            }
        }


        /// <summary>
        /// Destroy an object directly on the Map
        /// </summary>
        /// <param name="viewID"></param>
        public void DestroyObject(int viewID)
        {
            PhotonView targetView = PhotonView.Find(viewID);
            if (targetView != null)
            {
                // Check if the current client owns the object or if it's the master client
                photonView.RPC("Network_DestroyObject", RpcTarget.All, viewID);
            }
            else
            {
                Debug.LogError("PhotonView not existed!");
            }
        }




        /// <summary>
        /// Start is called before the first frame update
        /// Instantiate: mouseList
        /// Instantiate: supplyList
        /// </summary>
        void Start()
        {
            // Instantiate the lists that manages the creation and destroy of the prefab 
            mouseList = new List<GameObject>();
            supplyList = new List<GameObject>();
            StartCoroutine(spawnPrefabRoutine());
        }

        // Update is called once per frame
        void Update()
        {

        }
        
        /// <summary>
        /// Helper that could Print the log
        /// </summary>
        /// <param name="log"></param>
        public void LogHelper(string log)
        {
            photonView.RPC("Network_Log", RpcTarget.MasterClient, log);
        }

        /// <summary>
        /// <typeparamref name="PunRPC"/> Marked as PunRPC>
        /// 
        /// </summary>
        /// <param name="log"></param>
        [PunRPC]
        private void Network_Log(string log)
        {
            Debug.Log(log);
        }

        public void IncrementBulletSupply()
        {
            var playerName = $"Player {PhotonNetwork.LocalPlayer.ActorNumber}";
            var currentBullet = this.scoreboard.GetBulletNo(playerName);
            photonView.RPC("Network_SetPlayerBulletSupply", RpcTarget.All, playerName,currentBullet + 3);
        }

        [PunRPC]
        public void Network_SetPlayerBulletSupply(string playerName, int newSupply)
        {
            Debug.Log($"Player{playerName} getSupply!");
            scoreboard.SetBulletNo(playerName, newSupply);
        }
        
        public void IncrementScore(int score)
        {
            var playerName = $"Player {PhotonNetwork.LocalPlayer.ActorNumber}";
            var currentScore = this.scoreboard.GetScore(playerName);
            photonView.RPC("Network_SetPlayerScore", RpcTarget.All, playerName, currentScore + score);
        }

        [PunRPC] // Method that are managed by the Photon framework
        public void Network_SetPlayerScore(string playerName, int newScore)
        {
            Debug.Log($"Player {playerName} score!");
            scoreboard.SetScore(playerName, newScore);
        }

        public void UpdateForNewPlayer(Player player)
        {
            // Send the current score to the new player
            var playerName = $"Player {PhotonNetwork.LocalPlayer.ActorNumber}";
            var currentScore = this.scoreboard.GetScore(playerName);
            photonView.RPC("Network_SetPlayerScore", player, playerName, currentScore);
        }
    }
}