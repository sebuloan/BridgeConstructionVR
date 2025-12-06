using Bosch.SXEDA3;
using System;
using System.Collections;
using UnityEngine;


namespace Bosch.ESA.Managers
{
    public class PickAndDropManager : MonoBehaviour
    {
        private GameObject pickedObject;
        private GameObject targetDropLocation;
        private Vector3 pickedObjectLocation;
        private DropLocationManager dropLocationManager;
        private UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable selectInteractable;
        private Action onCompleteCallback;
        private bool isPickAndDropActive = false;
        private bool pickAndDropCompleted = false;
        private bool objectAtDropLocation = false;
        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable currentGrabInteractable;

        public bool IsPickAndDropActive => isPickAndDropActive;

        public void BeginPickAndDrop(string pickObject, string dropPoint, bool IsAssembly, Action callback)
        {
            if (isPickAndDropActive)
            {
                Debug.LogWarning("Pick and Drop already active. Cannot start a new one.");
                return;
            }

            isPickAndDropActive = true;
            pickAndDropCompleted = false;
            objectAtDropLocation = false;
            pickedObject = GameObject.Find(pickObject);

            UnityUtilities.EnsureCollider(pickedObject);
           
          
            UnityUtilities.EnsureRigidbody(pickedObject, true);
            

            currentGrabInteractable = pickedObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

            if (currentGrabInteractable == null)
            {
                currentGrabInteractable = pickedObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
                currentGrabInteractable.useDynamicAttach = true;
            }

            targetDropLocation = GameObject.Find(dropPoint);
            UnityUtilities.EnsureCollider(targetDropLocation,true);

            pickedObjectLocation = pickedObject.transform.position;
            onCompleteCallback = callback;

            // Ensure DropLocationManager is present on the drop location
            dropLocationManager = targetDropLocation.GetComponent<DropLocationManager>();
            if (dropLocationManager == null)
            {
                dropLocationManager = targetDropLocation.AddComponent<DropLocationManager>();
            }

            dropLocationManager.ObjectDroppedAtLocation.AddListener(OnObjectDropped);
            dropLocationManager.IsAssembly = IsAssembly;

            selectInteractable = pickedObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable>();
            if (selectInteractable == null)
            {
                Debug.LogError("PickAndDropManager: Picked object is not an IXRSelectInteractable.");
                isPickAndDropActive = false;
                pickAndDropCompleted = true;
                onCompleteCallback?.Invoke();
                return;
            }
            StartCoroutine(CheckForDrop());
        }

        private IEnumerator CheckForDrop()
        {
            // Wait for object to be picked up
            while (selectInteractable.isSelected == false)
            {
                yield return null;
            }

            // Wait for object to be dropped
            while (selectInteractable.isSelected == true)
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);

            if (objectAtDropLocation)
            {
                isPickAndDropActive = false;
                pickAndDropCompleted = true;
                pickedObject.GetComponent<Rigidbody>().isKinematic = true;
                pickedObject.GetComponent<Collider>().enabled = false;
                onCompleteCallback?.Invoke();
                yield break;
            }
            else
            {
                pickedObject.transform.position = pickedObjectLocation;
                StartCoroutine(CheckForDrop());
            }
        }

        private void OnObjectDropped(GameObject droppedObject)
        {
            if (droppedObject == pickedObject)
            {
                objectAtDropLocation = true;
            }
        }

        public void StopPickAndDrop()
        {
            isPickAndDropActive = false;
            StopAllCoroutines();
            if (dropLocationManager != null)
            {
                dropLocationManager.ObjectDroppedAtLocation.RemoveListener(OnObjectDropped);
            }
        }

        public bool IsPickAndDropComplete()
        {
            return pickAndDropCompleted;
        }
    }
}