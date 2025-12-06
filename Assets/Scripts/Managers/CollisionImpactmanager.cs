using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionImpactmanager : MonoBehaviour
{
    public CollisionManager collisionManager;
    private bool isTriggerMode;

    public void SetTriggerMode(bool triggerMode)
    {
        isTriggerMode = triggerMode;

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = isTriggerMode;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggerMode)
        {
            collisionManager.HandleTrigger(gameObject, other.gameObject);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!isTriggerMode)
        {
            collisionManager.HandleCollision(gameObject, collision.gameObject);
        }
    }
}
