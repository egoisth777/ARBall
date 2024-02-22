using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine;

namespace MyFirstARGame
{
    public class MouseBehavior : MonoBehaviourPun
    {
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log("Hit Something");
            if (photonView.IsMine)
            {
                Debug.Log("Mouse is Mine");
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}
