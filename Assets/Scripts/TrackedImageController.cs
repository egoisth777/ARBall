namespace MyFirstARGame
{
    using Photon.Pun;
    using System.Linq;
    using UnityEngine;

    [RequireComponent(typeof(PressEventsProvider))]
    /// <summary>
    /// This class is responsible for controlling the TrackedImage prefab. The prefab gets instantiated by <see cref="SharedSpaceManager"/>
    /// when the phone is tracking an image.
    /// </summary>
    public class TrackedImageController : MonoBehaviourPun
    {
        /// <summary>
        /// Event handler for interaction events.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="position">The position of the interaction.</param>
        public delegate void TrackedImageInteractionEventHandler(TrackedImageController sender, Vector3 position);

        /// <summary>
        /// Event that is raised when the user pressed on this image target.
        /// </summary>
        public event TrackedImageInteractionEventHandler Pressed;

        [SerializeField]
        private GameObject[] outlines;

        private void Awake()
        {
            // Hide borders by default.
            this.ShowOutline = false;

            // Hook up touch events on mobile.
            if (Application.isMobilePlatform && this.photonView.IsMine)
            {
                this.GetComponent<PressEventsProvider>().Pressed += this.TrackedImageController_Pressed;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the outline is currently being shown.
        /// </summary>
        public bool ShowOutline
        {
            get => this.outlines.Count(g => g.activeSelf) == this.outlines.Length;
            set
            {
                foreach (var outline in this.outlines)
                {
                    outline.SetActive(value);
                }
            }
        }

        /// <summary>
        /// Sets the outline material of the tracked image.
        /// </summary>
        public Material OutlineMaterial
        {
            set
            {
                foreach (var outline in this.outlines)
                {
                    outline.GetComponent<Renderer>().material = value;
                }
            }
        }

        private void TrackedImageController_Pressed(Vector3 position)
        {
            // Test whether the pressed position intersects with the tracked image.
            var ray = Camera.main.ScreenPointToRay(position);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000, LayerMask.GetMask("TrackedImage")) && hit.transform == this.transform)
            {
                this.Pressed?.Invoke(this, position);
            }
        }

        /// <summary>
        /// Updates the scale of the reference image target object on the PC scene to reflect the tracked dimensions.
        /// </summary>
        /// <param name="realScale">The real world scale.</param>
        [PunRPC]
        public void UpdateScale(Vector3 realScale)
        {
            // In our PC scene, we have an ImageTarget object that we can update with the observerd real word size and then disable us.
            // It might still be desirable to keep this GameObject around, especially when troubleshooting image tracking related issues.
            // But you could also remove it entirely and just send the tracked data to the PC instead of instantiating a GameObject.
            var dummyImageTarget = GameObject.Find("ImageTarget");
            if (dummyImageTarget != null)
            {
                dummyImageTarget.transform.localScale = realScale;
                this.gameObject.SetActive(false);
            }
        }
    }
}
