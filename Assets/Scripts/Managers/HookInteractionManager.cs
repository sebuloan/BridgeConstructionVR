using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Bosch.SXEDA3;
using Bosch.ESA.Managers;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing;
using UnityEngine.InputSystem.EnhancedTouch;

public class HookInteractionManager : MonoBehaviour
{
    private bool touched;


    public void HandleHookInteraction(string pickObjectName, string hookTargetLocationName, List<ClimbInteractorObject> climbInteractors, Action onHookInteractionCompleted)
    {
        StartCoroutine(HandleHookCoroutine(pickObjectName, hookTargetLocationName, climbInteractors, onHookInteractionCompleted));
    }
    private IEnumerator HandleHookCoroutine(
    string pickObjectName,
    string hookTargetLocationName,
    List<ClimbInteractorObject> climbInteractors,
    Action onHookInteractionCompleted)
    {
        GameObject pickObject = GameObject.Find(pickObjectName);
        GameObject hookTargetObject = GameObject.Find(hookTargetLocationName);

        if (pickObject == null || hookTargetObject == null)
        {
            Debug.LogWarning("PickObject or HookTargetLocation not found.");
            yield break;
        }

        UnityUtilities.EnsureCollider(pickObject);
        UnityUtilities.EnsureRigidbody(pickObject, true);

        if (hookTargetObject.GetComponent<Collider>() == null)
        {
            BoxCollider box = hookTargetObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
        }

        var grabInteractable = pickObject.GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
            grabInteractable = pickObject.AddComponent<XRGrabInteractable>();

        grabInteractable.enabled = true;

        var trigger = hookTargetObject.GetComponent<HookTargetTriggerManager>();
        if (trigger == null)
            trigger = hookTargetObject.AddComponent<HookTargetTriggerManager>();

        trigger.expectedObjectName = pickObject.name;

        bool touched = false;
        trigger.OnObjectTouched += (GameObject touchedObject) =>
        {
            if (touchedObject == pickObject)
            {
                touched = true;
            }
        };

      //  pickObject.transform.SetParent(null);

        while (!touched)
            yield return null;

        pickObject.transform.position = hookTargetObject.transform.position;
        pickObject.transform.rotation = hookTargetObject.transform.rotation;

        grabInteractable.enabled = false;
        pickObject.transform.SetParent(hookTargetObject.transform.parent);

        // No need to use names — use direct references
        if (climbInteractors != null)
        {
            foreach (var item in climbInteractors)
            {
                if (item.climbInteractorgameObject != null)
                {
                    GameObject climbInteractor = GameObject.Find(item.climbInteractorgameObject);

                    var climb = climbInteractor.GetComponent<ClimbInteractable>();
                    if (climb != null)
                        climb.enabled = item.isEnabled;
                }
            }
        }

        onHookInteractionCompleted?.Invoke();
    }



    //public void HandleHookInteraction(string pickObjectName, string hookTargetLocationName, string climbingInteractionObjectName, bool isClimbInteractor, Action onHookInteractionCompleted)
    //{
    //    StartCoroutine(HandleHookCoroutine(pickObjectName, hookTargetLocationName, climbingInteractionObjectName, isClimbInteractor, onHookInteractionCompleted));
    //}

    //private IEnumerator HandleHookCoroutine(string pickObjectName, string hookTargetLocationName, string climbingInteractionObjectName, bool isClimbInteractor, Action onHookInteractionCompleted)
    //{
    //    GameObject pickObject = GameObject.Find(pickObjectName);
    //    GameObject hookTargetObject = GameObject.Find(hookTargetLocationName);
    //    GameObject climbingObject = GameObject.Find(climbingInteractionObjectName);

    //    if (pickObject == null || hookTargetObject == null)
    //    {
    //        Debug.LogWarning("PickObject or HookTargetLocation not found.");
    //        yield break;
    //    }

    //    UnityUtilities.EnsureCollider(pickObject);
    //    UnityUtilities.EnsureRigidbody(pickObject, true);

    //    if (hookTargetObject.GetComponent<Collider>() == null)
    //    {
    //        BoxCollider box = hookTargetObject.AddComponent<BoxCollider>();
    //        box.isTrigger = true;
    //    }

    //    var grabInteractable = pickObject.GetComponent<XRGrabInteractable>();
    //    if (grabInteractable == null)
    //        grabInteractable = pickObject.AddComponent<XRGrabInteractable>();

    //    // Enable grab interactable to start
    //    grabInteractable.enabled = true;
    //    Debug.Log($"grabInteractable.enabled set to true. isClimbInteractor = {isClimbInteractor}");

    //    // Setup trigger detection once
    //    var trigger = hookTargetObject.GetComponent<HookTargetTriggerManager>();
    //    if (trigger == null)
    //        trigger = hookTargetObject.AddComponent<HookTargetTriggerManager>();

    //    trigger.expectedObjectName = pickObject.name;

    //    bool touched = false;
    //    trigger.OnObjectTouched += (GameObject touchedObject) =>
    //    {
    //        if (touchedObject == pickObject)
    //        {
    //            touched = true;
    //        }
    //    };

    //    // For detach, unparent first; for attach, do nothing yet
    //    if (!isClimbInteractor)
    //    {
    //        pickObject.transform.SetParent(null);
    //    }

    //    // Wait for touch event
    //    while (!touched)
    //        yield return null;

    //    // Snap to target position and rotation
    //    pickObject.transform.position = hookTargetObject.transform.position;
    //    pickObject.transform.rotation = hookTargetObject.transform.rotation;

    //    // Disable grab interactable after snapping
    //    grabInteractable.enabled = false;

    //    // Parent to hook target's parent after snapping
    //    pickObject.transform.SetParent(hookTargetObject.transform.parent);

    //    // Enable or disable climbing component based on isClimbInteractorSelected
    //    if (climbingObject != null)
    //    {
    //        var climb = climbingObject.GetComponent<ClimbInteractable>();
    //        if (climb != null)
    //            climb.enabled = isClimbInteractor;
    //    }

    //    onHookInteractionCompleted?.Invoke();
    //}
}