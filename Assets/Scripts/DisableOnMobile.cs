namespace MyFirstARGame
{
    using UnityEngine;

    /// <summary>
    /// Simple script that disables this game object on mobile platforms.
    /// </summary>
    public class DisableOnMobile : MonoBehaviour
    {
        private void Awake()
        {
            if (Application.isMobilePlatform)
            {
                this.gameObject.SetActive(false);
            }
        }
    }
}
