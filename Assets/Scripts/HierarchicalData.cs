using UnityEngine;
using UnityEditor;
using UnityEngine.Video;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.XR;

#if UNITY_EDITOR
using UnityEditorInternal;

using System;
namespace Bosch.VRTraining.Author
{
    public class HierarchicalData : MonoBehaviour
    {
        public List<Procedure> procedures = new List<Procedure>();
    }

    [System.Serializable]
    public class AnimationEntry
    {
        public GameObject targetGameObject;
        public AnimationClip animationClip;
        public float startFrame;  // Added start frame
        public float endFrame;
    }

    [System.Serializable]
    public class SelectableObject
    {
        public GameObject gameObject;
        public bool isSelected;
    }

    [System.Serializable]
    public class ClimbInteractorObject
    {
        public GameObject climbInteractorgameObject;
        public bool isEnabled;
    }

    [System.Serializable]
    public class Procedure
    {
        public string procedureType;
        public int stepNumber;  // New field
        public bool isExpanded = true;
        public GameObject gameObject;
        public GameObject dropLocation;
        public GameObject collidingObject;
        public GameObject impactedObject;
        public GameObject particleSystemObject;
        public GameObject glovesObject;
        public AnimationEntry animations;
        public List<AudioClip> stepDescriptionAudioClips = new List<AudioClip>();
        public AudioClip stepInteractionAudioClip;
        public List<string> texts = new List<string>();
        public List<VideoClip> videoClips = new List<VideoClip>();
        public int delay;
        public bool userConfirmation;
        public List<GameObject> highlightGameObject = new List<GameObject>();

        public List<GameObject> otherSafetyObjects = new List<GameObject>();
        public bool assembly = false;
        public string confirmationText;
        public bool retainInHands = false;
        public bool triggerMode = false;
        public bool isHandCollision = false;

        public bool customPosition = false;
        public bool customRotation = false;
        public List<GameObject> objectsToPosition = new List<GameObject>();               // Objects you want to place somewhere
        public List<GameObject> snapPointsForPositioningObjects = new List<GameObject>(); // Snap points where those objects should be placed
        public GameObject endSnapPoint;                                                 // The final snap point or position
        public GameObject endObject;                                                    // The final object or target object
        public GameObject pickObject;
        public GameObject hookAttachTarget;
        public List<ClimbInteractorObject> climbInteractors = new List<ClimbInteractorObject>();
        public List<SelectableObject> genericObjects = new List<SelectableObject>();
    }
#endif
#if UNITY_EDITOR
    [CustomEditor(typeof(HierarchicalData))]
    public class HierarchicalDataEditor : Editor
    {
        private string jsonFileName = " ";
        private SerializedProperty proceduresProp;
        private ReorderableList reorderableList;
        private string[] topLevelOptions = { "PickAndDrop", "Animation", "Pick", "Drop", "CollisionType", "Teleport", "General", "SafetyMeasurement", "CustomInteraction", "HookInteraction" };
        private bool isExpanded = true;
        private void OnEnable()
        {
            proceduresProp = serializedObject.FindProperty("procedures");

            reorderableList = new ReorderableList(serializedObject, proceduresProp, true, false, false, false); // Only drag-to-reorder

            reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
                SerializedProperty procedureTypeProp = element.FindPropertyRelative("procedureType");
                SerializedProperty stepNumberProp = element.FindPropertyRelative("stepNumber");

                EditorGUI.LabelField(
                    new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                    new GUIContent($"Step {stepNumberProp.intValue}: {procedureTypeProp.stringValue}")
                );
            };

            reorderableList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Procedures List");
            };

            reorderableList.onReorderCallback = (ReorderableList list) =>
            {
                UpdateStepNumbers();
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
                Repaint();
            };

