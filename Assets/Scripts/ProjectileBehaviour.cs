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
                NetworkCommunication networkCommunication = FindObjectOfType<NetworkCommunication>();
                if (this.gameObject.tag == "cheese")
                {
                    networkCommunication.IncrementScore(1);
                }
                else if (this.gameObject.tag == "croissant")
                {
                    networkCommunication.IncrementScore(3);
                }
                networkCommunication.DestroyObject(collision.gameObject);
                Die();
            }
        }

        private void Die()
        {
            NetworkCommunication networkCommunication = FindObjectOfType<NetworkCommunication>();
            networkCommunication.DestroyObject(this.gameObject);
        }
    }
}