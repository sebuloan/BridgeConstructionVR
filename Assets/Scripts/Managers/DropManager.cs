using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace Bosch.ESA.Managers
{
    public class DropManager : MonoBehaviour
    {
        [Header("Grip Actions")]
        public InputActionReference leftGripAction;
        public InputActionReference rightGripAction;

        public event Action<GameObject> ObjectDropped;

        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable heldGrabInteractable;
        private GameObject objectToDrop;
        private string grabbedHandName;

        private bool waitingForGripRelease = false;

        private void Update()
        {
            if (waitingForGripRelease && grabbedHandName != null)
            {
                if (IsGripReleased(grabbedHandName))
                {
                    Debug.Log("Grip released after ungrab. Completing drop.");
                    OnDropCompleted();
                }
            }
        }

        public void BeginDrop(string dropObjectName)
        {
            objectToDrop = GameObject.Find(dropObjectName);
            if (objectToDrop == null)
            {
                Debug.LogWarning("DropManager: Could not find object: " + dropObjectName);
                return;
            }
            heldGrabInteractable = objectToDrop.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (heldGrabInteractable != null)
            {
                heldGrabInteractable.enabled = true; // Re-enable grabbing
                heldGrabInteractable.retainTransformParent = true;
                var grabHandDetector = objectToDrop.GetComponent<GrabHandDetector>();
                if (grabHandDetector != null)
                {
                    grabbedHandName = grabHandDetector.GetGrabHandName();
                    Debug.Log($"DropManager: Grabbed hand from GrabHandDetector: {grabbedHandName}");
                }
                else
                {
                    Debug.LogWarning("DropManager: No GrabHandDetector found.");
                    grabbedHandName = null;
                }

                heldGrabInteractable.selectExited.AddListener(OnObjectReleased);
                Debug.Log($"DropManager: Drop initiated for {objectToDrop.name}. Waiting for grip release.");
            }
            else
            {
                Debug.LogWarning($"DropManager: Object {objectToDrop.name} has no XRGrabInteractable.");
                OnDropCompleted();
            }
        }

        private void OnObjectReleased(SelectExitEventArgs args)
        {
            string releasedHand = GetInteractorHandName(args.interactorObject.transform);
            Debug.Log("Released by: " + releasedHand);

            if (releasedHand == grabbedHandName || grabbedHandName == null)
            {
                if (IsGripReleased(releasedHand))
                {
                    Debug.Log("Grip already released. Completing drop.");
                    OnDropCompleted();
                }
                else
                {
                    Debug.Log("Grip still held. Will wait in Update().");
                    waitingForGripRelease = true;
                }
            }
            else
            {
                Debug.LogWarning("Released by wrong hand. Ignoring.");
            }

            if (heldGrabInteractable != null)
                heldGrabInteractable.selectExited.RemoveListener(OnObjectReleased);
        }

        private bool IsGripReleased(string handName)
        {
            float gripValue = 1f;

            if (handName.ToLower().Contains("left") && leftGripAction != null)
                gripValue = leftGripAction.action.ReadValue<float>();
            else if (handName.ToLower().Contains("right") && rightGripAction != null)
                gripValue = rightGripAction.action.ReadValue<float>();

            return gripValue < 0.1f;
        }

        private string GetInteractorHandName(Transform interactorTransform)
        {
            return interactorTransform.parent != null ? interactorTransform.parent.name : interactorTransform.name;
        }

        private void OnDropCompleted()
        {
            if (objectToDrop != null)
            {
                objectToDrop.transform.SetParent(null);

                Rigidbody rb = objectToDrop.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;
                }

                ObjectDropped?.Invoke(objectToDrop);
            }

            objectToDrop = null;
            heldGrabInteractable = null;
            grabbedHandName = null;
            waitingForGripRelease = false;
        }

        public void StopDrop()
        {
            if (heldGrabInteractable != null)
                heldGrabInteractable.selectExited.RemoveListener(OnObjectReleased);

            objectToDrop = null;
            heldGrabInteractable = null;
            grabbedHandName = null;
            waitingForGripRelease = false;
        }
    }
    }