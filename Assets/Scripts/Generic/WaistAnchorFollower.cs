using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaistAnchorFollower : MonoBehaviour
{
    public Transform head;
    public Vector3 offset = new Vector3(0f, -0.6f, 0f); // Half meter down

    void Update()
    {
        if (head != null)
        {
            Vector3 flatForward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
            Quaternion waistRotation = Quaternion.LookRotation(flatForward, Vector3.up);

            transform.position = head.position + waistRotation * offset;
            transform.rotation = waistRotation;
        }
    }
}
