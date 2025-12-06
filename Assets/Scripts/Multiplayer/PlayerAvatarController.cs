using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerAvatarController : NetworkBehaviour
{
    [SerializeField]
    private Transform syncedHead;
    [SerializeField]
    private Transform syncedLeftHand;
    [SerializeField]
    private Transform syncedRightHand;

    private Transform xrLeftHand;
    private Transform xrRightHand;
    private Transform xrHead;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            xrHead = Camera.main.transform;
            var xrOrigin = GameObject.Find("XR Origin (XR Rig)");
            xrLeftHand = xrOrigin.transform.Find("Camera Offset/Left Controller");
            xrRightHand = xrOrigin.transform.Find("Camera Offset/Right Controller");
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        syncedHead.position = xrHead.position;
        syncedHead.rotation = xrHead.rotation;

        syncedLeftHand.position = xrLeftHand.position;
        syncedLeftHand.rotation = xrLeftHand.rotation;

        syncedRightHand.position = xrRightHand.position;
        syncedRightHand.rotation = xrRightHand.rotation;
    }
}
