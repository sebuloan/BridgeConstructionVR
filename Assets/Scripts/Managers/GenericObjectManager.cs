using System;
using System.Collections.Generic;
using UnityEngine;
using Bosch.ESA.Managers;
public class GenericObjectManager : MonoBehaviour
{
    /// <summary>
    /// Sets the active state of a list of GameObjects based on the provided data.
    /// Each entry in the list contains a GameObject name and a bool indicating whether it should be enabled.
    /// </summary>
    /// <param name="genericObjects">A list of <see cref="GenericObjectData"/> containing GameObject names and desired active states.</param>
    public void SetGameObjectStates(List<GenericObjectData> genericObjects, Action callback)
    {
        foreach (var data in genericObjects)
        {
            GameObject obj = null;
            //        GameObject obj = GameObject.Find(data.gameObjectName);

            // Find the parent GameObject by name
            GameObject parent = GameObject.Find(data.genericObjectParent);
            if (parent != null)
            {
                // Recursively search inside the parent for the child GameObject
                obj = FindChildRecursive(parent.transform, data.gameObjectName);
            }

            if (obj != null)
            {
                obj.SetActive(data.isSelected);
                Debug.Log($"Object '{data.gameObjectName}' under parent '{data.genericObjectParent}' set to {data.isSelected}");
            }
            else
            {
                Debug.LogWarning($"GameObject '{data.gameObjectName}' under parent '{data.genericObjectParent}' not found.");
            }
        }

        callback?.Invoke();
    }

    private GameObject FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child.gameObject;

            //GameObject result = FindChildRecursive(child, name);
            //if (result != null)
            //    return result;
        }

        return null;
    }

}
