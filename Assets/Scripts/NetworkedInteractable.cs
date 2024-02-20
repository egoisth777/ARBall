namespace MyFirstARGame
{
    using UnityEngine;
    using UnityEngine.XR.Interaction.Toolkit.AR;
    using Photon.Pun;
    
    /// <summary>
    /// Helper class for interactables that disables certain components on non-mobile platforms to prevent certain synchronization issues.
    /// </summary>
    public class NetworkedInteractable : MonoBehaviour
    {
        private void Start()
        {
            if (!Application.isMobilePlatform)
            {
                this.GetComponent<ARSelectionInteractable>().enabled = false;
                this.GetComponent<ARTranslationInteractable>().enabled = false;
                this.GetComponent<ARRotationInteractable>().enabled = false;
                this.GetComponent<ARScaleInteractable>().enabled = false;
            }
        }
        public void Die()
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
