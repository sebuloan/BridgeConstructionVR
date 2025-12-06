using UnityEngine;
using System;
using Bosch.SXEDA3;
using System.Collections.Generic;

public class CollisionManager : MonoBehaviour
{
    public Action onCollisionDetected;

    private GameObject collidingObject;
    private GameObject impactedObject;
    private bool isTriggerMode;
    public List<GameObject> collidingObjects;
    private bool isHandBasedCollision;

    public void Initialize(string colliding, string impacted, bool triggerMode,bool isHandCollision, Action callback)
    {
        isHandBasedCollision = isHandCollision;
        if (!isHandBasedCollision)
        {
            collidingObject = GameObject.Find(colliding);
        }
        impactedObject = GameObject.Find(impacted);


        GameObject managerGO = GameObject.Find("CollisionManager");
        //if (!collidingObject.name.Contains("Button"))
        //{
        //    UnityUtilities.EnsureCollider(collidingObject, !(triggerMode));
        //    //UnityUtilities.EnsureRigidbody(impactedObject, false);
        //}

        if (isHandBasedCollision)
        {
            UnityUtilities.EnsureCollider(impactedObject, triggerMode);
            UnityUtilities.EnsureRigidbody(impactedObject, true);
          //  UnityUtilities.EnsureRigidbody(collidingObject, true);
        }
        else
        {
            UnityUtilities.EnsureCollider(impactedObject, triggerMode);
            UnityUtilities.EnsureRigidbody(impactedObject, true);
        }

        onCollisionDetected = callback;
        isTriggerMode = triggerMode;
        if (impactedObject != null)
        {
            CollisionImpactmanager impactManager = impactedObject.GetComponent<CollisionImpactmanager>();
            if (impactManager == null)
                impactManager = impactedObject.AddComponent<CollisionImpactmanager>();

            impactManager.collisionManager = managerGO.GetComponent<CollisionManager>();
            impactManager.SetTriggerMode(isTriggerMode);
        }
        else
        {
            Debug.LogError("Impacted object not found.");
        }
    }

    public void HandleTrigger(GameObject objA, GameObject objB)
    {
        if (!isTriggerMode) return;

        if (IsTargetCollision(objA, objB))
        {
            Debug.Log("Trigger collision detected.");
            onCollisionDetected?.Invoke();
        }
    }

    public void HandleCollision(GameObject objA, GameObject objB)
    {
        if (isTriggerMode) return;

        if (IsTargetCollision(objA, objB))
        {
            Debug.Log("Collision detected.");
            onCollisionDetected?.Invoke();
        }
    }
    private bool IsTargetCollision(GameObject a, GameObject b)
    {
        if(!isHandBasedCollision)
        {
            return (a == impactedObject && b == collidingObject);
        }
        else
        {
            return (a == impactedObject && collidingObjects.Contains(b));
        }
       
    }
}
