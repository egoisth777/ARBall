namespace MyFirstARGame
{
    using System.Collections.Generic;
    using System.Linq;
    using Photon.Pun;
    using Unity.XR.CoreUtils;
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// Class responsible for managing the creation of one shared space for all devices.
    /// It uses an image target at a known root position (0, 0, 0) to synchronize coordinate systems.
    /// It also creates a networked instance of its own camera position.
    /// </summary>
    public class SharedSpaceManager : MonoBehaviour
    {
        [SerializeField]
        private ARTrackedImageManager arTrackedImageManager;

        [SerializeField]
        private GameObject hostOrigin;

        [SerializeField]
        private Material trackedImageMaterialPreSync;

        [SerializeField]
        private Material trackedImageMaterialPostSync;

        private List<ARTrackedImage> trackedImages;
        private GameObject networkedTrackedImage;
        private bool hasFoundImageTarget;
        private bool hasFoundOrigin;

        private GameObject arCamera;
        private bool syncNextTick;

        private void Awake()
        {
            // Image tracking needs a mobile device to work.
            if (!Application.isMobilePlatform)
            {
                this.enabled = false;
            }
        }

        private void Start()
        {
            this.trackedImages = new List<ARTrackedImage>();
            NetworkLauncher.Singleton.JoinedRoom += this.NetworkLauncher_JoinedRoom;
        }

        private void OnEnable()
        {
            this.arTrackedImageManager.trackedImagesChanged += this.ArTrackedImageManager_trackedImagesChanged;
        }

        private void OnDisable()
        {
            this.arTrackedImageManager.trackedImagesChanged -= this.ArTrackedImageManager_trackedImagesChanged;
        }

        private void NetworkLauncher_JoinedRoom(NetworkLauncher sender)
        {
            var mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            this.arCamera = PhotonNetwork.Instantiate("ARCamera", mainCamera.transform.position, mainCamera.transform.rotation);
        }

        private void ArTrackedImageManager_trackedImagesChanged(ARTrackedImagesChangedEventArgs args)
        {
            var addedAndUpdated = args.added.Concat(args.updated);
            foreach (ARTrackedImage trackedImg in addedAndUpdated)
            {
                if (trackedImg.trackingState == TrackingState.Tracking)
                {
                    // Our SharedSpaceImageLibrary only contains one image, so we do not have to filter anything here.
                    Debug.Log("Tracking image: " + trackedImg.referenceImage.name);

                    // If we have not found our image target yet, show it to the user.
                    if (!this.hasFoundImageTarget)
                    {
                        // You can instantiate a GameObject at the exact location of the tracked image if you wanted to.
                        this.networkedTrackedImage = PhotonNetwork.Instantiate("TrackedImage", trackedImg.transform.position, trackedImg.transform.rotation);

                        // Now inform the user that the image has been found and that they can synchronize the world.
                        // We also hook up an event to know when the user has pressed the image target, so we can
                        // synchronize the coordinate systems.
                        this.ShowOutline(true, true);
                        this.networkedTrackedImage.GetComponent<TrackedImageController>().Pressed += this.TrackedImageController_Pressed;
                        this.hasFoundImageTarget = true;
                    }
                    else
                    {
                        // First, let us update the transform of the tracked image.
                        // We use the UpdateScale RPC for the host scene because it has its own image target object that needs scaling.
                        // Check TrackedImageController.UpdateScale for more information.
                        this.networkedTrackedImage.transform.SetPositionAndRotation(trackedImg.transform.position, trackedImg.transform.rotation);
                        this.networkedTrackedImage.transform.localScale = new Vector3(trackedImg.size.x, 1f, trackedImg.size.y);
                        this.networkedTrackedImage.GetPhotonView().RPC("UpdateScale", RpcTarget.Others, this.networkedTrackedImage.transform.localScale);

                        // The user pressed our image target. Sync up the coordinate systems and inform the user by rendering a green outline.
                        // We also could choose to update our coordinate systems here periodically if we are still tracking.
                        // This allows us to compensate for potential drifting due to sensor inaccuracies.
                        // if (someTimeElapsed) -> MatchReferenceCoordinateSystem()
                        if (this.syncNextTick)
                        {
                            this.ShowOutline(true, false);
                            this.MatchReferenceCoordinateSystem(trackedImg.gameObject);
                            this.hasFoundOrigin = true;
                            this.syncNextTick = false;
                        }
                    }
                }
                else
                {
                    Debug.Log("Not tracking image: " + trackedImg.referenceImage.name);
                }
            }

            this.trackedImages = new List<ARTrackedImage>(addedAndUpdated);
        }

        private void TrackedImageController_Pressed(TrackedImageController sender, Vector3 position)
        {
            // The user pressed our image target. Sync up the coordinate systems the next time we get an update.
            Debug.Log("Image target pressed.");
            this.syncNextTick = true;
        }

        private void MatchReferenceCoordinateSystem(GameObject trackedImage)
        {
            // Compute where our camera is relative to the image target in the image target's coordinate system.
            var arCamera = GameObject.FindGameObjectWithTag("MainCamera");
            var localPosition = trackedImage.transform.InverseTransformPoint(arCamera.transform.position);
            var localRotation = Quaternion.Inverse(trackedImage.transform.rotation) * arCamera.transform.rotation;

            // Now compute where that point is in the reference image's coordinate system.
            // Host origin is at 0, 0, 0 in our global coordinate system. This is where the host has placed its image target
            // and where, once we track it, we want our camera to be relative to.
            // By doing so, we effectively "merge" our two virtual scenes into one and can proceed with one uniform coordinate
            // system for all objects that can be easily synchronized across the network.
            var hostCameraPosition = this.hostOrigin.transform.TransformPoint(localPosition);
            var hostCameraRotation = this.hostOrigin.transform.rotation * localRotation;

            // Now we can move our origin to the host's coordinate system.
            // Our XROrigin object allows us to move the camera around without having to move  any objects in the actual scene.
            var xrOrigin = GameObject.FindObjectOfType<XROrigin>();
            var targetPosition = hostCameraPosition;
            var targetRotation = hostCameraRotation;

            // We compute how much we have to move the parent of the camera, which is the offset necessary so that our child
            // (the camera) achives the desired position and rotation.
            Quaternion currentChildRot = xrOrigin.Camera.transform.rotation;
            Quaternion relativeRotation = Quaternion.Inverse(xrOrigin.transform.rotation) * currentChildRot;
            xrOrigin.transform.rotation = targetRotation * Quaternion.Inverse(relativeRotation);
            xrOrigin.transform.position += targetPosition - xrOrigin.Camera.transform.position;
        }

        private void ShowOutline(bool show, bool preSync)
        {
            if (this.networkedTrackedImage != null)
            {
                var controller = this.networkedTrackedImage.GetComponent<TrackedImageController>();
                controller.ShowOutline = show;
                controller.OutlineMaterial = preSync ? this.trackedImageMaterialPreSync : this.trackedImageMaterialPostSync;
            }
        }

        private void Update()
        {
            // Update our camera position every frame.
            if (this.arCamera != null)
            {
                var mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
                this.arCamera.transform.SetPositionAndRotation(mainCamera.transform.position, mainCamera.transform.rotation);
            }
        }

        private void OnGUI()
        {
            // Creating a simple GUI on the phone screen so we can debug the image target.
            GUIStyle style = new()
            {
                fontSize = 30,
            };

            var text = this.hasFoundOrigin ? "Scanned image target" : this.hasFoundImageTarget ? "Found image target" : "Looking for image target";
            style.normal.textColor = this.hasFoundOrigin ? Color.green : Color.red;
            GUI.Label(new Rect(0, Screen.height / 2 + 200, 100, 100), text, style);
        }
    }
}