            reorderableList.onChangedCallback = (ReorderableList list) =>
            {
                serializedObject.ApplyModifiedProperties();
            };
        }
        private void UpdateStepNumbers()
        {
            for (int i = 0; i < proceduresProp.arraySize; i++)
            {
                SerializedProperty procedure = proceduresProp.GetArrayElementAtIndex(i);
                SerializedProperty stepNumberProp = procedure.FindPropertyRelative("stepNumber");

                stepNumberProp.intValue = i + 1;  // Step numbers are 1-based
            }
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.LabelField("Procedures", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            jsonFileName = EditorGUILayout.TextField("JSON File Name", jsonFileName);
            isExpanded = EditorGUILayout.Foldout(isExpanded, "Procedures List", true, EditorStyles.foldoutHeader);

            if (isExpanded)
            {
                reorderableList.DoLayoutList(); // Draw the reorderable list
            }
            for (int i = 0; i < proceduresProp.arraySize; i++)
            {
                SerializedProperty procedure = proceduresProp.GetArrayElementAtIndex(i);
                SerializedProperty stepNumberProp = procedure.FindPropertyRelative("stepNumber"); // Step number
                SerializedProperty typeProp = procedure.FindPropertyRelative("procedureType");
                SerializedProperty isExpandedProp = procedure.FindPropertyRelative("isExpanded");
                SerializedProperty gameObjectsProp = procedure.FindPropertyRelative("gameObject");
                SerializedProperty animationsProp = procedure.FindPropertyRelative("animations");
                SerializedProperty descriptionAudioProp = procedure.FindPropertyRelative("stepDescriptionAudioClips");
                SerializedProperty interactionAudioPro = procedure.FindPropertyRelative("stepInteractionAudioClip");
                SerializedProperty textsProp = procedure.FindPropertyRelative("texts");
                SerializedProperty videoProp = procedure.FindPropertyRelative("videoClips");
                SerializedProperty genericObjects = procedure.FindPropertyRelative("genericObjects");
                SerializedProperty delayProp = procedure.FindPropertyRelative("delay");
                SerializedProperty confirmationProp = procedure.FindPropertyRelative("userConfirmation"); // Fix incorrect property name
                SerializedProperty highlightObjectProp = procedure.FindPropertyRelative("highlightGameObject");
                SerializedProperty retainInHandsProp = procedure.FindPropertyRelative("retainInHands");
                SerializedProperty triggerBasedProp = procedure.FindPropertyRelative("triggerMode");
                SerializedProperty confirmationTextProp = procedure.FindPropertyRelative("confirmationText");
                SerializedProperty droplocationObject = procedure.FindPropertyRelative("dropLocation");
                SerializedProperty collidingObjectProp = procedure.FindPropertyRelative("collidingObject");
                SerializedProperty impactedObjectProp = procedure.FindPropertyRelative("impactedObject");
                SerializedProperty particleSystemProp = procedure.FindPropertyRelative("particleSystemObject");
                SerializedProperty glovesObjectProp = procedure.FindPropertyRelative("glovesObject");
                //SerializedProperty glovesObjectPropRight = procedure.FindPropertyRelative("glovesObjectRight");
                SerializedProperty otherSafetyProp = procedure.FindPropertyRelative("otherSafetyObjects");
                SerializedProperty hookPickAttachProp = procedure.FindPropertyRelative("pickObject");
                SerializedProperty hookAttachTargetProp = procedure.FindPropertyRelative("hookAttachTarget");
                SerializedProperty climbListProp = procedure.FindPropertyRelative("climbInteractors");

                SerializedProperty enablePositionProp = procedure.FindPropertyRelative("customPosition");
                SerializedProperty enableRotationProp = procedure.FindPropertyRelative("customRotation");
                SerializedProperty PositioningObjectProp = procedure.FindPropertyRelative("objectsToPosition");
                SerializedProperty snapPointsForPositioningObjectsProp = procedure.FindPropertyRelative("snapPointsForPositioningObjects");
                SerializedProperty endSnapPointProp = procedure.FindPropertyRelative("endSnapPoint");
                SerializedProperty endObjectProp = procedure.FindPropertyRelative("endObject");

                stepNumberProp.intValue = i + 1;  // Auto-assign step number

                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.BeginHorizontal();

                //  isExpandedProp.boolValue = EditorGUILayout.Foldout(isExpandedProp.boolValue, typeProp.stringValue, true, EditorStyles.foldoutHeader);
                isExpandedProp.boolValue = EditorGUILayout.Foldout(isExpandedProp.boolValue, $"Step {stepNumberProp.intValue}: {typeProp.stringValue}", true, EditorStyles.foldoutHeader);

                int selectedIndex = System.Array.IndexOf(topLevelOptions, typeProp.stringValue);
                if (selectedIndex < 0) selectedIndex = 0;

                int newSelectedIndex = EditorGUILayout.Popup(selectedIndex, topLevelOptions);
                if (newSelectedIndex != selectedIndex)
                {
                    typeProp.stringValue = topLevelOptions[newSelectedIndex];
                }

                if (GUILayout.Button("-", GUILayout.Width(30)))
                {
                    proceduresProp.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                    break;
                }
                EditorGUILayout.EndHorizontal();

                if (isExpandedProp.boolValue)
                {
                    GUILayout.Space(5);
                    EditorGUILayout.LabelField("Step Number: " + stepNumberProp.intValue, EditorStyles.boldLabel);

                    if (typeProp.stringValue == "PickAndDrop")
                    {
                        DrawSingleGameObjectField(gameObjectsProp, "GameObject to pick");
                        DrawSingleGameObjectField(droplocationObject, "DropLocation");
                        DrawHighlightGameObjectListField(highlightObjectProp, "Highlight Objects");
                        DrawAnimationList("Animations", animationsProp);
                        DrawList("StepDescription AudioClips", descriptionAudioProp, typeof(AudioClip));
                        DrawList("Video Clips", videoProp, typeof(VideoClip));
                        DrawList("Texts", textsProp, typeof(string));
                        // DrawSingleProperty("Animation Audio", interactionAudioPro, typeof(AudioClip));
                        SerializedProperty assemblyProp = procedure.FindPropertyRelative("assembly");
                        assemblyProp.boolValue = EditorGUILayout.Toggle("Assembly", assemblyProp.boolValue);
                        if (retainInHandsProp != null)
                        {
                            retainInHandsProp.boolValue = EditorGUILayout.Toggle("Retain in Hands", retainInHandsProp.boolValue);
                        }
                        else
                        {
                            Debug.LogWarning("retainInHandsProp is null");
                        }

                    }
                    else if (typeProp.stringValue == "CollisionType")
                    {

                        SerializedProperty useGenericCollisionProp = procedure.FindPropertyRelative("isHandCollision");

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Hand Collision", GUILayout.Width(150));
                        useGenericCollisionProp.boolValue = EditorGUILayout.Toggle(useGenericCollisionProp.boolValue);
                        EditorGUILayout.EndHorizontal();

                        if (!useGenericCollisionProp.boolValue)
                        {
                            DrawSingleGameObjectField(collidingObjectProp, "Colliding Object");
                        }

                        DrawSingleGameObjectField(impactedObjectProp, "Impacted Object");
                        DrawHighlightGameObjectListField(highlightObjectProp, "Highlight Objects");
                        DrawList("StepDescription AudioClips", descriptionAudioProp, typeof(AudioClip));
                        DrawList("Video Clips", videoProp, typeof(VideoClip));
                        DrawList("Texts", textsProp, typeof(string));
                        triggerBasedProp.boolValue = EditorGUILayout.Toggle("Trigger Mode", triggerBasedProp.boolValue);
                    }
                    else if (typeProp.stringValue == "Animation")
                    {
                        DrawHighlightGameObjectListField(highlightObjectProp, "Highlight Objects");
                        DrawAnimationList("Animations", animationsProp);
                        DrawList("StepDescription AudioClips", descriptionAudioProp, typeof(AudioClip));
                        DrawList("Video Clips", videoProp, typeof(VideoClip));
                        DrawList("Texts", textsProp, typeof(string));
                    }

                    else if (typeProp.stringValue == "CustomInteraction")
                    {
                        enablePositionProp.boolValue = EditorGUILayout.Toggle("Custom Position", enablePositionProp.boolValue);
                        if (enablePositionProp.boolValue)
                        {
                            EditorGUILayout.BeginVertical("box");
                            EditorGUILayout.LabelField("Position Interaction", EditorStyles.boldLabel);

                            DrawHighlightGameObjectListField(PositioningObjectProp, "Objects");
                            DrawHighlightGameObjectListField(snapPointsForPositioningObjectsProp, "Snap Location Object");
                            DrawSingleGameObjectField(endSnapPointProp, "End Position");
                            DrawSingleGameObjectField(endObjectProp, "End Object");

                            EditorGUILayout.EndVertical();
                        }

                        enableRotationProp.boolValue = EditorGUILayout.Toggle("Custom Rotation", enableRotationProp.boolValue);
                        if (enableRotationProp.boolValue)
                        {
                            EditorGUILayout.BeginVertical("box");
                            EditorGUILayout.LabelField("Rotation Interaction", EditorStyles.boldLabel);

                            // Add any unique rotation logic if needed in future

                            EditorGUILayout.EndVertical();
                        }

                        // Shared fields for both Position and Rotation
                        EditorGUILayout.BeginVertical("box");
                        // EditorGUILayout.LabelField("Shared Interaction Data", EditorStyles.boldLabel);

                        DrawHighlightGameObjectListField(highlightObjectProp, "Highlight Objects");
                        DrawList("StepDescription AudioClips", descriptionAudioProp, typeof(AudioClip));
                        DrawList("Texts", textsProp, typeof(string));
                        DrawList("Video Clips", videoProp, typeof(VideoClip));
                        DrawAnimationList("Animations", animationsProp);

                        delayProp.intValue = EditorGUILayout.IntField("Delay (seconds)", delayProp.intValue);
                        confirmationProp.boolValue = EditorGUILayout.Toggle("Requires User Confirmation", confirmationProp.boolValue);
                        if (confirmationProp.boolValue)
                        {
                            confirmationTextProp.stringValue = EditorGUILayout.TextField("Confirmation Text", confirmationTextProp.stringValue);
                        }

                        EditorGUILayout.EndVertical();
                    }


                    else if (typeProp.stringValue == "Teleport")
                    {
                        DrawSingleGameObjectField(gameObjectsProp, "TeleportPoint");
                        DrawHighlightGameObjectListField(highlightObjectProp, "Highlight Object");
                        DrawList("StepDescription AudioClips", descriptionAudioProp, typeof(AudioClip));

                        DrawList("Texts", textsProp, typeof(string));
                    }
                    else if (typeProp.stringValue == "General")
                    {
                        DrawSingleGameObjectField(particleSystemProp, "Particle object");
                        DrawList("StepDescription AudioClips", descriptionAudioProp, typeof(AudioClip));
                        DrawList("Texts", textsProp, typeof(string));
                        DrawList("Video Clips", videoProp, typeof(VideoClip));
                    }
                    else if (typeProp.stringValue == "Pick" || typeProp.stringValue == "Drop")
                    {

                        DrawSingleGameObjectField(gameObjectsProp, "GameObject");
                        DrawHighlightGameObjectListField(highlightObjectProp, "Highlight Objects");
                        DrawAnimationList("Animations", animationsProp);
                        DrawList("StepDescription AudioClips", descriptionAudioProp, typeof(AudioClip));
                        DrawList("Video Clips", videoProp, typeof(VideoClip));
                        DrawList("Texts", textsProp, typeof(string));
                        //    DrawSingleProperty("Animation Audio", interactionAudioPro, typeof(AudioClip));
                        if (typeProp.stringValue == "Drop")
                        {
                            DrawSingleGameObjectField(droplocationObject, "DropLocation");
                        }

                        if (retainInHandsProp != null)
                        {
                            retainInHandsProp.boolValue = EditorGUILayout.Toggle("Retain in Hands", retainInHandsProp.boolValue);
                        }
                        else
                        {
                            Debug.LogWarning("retainInHandsProp is null");
                        }
                    }
                    else if (typeProp.stringValue == "SafetyMeasurement")
                    {
                        DrawSingleGameObjectField(glovesObjectProp, "Gloves");
                        DrawList("Other safety Objects", otherSafetyProp, typeof(GameObject));
                    }
                    else if (typeProp.stringValue == "HookInteraction")
                    {

                        EditorGUILayout.LabelField("Hook Interaction", EditorStyles.boldLabel);

                        EditorGUILayout.BeginVertical("box");
                        DrawSingleGameObjectField(hookPickAttachProp, "Object to Pick (Attach)");
                        DrawSingleGameObjectField(hookAttachTargetProp, "AttachLocation");
                        DrawClimbInteractorListField(climbListProp, "Climb Interactors");
                        EditorGUILayout.EndVertical();

                        DrawHighlightGameObjectListField(highlightObjectProp, "Highlight Objects");
                        DrawList("StepDescription AudioClips", descriptionAudioProp, typeof(AudioClip));
                        DrawList("Texts", textsProp, typeof(string));
                        DrawList("Video Clips", videoProp, typeof(VideoClip));
                        DrawAnimationList("Animations)", animationsProp);
                    }
                    if (typeProp.stringValue != "SafetyMeasurement")
                    {
                        DrawSingleProperty("Interaction AudioClip", interactionAudioPro, typeof(AudioClip));
                        delayProp.intValue = EditorGUILayout.IntField("Delay (seconds)", delayProp.intValue);
                        confirmationProp.boolValue = EditorGUILayout.Toggle("Requires User Confirmation", confirmationProp.boolValue);
                        if (confirmationProp.boolValue)
                        {
                            confirmationTextProp.stringValue = EditorGUILayout.TextField("Confirmation Text", confirmationTextProp.stringValue);
                        }
                        else
                        {
                            confirmationTextProp.stringValue = "";
                        }
                        DrawSelectableObjectListField(genericObjects, "Generic Objects");
                    }

                }

                EditorGUILayout.EndVertical();
                GUILayout.Space(25);
            }

            if (GUILayout.Button("Add Procedure", GUILayout.Height(30)))
            {
                proceduresProp.arraySize++;
                SerializedProperty newProcedure = proceduresProp.GetArrayElementAtIndex(proceduresProp.arraySize - 1);
                newProcedure.FindPropertyRelative("procedureType").stringValue = "PickAndDrop";
                newProcedure.FindPropertyRelative("isExpanded").boolValue = true;
                newProcedure.FindPropertyRelative("delay").intValue = 0;
                newProcedure.FindPropertyRelative("userConfirmation").boolValue = false;
                newProcedure.FindPropertyRelative("stepNumber").intValue = proceduresProp.arraySize;  // Assign correct step number
            }
            if (GUILayout.Button("Save to JSON", GUILayout.Height(30)))
            {
                SaveToJson();
            }

            serializedObject.ApplyModifiedProperties();
        }
        private void DrawSelectableObjectListField(SerializedProperty listProp, string label)
        {
            if (listProp == null)
            {
                Debug.LogWarning($"{label} property is null.");
                return;
            }

            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            for (int i = 0; i < listProp.arraySize; i++)
            {
                SerializedProperty item = listProp.GetArrayElementAtIndex(i);
                SerializedProperty objProp = item.FindPropertyRelative("gameObject");
                SerializedProperty selectedProp = item.FindPropertyRelative("isSelected");

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PropertyField(objProp, GUIContent.none);
                selectedProp.boolValue = EditorGUILayout.Toggle("Selected", selectedProp.boolValue);

                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    listProp.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                    GUIUtility.ExitGUI();
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+", GUILayout.Height(25)))
            {
                listProp.InsertArrayElementAtIndex(listProp.arraySize);
                SerializedProperty newItem = listProp.GetArrayElementAtIndex(listProp.arraySize - 1);
                newItem.FindPropertyRelative("gameObject").objectReferenceValue = null;
                newItem.FindPropertyRelative("isSelected").boolValue = false;

                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawClimbInteractorListField(SerializedProperty listProp, string label)
        {
            if (listProp == null)
            {
                Debug.LogWarning($"{label} property is null.");
                return;
            }

            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            for (int i = 0; i < listProp.arraySize; i++)
            {
                SerializedProperty item = listProp.GetArrayElementAtIndex(i);
                SerializedProperty objProp = item.FindPropertyRelative("climbInteractorgameObject");
                SerializedProperty boolProp = item.FindPropertyRelative("isEnabled");

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PropertyField(objProp, GUIContent.none);
                boolProp.boolValue = EditorGUILayout.Toggle("Climb Enabled", boolProp.boolValue);

                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    listProp.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                    GUIUtility.ExitGUI();
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            if (GUILayout.Button("+", GUILayout.Height(25)))
            {
                listProp.InsertArrayElementAtIndex(listProp.arraySize);
                SerializedProperty newItem = listProp.GetArrayElementAtIndex(listProp.arraySize - 1);

                newItem.FindPropertyRelative("climbInteractorgameObject").objectReferenceValue = null;
                newItem.FindPropertyRelative("isEnabled").boolValue = false;

                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawSingleGameObjectField(SerializedProperty gameObjectProp, string label)
        {
            if (gameObjectProp == null)
            {
                Debug.LogWarning($"{label} property is null.");
                return;
            }

            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            // Display the GameObject field
            EditorGUILayout.PropertyField(gameObjectProp, GUIContent.none);

            // Clear button to remove the assigned GameObject
            if (gameObjectProp.objectReferenceValue != null)
            {
                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    gameObjectProp.objectReferenceValue = null;
                    serializedObject.ApplyModifiedProperties();
                    GUIUtility.ExitGUI();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawHighlightGameObjectListField(SerializedProperty gameObjectProp, string label)
        {
            if (gameObjectProp == null)
            {
                Debug.LogWarning($"{label} property is null.");
                return;
            }

            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            for (int i = 0; i < gameObjectProp.arraySize; i++)
            {
                SerializedProperty itemProp = gameObjectProp.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PropertyField(itemProp, GUIContent.none);

                if (itemProp.objectReferenceValue != null && GUILayout.Button("🔍", GUILayout.Width(30)))
                {
                    Selection.activeGameObject = itemProp.objectReferenceValue as GameObject;
                    SceneView.lastActiveSceneView.FrameSelected();
                }

                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    gameObjectProp.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                    GUIUtility.ExitGUI();
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+", GUILayout.Height(25)))
            {
                gameObjectProp.InsertArrayElementAtIndex(gameObjectProp.arraySize);
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawAnimationList(string label, SerializedProperty animationEntryProp)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            SerializedProperty gameObjectProp = animationEntryProp.FindPropertyRelative("targetGameObject");
            SerializedProperty clipProp = animationEntryProp.FindPropertyRelative("animationClip");
            SerializedProperty startFrameProp = animationEntryProp.FindPropertyRelative("startFrame");
            SerializedProperty endFrameProp = animationEntryProp.FindPropertyRelative("endFrame");
            EditorGUILayout.BeginHorizontal();  // Start horizontal layout
            EditorGUILayout.PropertyField(gameObjectProp, GUIContent.none);
            EditorGUILayout.PropertyField(clipProp, GUIContent.none);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginVertical();
            startFrameProp.floatValue = EditorGUILayout.FloatField("Start Frame", startFrameProp.floatValue);
            endFrameProp.floatValue = EditorGUILayout.FloatField("End Frame", endFrameProp.floatValue);
            EditorGUILayout.EndVertical(); // Close vertical layout group
        }

        private void DrawList(string label, SerializedProperty listProp, System.Type type)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            for (int i = 0; i < listProp.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                SerializedProperty itemProp = listProp.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(itemProp, GUIContent.none);

                if (GUILayout.Button("-", GUILayout.Width(30)))
                {
                    listProp.DeleteArrayElementAtIndex(i);
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+", GUILayout.Height(25)))
            {
                listProp.arraySize++;
            }
        }
        private void DrawSingleProperty(string label, SerializedProperty property, System.Type type)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(property, GUIContent.none);
        }


        private void SaveToJson()
        {
            HierarchicalData data = (HierarchicalData)target;
            string fileName = jsonFileName;
            List<ProcedureData> jsonData = new List<ProcedureData>();
            int stepCounter = 1;

            foreach (var procedure in data.procedures)
            {
                ProcedureData procData = new ProcedureData
                {
                    stepNumber = stepCounter++,
                    procedureType = procedure.procedureType,
                    delay = procedure.delay,
                    userConfirmation = procedure.userConfirmation,
                    confirmationText = procedure.confirmationText,
                    stepDescriptionAudioClips = procedure.stepDescriptionAudioClips.Where(a => a != null).Select(a => a.name).ToList(),
                    videoClips = procedure.videoClips.Where(v => v != null).Select(v => v.name).ToList(),
                    particleSystemObject = procedure.particleSystemObject != null ? procedure.particleSystemObject.name : null,
                    texts = new List<string>(procedure.texts),
                    highlightGameObject = procedure.highlightGameObject.Where(go => go != null).Select(go => go.name).ToList(),
                    retainInHands = procedure.retainInHands,
                    triggerMode = procedure.triggerMode,
                    genericObjects = procedure.genericObjects
                    .Where(so => so.gameObject != null)
                    .Select(so => new GenericObjectData
                    {
                        genericObjectParent = so.gameObject.transform.parent.name,
                        gameObjectName = so.gameObject.name,
                        isSelected = so.isSelected
                    })
                    .ToList()
                };

                // Handle different procedure types
                if (procedure.procedureType == "PickAndDrop" || procedure.procedureType == "Pick" || procedure.procedureType == "Drop")
                {
                    procData.gameObject = procedure.gameObject != null ? procedure.gameObject.name : null;
                    //  procData.stepInteractionAudioClip = procedure.stepInteractionAudioClip.name;
                    procData.stepInteractionAudioClip = procedure.stepInteractionAudioClip != null ? procedure.stepInteractionAudioClip.name : null;

                    // Handle animation for Pick/Drop/PickAndDrop
                    var animationEntry = procedure.animations;
                    if (animationEntry.targetGameObject != null && animationEntry.animationClip != null)
                    {
                        procData.animations = new AnimationData
                        {
                            gameObjectName = animationEntry.targetGameObject.name,
                            animationClipName = animationEntry.animationClip.name,
                            startFrame = animationEntry.startFrame,
                            endFrame = animationEntry.endFrame
                        };
                    }

                    // Drop location for PickAndDrop/Drop
                    if (procedure.procedureType == "Drop" || procedure.procedureType == "PickAndDrop")
                        procData.dropLocation = procedure.dropLocation != null ? procedure.dropLocation.name : null;

                    // Assembly for PickAndDrop
                    if (procedure.procedureType == "PickAndDrop")
                        procData.assembly = procedure.assembly;
                }

                else if (procedure.procedureType == "Animation")
                {
                    //  procData.stepInteractionAudioClip = procedure.stepInteractionAudioClip.name;
                    procData.stepInteractionAudioClip = procedure.stepInteractionAudioClip != null ? procedure.stepInteractionAudioClip.name : null;

                    // Handle animation for Animation type
                    var animationEntry = procedure.animations;
                    if (animationEntry.targetGameObject != null && animationEntry.animationClip != null)
                    {
                        procData.animations = new AnimationData
                        {
                            gameObjectName = animationEntry.targetGameObject.name,
                            animationClipName = animationEntry.animationClip.name,
                            startFrame = animationEntry.startFrame,
                            endFrame = animationEntry.endFrame
                        };
                    }
                }
                else if (procedure.procedureType == "HookInteraction")
                {
                    procData.climbInteractors = procedure.climbInteractors
           .Where(item => item.climbInteractorgameObject != null)
           .Select(item => new ClimbInteractorObject
           {
               climbInteractorgameObject = item.climbInteractorgameObject.name,
               isEnabled = item.isEnabled
           })
           .ToList();

                    procData.pickObject = procedure.pickObject != null ? procedure.pickObject.name : null;
                    procData.hookAttachTarget = procedure.hookAttachTarget != null ? procedure.hookAttachTarget.name : null;
                }

                else if (procedure.procedureType == "CustomInteraction")
                {
                    procData.customPosition = procedure.customPosition; // or your actual bool field
                    procData.customRotation = procedure.customRotation;

                    procData.objectsToPosition = procedure.objectsToPosition
                        .Where(go => go != null)
                        .Select(go => go.name)
                        .ToList();

                    procData.snapPointsForPositioningObjects = procedure.snapPointsForPositioningObjects
                        .Where(go => go != null)
                        .Select(go => go.name)
                        .ToList();

                    procData.endSnapPoint = procedure.endSnapPoint != null ? procedure.endSnapPoint.name : null;
                    procData.endObject = procedure.endObject != null ? procedure.endObject.name : null;
                }

                else if (procedure.procedureType == "CollisionType")
                {
                    //procData.stepInteractionAudioClip = procedure.stepInteractionAudioClip.name;
                    procData.stepInteractionAudioClip = procedure.stepInteractionAudioClip != null ? procedure.stepInteractionAudioClip.name : null;

                    procData.isHandCollision = procedure.isHandCollision;

                    if (!procedure.isHandCollision)
                    {
                        procData.collidingObject = procedure.collidingObject != null ? procedure.collidingObject.name : null;

                    }

                    procData.impactedObject = procedure.impactedObject != null ? procedure.impactedObject.name : null;
                }

                else if (procedure.procedureType == "Teleport")
                {
                    procData.stepInteractionAudioClip = procedure.stepInteractionAudioClip != null ? procedure.stepInteractionAudioClip.name : null;

                    procData.gameObject = procedure.gameObject != null ? procedure.gameObject.name : null;
                    procData.teleportParent = procedure.gameObject != null && procedure.gameObject.transform.parent != null
                        ? procedure.gameObject.transform.parent.name
                        : null;
                }

                else if (procedure.procedureType == "General")
                {
                    procData.stepInteractionAudioClip = procedure.stepInteractionAudioClip != null ? procedure.stepInteractionAudioClip.name : null;
                }
                else if (procedure.procedureType == "SafetyMeasurement")
                {
                    procData.glovesObject = procedure.glovesObject != null ? procedure.glovesObject.name : null;
                    procData.otherSafetyObjects = procedure.otherSafetyObjects.Where(go => go != null).Select(go => go.name).ToList();

                }
                jsonData.Add(procData);
            }

            HierarchicalDataContainer container = new HierarchicalDataContainer { procedures = jsonData };
            string json = JsonUtility.ToJson(container, true);
            //  string filePath = Path.Combine(Application.persistentDataPath, fileName + ".json");
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string folderPath = Path.Combine(desktopPath, "SequenceManagerData");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, fileName + ".json");
            // Save the JSON to a file
            File.WriteAllText(filePath, json);
            Debug.Log("JSON Saved to: " + filePath);
        }

        [System.Serializable]
        public class ProcedureData
        {
            public string procedureType;
            public int stepNumber;
            public string gameObject;
            public string dropLocation;
            public string collidingObject;
            public string impactedObject;
            public string particleSystemObject;
            public AnimationData animations;
            public List<GenericObjectData> genericObjects;
            public List<ClimbInteractorObject> climbInteractors;
            public List<string> stepDescriptionAudioClips;
            public List<string> texts;
            public List<string> videoClips;
            public string glovesObject;
            public List<string> otherSafetyObjects;
            public int delay;
            public string teleportParent;
            public bool userConfirmation;
            public List<string> highlightGameObject;
            public bool assembly;
            public string confirmationText;
            public bool retainInHands;
            public bool triggerMode;
            public bool isHandCollision;
            public string stepInteractionAudioClip;
            public string pickObject;
            public string hookAttachTarget;

            public List<string> objectsToPosition = new List<string>();
            public List<string> snapPointsForPositioningObjects = new List<string>();
            public string endSnapPoint;
            public string endObject;

            public bool customPosition = false;
            public bool customRotation = false;

            public ProcedureData()
            {
                animations = null;
                stepDescriptionAudioClips = null;
                texts = null;
                videoClips = null;
            }
        }

        [System.Serializable]
        public class AnimationData
        {
            public string gameObjectName;
            public string animationClipName;
            public float startFrame;
            public float endFrame;
        }
        [System.Serializable]
        public class GenericObjectData
        {
            public string gameObjectName;
            public string genericObjectParent;
            public bool isSelected;
        }
        [System.Serializable]
        public class ClimbInteractorObject
        {
            public string climbInteractorgameObject;
            public bool isEnabled;
        }
        [System.Serializable]
        public class HierarchicalDataContainer
        {
            public List<ProcedureData> procedures;
        }
    }
}
#endif