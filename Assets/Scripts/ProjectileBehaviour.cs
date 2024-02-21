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

        private void Awake()
        {
            // Pick a material based on our player number so that we can distinguish between projectiles. We use the player number
            // but wrap around if we have more players than materials. This number was passed to us when the projectile was instantiated.
            // See ProjectileLauncher.cs for more details.
            var photonView = this.transform.GetComponent<PhotonView>();
            var playerId = Mathf.Max((int)photonView.InstantiationData[0], 0);
            if (this.projectileMaterials.Length > 0)
            {
                var material = this.projectileMaterials[playerId % this.projectileMaterials.Length];
                this.transform.GetComponent<Renderer>().material = material;
            }
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            
            if (collision.gameObject.CompareTag("mice"))
            {   
                NetworkCommunication networkCommunication = FindObjectOfType<NetworkCommunication>();
                networkCommunication.LogHelper("step to increment score");
                networkCommunication.IncrementScore();
                networkCommunication.LogHelper("step to destroy");
                networkCommunication.LogHelper(collision.gameObject.name);
                networkCommunication.DestroyObject(collision.gameObject);
                Die();
            }
        }
        
        
        
        private void Die()
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
