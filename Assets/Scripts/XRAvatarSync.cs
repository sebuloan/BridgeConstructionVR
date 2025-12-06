using UnityEngine;
using Unity.Netcode;
using Unity.XR.CoreUtils;

public class XRAvatarSync : NetworkBehaviour
{
    [Header("Networked Avatar References")]
    public XRMultiplayer.XRINetworkPlayer networkPlayer;

    private XROrigin xrOrigin;
    private Transform headOrigin;
    private Transform leftHandOrigin;
    private Transform rightHandOrigin;

    void Start()
    {
        // Only run this logic for the local player
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        // Find the local XR Origin
        xrOrigin = FindFirstObjectByType<XROrigin>();

        if (xrOrigin != null)
        {
            headOrigin = xrOrigin.Camera.transform;
            leftHandOrigin = xrOrigin.transform.Find("Camera Offset/LeftHand Controller");
            rightHandOrigin = xrOrigin.transform.Find("Camera Offset/RightHand Controller");
        }

        // If XRINetworkPlayer reference is not set, try to find it automatically
        if (networkPlayer == null)
            networkPlayer = GetComponent<XRMultiplayer.XRINetworkPlayer>();

        // Optional: hide the local avatar visuals
        foreach (var hand in networkPlayer.GetComponentsInChildren<Renderer>())
            hand.enabled = false;
    }

    void LateUpdate()
    {
        if (!IsOwner || networkPlayer == null || xrOrigin == null)
            return;

        // Sync networked avatar head/hand positions with XR rig
        if (headOrigin != null)
            networkPlayer.head.SetPositionAndRotation(headOrigin.position, headOrigin.rotation);

        if (leftHandOrigin != null)
            networkPlayer.leftHand.SetPositionAndRotation(leftHandOrigin.position, leftHandOrigin.rotation);

        if (rightHandOrigin != null)
            networkPlayer.rightHand.SetPositionAndRotation(rightHandOrigin.position, rightHandOrigin.rotation);
    }
}
