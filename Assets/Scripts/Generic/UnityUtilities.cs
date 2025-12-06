//
// <UnityUtilities.cs>
//
// Copyright SX/EDA3 2023 all rights reserved.
//
// Authors:
//   Sagar T Y
//
// Defines:
// This script contains some of the unity utilities like Calculating the Mesh bounds etc.

using System;
//using Unity.Android.Types;
using UnityEngine;

namespace Bosch.SXEDA3
{
    public class UnityUtilities : MonoBehaviour
    {
        /// <summary>
        /// Acts as an interface for getting the bounds of a GameObject including all its children.
        /// </summary>
        /// <param name="gameObject">GameObject to calculate bounds for.</param>
        /// <returns>World-space bounds encapsulating all child renderers.</returns>
        public static Bounds GetObjectBounds(GameObject gameObject)
        {
            if (gameObject == null) return new Bounds();

            // Collect all renderers (MeshRenderer and SkinnedMeshRenderer)
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0)
                return new Bounds(gameObject.transform.position, Vector3.zero);

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        /// <summary>
        /// Calculates the bounds of all children meshes and adds or updates a BoxCollider on the parent object.
        /// </summary>
        /// <param name="animObject">GameObject to which the collider will be added.</param>
        public static void AddCollider(GameObject animObject)
        {
            if (animObject == null) return;

            Bounds meshBounds = GetObjectBounds(animObject);

            if (meshBounds.size == Vector3.zero) return;

            // Correct size considering parent scale
            Vector3 parentScale = animObject.transform.lossyScale;
            Vector3 newColliderSize = new Vector3(
                meshBounds.size.x / parentScale.x,
                meshBounds.size.y / parentScale.y,
                meshBounds.size.z / parentScale.z
            );

            BoxCollider boxCollider = animObject.GetComponent<BoxCollider>();
            if (boxCollider == null)
                boxCollider = animObject.AddComponent<BoxCollider>();

            boxCollider.size = meshBounds.size;
            boxCollider.center = animObject.transform.InverseTransformPoint(meshBounds.center);
        }

        /// <summary>
        /// Adds a BoxCollider to the target GameObject based on the combined bounds of all child MeshRenderers.
        /// </summary>
        /// <param name="target">The GameObject to add the collider to.</param>
        public static void AddBoxColliderFromMeshes(GameObject target, bool isTrigger = false)
        {
            if (target == null)
            {
                Debug.LogWarning("ColliderUtility: Target GameObject is null.");
                return;
            }

            // Get all mesh-based renderers
            var meshRenderers = target.GetComponentsInChildren<MeshRenderer>();
            var skinnedMeshRenderers = target.GetComponentsInChildren<SkinnedMeshRenderer>();

            if (meshRenderers.Length == 0 && skinnedMeshRenderers.Length == 0)
            {
                Debug.LogWarning("ColliderUtility: No MeshRenderers or SkinnedMeshRenderers found.");
                return;
            }

            // Initialize bounds with the first valid renderer
            Bounds combinedBounds = new Bounds();
            bool boundsInitialized = false;

            foreach (var r in meshRenderers)
            {
                if (!boundsInitialized)
                {
                    combinedBounds = r.bounds;
                    boundsInitialized = true;
                }
                else
                {
                    combinedBounds.Encapsulate(r.bounds);
                }
            }

            foreach (var r in skinnedMeshRenderers)
            {
                if (!boundsInitialized)
                {
                    combinedBounds = r.bounds;
                    boundsInitialized = true;
                }
                else
                {
                    combinedBounds.Encapsulate(r.bounds);
                }
            }

            // Convert world bounds to local space
            Vector3 localCenter = target.transform.InverseTransformPoint(combinedBounds.center);
            Vector3 localMin = target.transform.InverseTransformPoint(combinedBounds.min);
            Vector3 localMax = target.transform.InverseTransformPoint(combinedBounds.max);
            Vector3 localSize = localMax - localMin;

            // Add or update BoxCollider
            BoxCollider collider = target.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = target.AddComponent<BoxCollider>();
            }

            collider.center = localCenter;
            collider.size = new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z));
            collider.isTrigger = isTrigger;
        }

        public static void EnsureCollider(GameObject obj, bool isTrigger = false)
        {
            //AddCollider(obj);
            AddBoxColliderFromMeshes(obj, isTrigger);
            //obj.GetComponent<BoxCollider>().isTrigger = isTrigger;
        }

        public static void EnsureRigidbody(GameObject obj, bool isKinematic =true)
        {
            if (obj == null)
            {
                Debug.LogError("EnsureRigidbody failed: GameObject is null.");
                return;
            }

            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb == null)
                rb = obj.AddComponent<Rigidbody>();
                rb.isKinematic = isKinematic;

            rb.isKinematic = isKinematic;
        }


    }
}
