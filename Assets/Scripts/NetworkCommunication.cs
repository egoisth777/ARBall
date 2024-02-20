using System.Collections;

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

        [SerializeField]
        private Scoreboard scoreboard; // Network Manager Could view the Scoreboard

        [SerializeField] 
        private GameObject mousePrefab;
        [SerializeField]
        private GameObject bulletPrefab;
        
        public float spawnRadius = 10.0f; // 5 meters of spawning range
        public float spawnInterval = 5.0f; // 5 seconds for spawning 


        // Gameobject list keeps tracking of resources and targets generated in the game
        private ArrayList mouseList;
        private ArrayList supplyList;
        
        private IEnumerator spawnPrefabRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(spawnInterval);
                // call spawning code here
                Network_SpawnMouseAroundOrigin(); // Will spawn the mouse every 5 seconds
                Network_SpawnBulletSupplyAroundOrigin();
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
            photonView.RPC("InstantiateMouse", RpcTarget.All, randomPosition3D);
            
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
            photonView.RPC("InstantiateBulletSupply", RpcTarget.All, randomPosition3D);
        }
        
        /// <summary>
        /// 
        /// </summary>
        private void Network_DestroyMouse(GameObject mouseObject)
        {
            photonView.RPC("DestroyMouse", RpcTarget.All, mouseObject);
        }


        [PunRPC]
        public void InstantiateBulletSupply(Vector3 position)
        {
            supplyList.Add(Instantiate(bulletPrefab, position, Quaternion.identity));
        }
        
        
        
        [PunRPC] // must be marked for methods that is intended to be called by PhotonView
        public void InstantiateMouse(Vector3 position)
        {
            GameObject mouse = Instantiate(mousePrefab, position, Quaternion.identity);
            mouseList.Add(mouse);
        }

        /// <summary>
        /// Destroys the specified mouse game object.
        /// </summary>
        /// <param name="mousePrefab">The mouse game object to destroy.</param>
        [PunRPC]
        public void DestroyMouse(GameObject mousePrefab)
        {
            Destroy(mousePrefab);
        }
        
        
        
        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(spawnPrefabRoutine());
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void IncrementScore()
        {
            var playerName = $"Player {PhotonNetwork.LocalPlayer.ActorNumber}";
            var currentScore = this.scoreboard.GetScore(playerName);
            photonView.RPC("Network_SetPlayerScore", RpcTarget.All, playerName, currentScore + 1);
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