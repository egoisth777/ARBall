namespace MyFirstARGame
{
    using UnityEngine;
    using UnityEngine.XR.Interaction.Toolkit.AR;

    /// <summary>
    /// Helper class that exposes a few common settings that you might want to toggle from the Unity Editor.
    /// </summary>
    public class SessionSettings : MonoBehaviour
    {
        [Header("General")]
        [SerializeField]
        private bool enableXRPlacement;

        [Header("Planes")]
        [SerializeField]
        private bool enablePlaneDetection;

        [SerializeField]
        private bool showDebugPlanes;

        // Indicates whether there are any pending changes.
        private bool pendingChanges;

        private void Start()
        {
            this.ApplyChanges();
        }

        private void Update()
        {
            if (this.pendingChanges)
            {
                this.pendingChanges = false;
                this.ApplyChanges();
            }
        }

        private void ApplyChanges()
        {
            var placementObj = GameObject.FindObjectOfType<PlaceOnPlane>(true);
            if (placementObj != null)
            {
                placementObj.enabled = this.enableXRPlacement;
            }

            var planeManager = GameObject.FindObjectOfType<UnityEngine.XR.ARFoundation.ARPlaneManager>(true);
            if (planeManager != null)
            {
                planeManager.enabled = this.enablePlaneDetection;
            }

            var planeDebug = GameObject.FindObjectsOfType<UnityEngine.XR.ARFoundation.ARPlaneMeshVisualizer>(true);
            foreach (var plane in planeDebug)
            {
                plane.enabled = this.showDebugPlanes;
            }
        }

        private void OnValidate()
        {
            this.pendingChanges = true;
        }
    }
}
