using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(NetworkObject))]
public class NetworkInteractable : NetworkBehaviour
{
    private XRGrabInteractable _grabInteractable;
    private Rigidbody _rb;

    private void Awake()
    {
        _grabInteractable = GetComponent<XRGrabInteractable>();
        _rb = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"NetworkInteractable spawned - IsServer: {IsServer}, IsClient: {IsClient}");

        if (_grabInteractable == null)
            return;

        // Configure based on network role
        if (IsServer)
        {
            Debug.Log("Server mode - Enabling interactions");
            ConfigureForServer();
        }
        else
        {
            Debug.Log("Client mode - Disabling interactions");
            ConfigureForClient();
        }
    }

    private void ConfigureForServer()
    {
        // Server can interact with objects
        _grabInteractable.enabled = true;

        // Setup physics for server
        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;
        }

        // Add listeners for grab events
        _grabInteractable.selectEntered.AddListener(OnGrab);
        _grabInteractable.selectExited.AddListener(OnRelease);
    }

    private void ConfigureForClient()
    {
        // Clients cannot interact with objects
        _grabInteractable.enabled = false;

        // Setup physics for client (kinematic so they don't fall)
        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (_grabInteractable == null)
            return;

        _grabInteractable.selectEntered.RemoveListener(OnGrab);
        _grabInteractable.selectExited.RemoveListener(OnRelease);
    }

    #region XR Grab Handlers
    private void OnGrab(SelectEnterEventArgs args)
    {
        if (!IsServer) return;

        Debug.Log($"Server grabbed object: {gameObject.name}");

        // For hand tracking, you might want to keep it kinematic during grab
        // to prevent physics interference with hand tracking
        if (_rb != null)
        {
            _rb.isKinematic = true; // Important for smooth hand tracking
            _rb.useGravity = false;
        }
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (!IsServer) return;

        Debug.Log($"Server released object: {gameObject.name}");

        // When released, enable physics again
        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;
        }
    }
    #endregion

    // Optional: Add visual feedback for clients to know they can't interact
    private void Update()
    {
        // You could add visual effects here to show interaction state
        if (!IsServer && _grabInteractable.enabled)
        {
            _grabInteractable.enabled = false;
        }
    }
}