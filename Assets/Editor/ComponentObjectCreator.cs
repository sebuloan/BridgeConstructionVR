//  <WMObjectCreator.cs>
//
// Copyright SX/EDA3 2023 all rights reserved.
//
// Authors:
//   Sagar T Y, Suraj M K
//
// Defines:
//   [C] Bosch.Evc.Interactable.WMObjectCreator
// This script will automate the process of preparing the game objects for WM Module

using Bosch.SXEDA3;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using System.Collections.Generic; // Added for List<GameObject>

public class ComponentObjectCreator : EditorWindow
{
    public TextAsset jsonObject;
    public GameObject parentObject; // New field for parent object reference

    private const string _windowTitle = "Interactable Objects";
    private static ComponentObjectCreator objectCreator;
    private ComponentInfo _componentInfo = new ComponentInfo();

    [MenuItem("Bosch/Prepare Assets for Component")]
    public static void ShowWindow()
    {
        objectCreator = (ComponentObjectCreator)GetWindow(typeof(ComponentObjectCreator));
        objectCreator.Show();
        objectCreator.titleContent.text = _windowTitle;
    }

    //Invoked when GUI is created
    private void OnGUI()
    {
        GUILayout.Space(5);
        GUILayout.Label("This will automatically add required components for interactable objects");

        GUILayout.Space(10);
        GUILayout.Label("Select the JSON File (Optional)", EditorStyles.boldLabel);
        jsonObject = (TextAsset)EditorGUILayout.ObjectField(jsonObject, typeof(TextAsset), false);

        GUILayout.Space(10);
        GUILayout.Label("Select Parent Object (Optional - Used if no JSON)", EditorStyles.boldLabel);
        parentObject = (GameObject)EditorGUILayout.ObjectField(parentObject, typeof(GameObject), true); // 'true' allows scene objects

        GUILayout.Space(10);
        if (GUILayout.Button("Add Interactable Components"))
        {
            List<GameObject> objectsToProcess = new List<GameObject>();

            if (jsonObject != null)
            {
                // Process objects from JSON
                try
                {
                    _componentInfo = JsonUtility.FromJson<ComponentInfo>(jsonObject.text);
                    foreach (var component in _componentInfo.components)
                    {
                        GameObject interactableObject = GameObject.Find(component.name);
                        if (interactableObject != null)
                        {
                            objectsToProcess.Add(interactableObject);
                        }
                        else
                        {
                            Debug.LogWarning($"GameObject with name '{component.name}' not found in the scene (from JSON).");
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error parsing JSON or finding objects from JSON: {e.Message}");
                }
            }
            else if (parentObject != null)
            {
                // Process children of the parent object
                foreach (Transform child in parentObject.transform)
                {
                    objectsToProcess.Add(child.gameObject);
                }
                if (objectsToProcess.Count == 0)
                {
                    Debug.LogWarning("No children found under the selected parent object.");
                }
            }
            else
            {
                Debug.LogWarning("Please provide either a JSON file or a Parent Object to add components.");
            }

            // Now, iterate through the collected objects and add components
            foreach (GameObject interactableObject in objectsToProcess)
            {
                if (interactableObject != null)
                {
                    if (interactableObject.GetComponent<Outline>() == null)
                    {
                        interactableObject.AddComponent<Outline>();
                    }

                    if (interactableObject.GetComponent<Rigidbody>() == null)
                    {
                        Rigidbody _rigidbody = interactableObject.AddComponent<Rigidbody>();
                        _rigidbody.mass = 10;
                        _rigidbody.useGravity = false;
                        _rigidbody.isKinematic = true;
                    }
                    if (interactableObject.GetComponent<XRGrabInteractable>() == null)
                    {
                        XRGrabInteractable xrGrab = interactableObject.AddComponent<XRGrabInteractable>();
                        xrGrab.useDynamicAttach = true;
                    }

                    if (interactableObject.GetComponent<XRGeneralGrabTransformer>() == null)
                    {
                        interactableObject.AddComponent<XRGeneralGrabTransformer>();
                    }

                    //Adding ComponentInterationTrigger to GameObject
                    if (interactableObject.GetComponent<ComponentInteractionTrigger>() == null)
                    {
                        interactableObject.AddComponent<ComponentInteractionTrigger>();
                    }

                    //Adding BoxCollider to GameObject
                    UnityUtilities.AddBoxColliderFromMeshes(interactableObject);
                }
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
    }
}