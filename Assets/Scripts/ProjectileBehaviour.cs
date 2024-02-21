namespace MyFirstARGame
{
    using UnityEngine;
    using Photon.Pun;

    /// <summary>
    /// Controls projectile behaviour. In our case it currently only changes the material of the projectile based on the player that owns it.
    /// </summary>
    public class ProjectileBehaviour : MonoBehaviour
    {
        [SerializeField]
        private Material[] projectileMaterials;

        public NetworkCommunication networkCommunication;
        private float alive;

        private void Awake()
        {
            // Pick a material based on our player number so that we can distinguish between projectiles. We use the player number
            // but wrap around if we have more players than materials. This number was passed to us when the projectile was instantiated.
            // See ProjectileLauncher.cs for more details.

            //var photonView = this.transform.GetComponent<PhotonView>();
            //var playerId = Mathf.Max((int)photonView.InstantiationData[0], 0);
            //if (this.projectileMaterials.Length > 0)
            //{
            //    var material = this.projectileMaterials[playerId % this.projectileMaterials.Length];
            //    this.transform.GetComponent<Renderer>().material = material;
            //}
        }

        private void Start()
        {
            alive = 0;
        }

        private void Update()
        {
            alive += Time.deltaTime;
            if (alive > 3.0f) {
                Die();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("mice"))
            {
                // Retrieve the PhotonView of the collided object
                PhotonView collidedObjectPhotonView = collision.gameObject.GetComponent<PhotonView>();

                // Ensure the PhotonView exists
                if (collidedObjectPhotonView != null)
                {
                    NetworkLauncher networkLauncher = FindObjectOfType<NetworkLauncher>();
                    if (networkLauncher != null && networkLauncher.NetworkCommunication != null)
                    {
                        // Destroy the collided object network-wide
                        networkLauncher.NetworkCommunication.LogHelper("I am reached");
                        networkLauncher.NetworkCommunication.DestroyObject(collidedObjectPhotonView.ViewID);
                        // Increment score based on the bullet's tag
                        if (this.gameObject.tag == "cheese")
                        {
                            networkLauncher.NetworkCommunication.IncrementScore(1);
                        }
                        else if (this.gameObject.tag == "croissant")
                        {
                            networkLauncher.NetworkCommunication.IncrementScore(3);
                        }

                        // Call Die method to destroy the bullet itself network-wide
                        Die();
                    }
                    else
                    {
                        Debug.LogError("NetworkLauncher or NetworkCommunication is not found. Destroying locally.");
                        PhotonNetwork.Destroy(collision.gameObject); // Fallback to local destruction if NetworkCommunication is not accessible
                        Die(); // Destroy the bullet as well
                    }
                }
                else
                {
                    Debug.LogError("PhotonView component is missing on the collided object.");
                }
            }
        }
        /// <summary>
        /// This function will get the bullet to be destroyed by the Network Components
        /// 
        /// </summary>
        private void Die()
        {
            // Check if the PhotonView component is attached to this gameObject
            PhotonView photonView = GetComponent<PhotonView>();
            if (photonView != null)
            {
                // Ensure this object is owned by the current client or is the master client
                if (photonView.IsMine || PhotonNetwork.IsMasterClient)
                {
                    // Use the NetworkCommunication class to destroy the object across the network
                    NetworkLauncher networkLauncher = FindObjectOfType<NetworkLauncher>();
                    if (networkLauncher != null && networkLauncher.NetworkCommunication != null)
                    {
                        networkLauncher.NetworkCommunication.DestroyObject(photonView.ViewID);
                    }
                }
            }
            else
            {
                Debug.LogError("PhotonView component is missing on this object.");
            }
        }
    }
}