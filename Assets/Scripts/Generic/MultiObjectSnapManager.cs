using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiObjectSnapManager : MonoBehaviour
{
    [Header("Target Object to Snap")]
    public GameObject objectToSnap;

    [Header("Snap Location")]
    public Transform snapPosition;
    private bool hasSnapped = false;
    private void OnTriggerEnter(Collider other)
    {
        if (hasSnapped) return;

        if (other.gameObject == objectToSnap)
        {
            Debug.Log("Object entered snap zone. Snapping now...");
            SnapObject();
        }
    }

    void SnapObject()
    {
        hasSnapped = true;
        objectToSnap.transform.SetPositionAndRotation(snapPosition.position, snapPosition.rotation);
        Rigidbody rb = objectToSnap.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.isKinematic = true;
            rb.detectCollisions = false;
            StartCoroutine(RemoveRigidbody(rb));
        }
    }

    IEnumerator RemoveRigidbody(Rigidbody rb)
    {
        yield return new WaitForEndOfFrame();
        Destroy(rb);
    }
}