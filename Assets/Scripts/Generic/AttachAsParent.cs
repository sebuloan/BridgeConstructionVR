using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachAsParent : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("XR Origin"))
        {
            other.transform.parent = this.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.Contains("XR Origin"))
        {
            other.transform.parent = null;
        }
    }
}
