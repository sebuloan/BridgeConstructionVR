using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookTargetTriggerManager : MonoBehaviour
{
    public string expectedObjectName; // set from code

    public event Action<GameObject> OnObjectTouched;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("triggerobject is " + other.gameObject.name);
        if (string.IsNullOrEmpty(expectedObjectName) || other.gameObject.name == expectedObjectName)
        {
            Debug.Log($"Trigger hit by: {other.gameObject.name}");
            OnObjectTouched?.Invoke(other.gameObject);
        }
    }
}
