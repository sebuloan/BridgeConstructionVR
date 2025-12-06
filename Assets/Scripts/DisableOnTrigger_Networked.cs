using Unity.Netcode;
using UnityEngine;

public class DisableOnTrigger_Networked : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"OnTriggerEnter detected by: {gameObject.name} with: {other.gameObject.name}");

        if (IsServer)
        {
            Debug.Log("Running on the server. Calling DisableCubeServerRpc.");
            DisableCubeServerRpc();
        }
        else
        {
            Debug.Log("OnTriggerEnter called on client, but this should run on server.");
        }
    }

    [ServerRpc]
    private void DisableCubeServerRpc()
    {
        Debug.Log("DisableCubeServerRpc called on the server. Disabling the cube.");
        gameObject.SetActive(false);
        DisableCubeClientRpc();
    }

    [ClientRpc]
    private void DisableCubeClientRpc()
    {
        Debug.Log("DisableCubeClientRpc called on the client. Disabling the cube on the client.");
        gameObject.SetActive(false);
    }
}
