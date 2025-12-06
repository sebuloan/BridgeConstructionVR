using Bosch.SXEDA3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class TeleportManager : MonoBehaviour
{
    private Action OnTeleportReached;
    private GameObject teleportPoint;
    private GameObject currentTeleportParent;
    private bool enableAllTeleport = false;

    public void EnableAllTeleportPoints()
    {
        if (currentTeleportParent == null) return;

        foreach (Transform child in currentTeleportParent.transform)
        {
            child.gameObject.SetActive(true);
        }

        enableAllTeleport = true;
    }

    public void PlayerTeleportArea(string teleportParentName, string teleportGameobjectName, Action OnTriggerReached)
    {
        GameObject foundParent = GameObject.Find(teleportParentName);
        if (foundParent == null)
        {
            Debug.LogWarning("Teleport parent not found: " + teleportParentName);
            return;
        }

        currentTeleportParent = foundParent;
        teleportPoint = FindTeleportPointByName(currentTeleportParent, teleportGameobjectName);

        if (teleportPoint != null)
        {
            teleportPoint.SetActive(true);
            OnTeleportReached = OnTriggerReached;

            UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationAnchor anchor = teleportPoint.GetComponent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationAnchor>();
            if (anchor != null)
            {
                anchor.teleporting.AddListener(OnTeleporting);
            }
        }
        else
        {
            Debug.LogWarning("Teleport point not found: " + teleportGameobjectName);
        }
    }

    private GameObject FindTeleportPointByName(GameObject parent, string name)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.name == name)
                return child.gameObject;
        }

        return null;
    }

    private void OnTeleporting(UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportingEventArgs args)
    {
        Debug.Log("Teleport completed to: " + teleportPoint?.name);

        UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationAnchor anchor = teleportPoint.GetComponent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationAnchor>();
        if (anchor != null)
        {
            anchor.teleporting.RemoveListener(OnTeleporting);
        }

        OnTeleportReached?.Invoke();
        teleportPoint.SetActive(false);

        if (enableAllTeleport && currentTeleportParent != null)
        {
            foreach (Transform child in currentTeleportParent.transform)
            {
                if (child.gameObject != teleportPoint)
                {
                    child.gameObject.SetActive(true);
                }
            }
        }
    }

    private void OnDisable()
    {
        enableAllTeleport = false;
    }
}
