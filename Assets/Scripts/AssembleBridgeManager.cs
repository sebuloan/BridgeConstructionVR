using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AssembleBridgeManager : NetworkBehaviour
{
    [Header("Assembly Setup")]
    public List<GameObject> disassembledObjects;
    public List<GameObject> assembledObjects;
    public Collider assemblyZone;

    [Header("UI")]
    public GameObject wrongObjectUI;

    private int currentStep = 0;

    void Start()
    {
        // Disable all assembled objects at start
        foreach (var obj in assembledObjects)
            obj.SetActive(false);

        if (wrongObjectUI != null)
            wrongObjectUI.SetActive(false);

        // Debug collider setup
        Debug.Log("[Setup] Assembly Zone Collider: " + (assemblyZone != null ? assemblyZone.name : "NULL"));
        Debug.Log("[Setup] Assembly Zone isTrigger: " + (assemblyZone != null ? assemblyZone.isTrigger.ToString() : "N/A"));
        Debug.Log("[Setup] Assembly Zone bounds: " + (assemblyZone != null ? assemblyZone.bounds.ToString() : "N/A"));

        // Check disassembled objects
        foreach (var obj in disassembledObjects)
        {
            if (obj != null)
            {
                Collider objCollider = obj.GetComponent<Collider>();
                Rigidbody objRigidbody = obj.GetComponent<Rigidbody>();
                Debug.Log($"[Setup] Object: {obj.name}, Collider: {objCollider != null}, Rigidbody: {objRigidbody != null}, IsTrigger: {(objCollider != null ? objCollider.isTrigger.ToString() : "N/A")}");
            }
        }
    }

    void Update()
    {
        // Debug any objects currently in the trigger
        if (assemblyZone != null)
        {
            Collider[] overlappedColliders = Physics.OverlapBox(
                assemblyZone.bounds.center,
                assemblyZone.bounds.extents,
                assemblyZone.transform.rotation
            );

            if (overlappedColliders.Length > 0)
            {
                foreach (var collider in overlappedColliders)
                {
                    if (disassembledObjects.Contains(collider.gameObject))
                    {
                        Debug.Log($"[Debug] Object {collider.name} is physically inside assembly zone but trigger not firing!");
                    }
                }
            }

            if (IsServer)
            {
                Debug.Log("SERVER sees object pos: " + disassembledObjects[0].transform.position);
            }
            else
            {
                Debug.Log("CLIENT sees object pos: " + disassembledObjects[0].transform.position);
            }

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[OnTriggerEnter] FINALLY CALLED! Object: {other.name}");

        if (!IsServer) return;

        Debug.Log($"[OnTriggerEnter] Processing: {other.name}");

        // Check if the object that entered is in our disassembled objects list
        if (disassembledObjects.Contains(other.gameObject))
        {
            Debug.Log($"[Assemble] Valid object placed: {other.gameObject.name}");

            if (currentStep < disassembledObjects.Count &&
                other.gameObject == disassembledObjects[currentStep])
            {
                Debug.Log($"[Assemble] Correct object placed: {other.gameObject.name}, Step {currentStep + 1}");

                assembledObjects[currentStep].SetActive(true);
                other.gameObject.SetActive(false);

                SetAssembledObjectStateClientRpc(currentStep, true);
                SetDisassembledObjectStateClientRpc(currentStep, false);

                currentStep++;
            }
            else
            {
                Debug.Log($"[Assemble] Wrong object placed: {other.gameObject.name}, Expected: {disassembledObjects[currentStep].name}");
                if (wrongObjectUI != null)
                    ShowWrongUIClientRpc();
            }
        }
    }

    [ClientRpc]
    void SetAssembledObjectStateClientRpc(int idx, bool isActive)
    {
        if (idx >= 0 && idx < assembledObjects.Count)
            assembledObjects[idx].SetActive(isActive);
    }

    [ClientRpc]
    void SetDisassembledObjectStateClientRpc(int idx, bool isActive)
    {
        if (idx >= 0 && idx < disassembledObjects.Count)
            disassembledObjects[idx].SetActive(isActive);
    }

    [ClientRpc]
    void ShowWrongUIClientRpc()
    {
        if (wrongObjectUI != null)
            wrongObjectUI.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"[OnTriggerExit] Collider exited: {other.name}");

        if (wrongObjectUI != null && wrongObjectUI.activeSelf)
        {
            Debug.Log("[UI] Hiding Wrong Object UI");
            HideWrongUIClientRpc();
        }
    }

    [ClientRpc]
    void HideWrongUIClientRpc()
    {
        if (wrongObjectUI != null)
            wrongObjectUI.SetActive(false);
    }
}


