using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Collections.Generic;

public class CustomPalmGrab : MonoBehaviour
{
    [Header("Hand Colliders")]
    public Collider palmCollider;
    public Collider middleCollider;
    public Collider ringFingerCollider;
    public Collider littleFingerCollider;

    [Header("XR Grab Targets")]
    public List<XRGrabInteractable> grabInteractables = new List<XRGrabInteractable>();

    private Dictionary<XRGrabInteractable, IXRSelectInteractor> currentInteractors = new Dictionary<XRGrabInteractable, IXRSelectInteractor>();
    private XRInteractionManager interactionManager;

    [System.Obsolete]
    private void Awake()
    {
        // If no grab interactables are assigned, try to get one from this GameObject
        if (grabInteractables.Count == 0)
        {
            var interactable = GetComponent<XRGrabInteractable>();
            if (interactable != null)
            {
                grabInteractables.Add(interactable);
            }
        }

        interactionManager = FindObjectOfType<XRInteractionManager>();
    }

    private void Update()
    {
        // Check if palm is intersecting with ANY finger collider
        bool isTouching =
            (palmCollider.bounds.Intersects(middleCollider.bounds)) ||
            (palmCollider.bounds.Intersects(ringFingerCollider.bounds)) ||
            (palmCollider.bounds.Intersects(littleFingerCollider.bounds));

        if (isTouching)
        {
            TryAutoGrab();
        }
        else
        {
            ForceReleaseAll();
        }
    }

    private void TryAutoGrab()
    {
        foreach (var grabInteractable in grabInteractables)
        {
            // Skip if already grabbed
            if (currentInteractors.ContainsKey(grabInteractable) && grabInteractable.isSelected)
                continue;

            // Find a valid interactor hovering this object
            foreach (var interactor in grabInteractable.interactorsHovering)
            {
                if (interactor is IXRSelectInteractor selectInteractor)
                {
                    // Force grab
                    if (!grabInteractable.isSelected)
                    {
                        interactionManager.SelectEnter(selectInteractor, grabInteractable);
                        currentInteractors[grabInteractable] = selectInteractor;
                    }
                    break;
                }
            }
        }
    }

    private void ForceReleaseAll()
    {
        List<XRGrabInteractable> toRemove = new List<XRGrabInteractable>();

        foreach (var kvp in currentInteractors)
        {
            var grabInteractable = kvp.Key;
            var interactor = kvp.Value;

            if (grabInteractable.isSelected)
            {
                interactionManager.SelectExit(interactor, grabInteractable);
            }
            toRemove.Add(grabInteractable);
        }

        // Remove all released interactables from the dictionary
        foreach (var grabInteractable in toRemove)
        {
            currentInteractors.Remove(grabInteractable);
        }
    }

    // Optional: Helper methods to manage the list
    public void AddGrabInteractable(XRGrabInteractable interactable)
    {
        if (!grabInteractables.Contains(interactable))
        {
            grabInteractables.Add(interactable);
        }
    }

    public void RemoveGrabInteractable(XRGrabInteractable interactable)
    {
        if (grabInteractables.Contains(interactable))
        {
            grabInteractables.Remove(interactable);

            // Also remove from current interactors if present
            if (currentInteractors.ContainsKey(interactable))
            {
                currentInteractors.Remove(interactable);
            }
        }
    }

    public void ClearAllGrabInteractables()
    {
        ForceReleaseAll();
        grabInteractables.Clear();
    }
}