using Bosch.SXEDA3;
using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Bosch.ESA.Managers
{
    public class PickManager : MonoBehaviour
    {
        private GameObject currentPickableObject;
        private bool retainInHands;
        private Action<string> objectPickedCallback;
        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable currentGrabInteractable;
        private UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor selectingInteractor;
        private GameObject pickObject;
        private GameObject currentlyHeldObject;
        public GameObject CurrentlyHeldObject => currentlyHeldObject;

        [Header("Hand Attach Points")]
        public Transform leftHandAttachPoint;
        public Transform rightHandAttachPoint;

        public void BeginPick(string pickableObject, bool retainInHands, Action<string> onObjectPickedCallback)
        {
            if (string.IsNullOrEmpty(pickableObject))
            {
                Debug.LogError("PickManager: BeginPick called with an empty or null object name.");
                return;
            }

            // Try to find the pickable object in the scene
             pickObject = GameObject.Find(pickableObject);
            UnityUtilities.EnsureCollider(pickObject);
            UnityUtilities.EnsureRigidbody(pickObject, true);

            if (pickObject == null)
            {
                Debug.LogError($"PickManager: GameObject '{pickableObject}' not found.");
                return;
            }

            currentPickableObject = pickObject;
            this.retainInHands = retainInHands;
            objectPickedCallback = onObjectPickedCallback;
            // Check if XRGrabInteractable is attached to the object; if not, add it
            currentGrabInteractable = currentPickableObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (currentGrabInteractable == null)
            {
                currentGrabInteractable = currentPickableObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
                currentGrabInteractable.useDynamicAttach = true;
                currentGrabInteractable.farAttachMode = UnityEngine.XR.Interaction.Toolkit.Attachment.InteractableFarAttachMode.Near;
            }
            currentPickableObject.AddComponent<GrabHandDetector>();
            // Avoid multiple listeners if BeginPick is called again for the same object
            currentGrabInteractable.selectEntered.RemoveListener(OnObjectSelected);
            currentGrabInteractable.selectEntered.AddListener(OnObjectSelected);
        }

        private void OnObjectSelected(SelectEnterEventArgs args)
        {
            Debug.Log("PickManager.OnObjectSelected called.");
            selectingInteractor = args.interactorObject;

            if (retainInHands && selectingInteractor != null)
            {
                // Determine the attach transform from the interactor
                Transform attachTransform = GetAttachTransform(selectingInteractor);

                // Parent the picked object to the attach transform
                currentPickableObject.transform.SetParent(attachTransform);
                currentPickableObject.transform.localPosition = Vector3.zero;

                // Disable Retain Transform Parent to prevent auto-unparenting
                currentGrabInteractable.retainTransformParent = false;

                // Prevent further grabbing while retained
                currentGrabInteractable.enabled = false;

                currentlyHeldObject = currentPickableObject;

                // Invoke the callback
                objectPickedCallback?.Invoke(currentPickableObject.name);
            }
            else if (!retainInHands)
            {
                // Invoke the callback on the first select enter (pick)
                objectPickedCallback?.Invoke(currentPickableObject.name);
            }
        }

        public void StopPick()
        {
            if (currentGrabInteractable != null)
            {
                currentGrabInteractable.selectEntered.RemoveListener(OnObjectSelected);
            }

            currentPickableObject = null;
            currentlyHeldObject = null;
            retainInHands = false;
            objectPickedCallback = null;
            currentGrabInteractable = null;
            selectingInteractor = null;
        }

        // Helper function to get the correct attach transform
        //private Transform GetAttachTransform(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor interactor)
        //{
        //    if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor directInteractor)
        //    {
        //        return directInteractor.attachTransform;
        //    }
        //    else if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor)
        //    {
        //        return rayInteractor.attachTransform;
        //    }
        //    return interactor.transform; // Fallback to interactor transform if no specific type
        //}
        private Transform GetAttachTransform(IXRInteractor interactor)
        {
            string name = interactor.transform.name.ToLower();

            if (name.Contains("left"))
                return leftHandAttachPoint;
            else if (name.Contains("right"))
                return rightHandAttachPoint;

            Debug.LogWarning("Unknown interactor, defaulting to left hand.");
            return interactor.transform; // fallback
        }

    }
}
