using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


#if UNITY_EDITOR
[CustomEditor(typeof(Oscillate))]
public class OscillateEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Axis Selection", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("animateX"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("animateY"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("animateZ"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Position Range", EditorStyles.boldLabel);

        if (((Oscillate)target).animateX)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("startX"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("endX"));
        }

        if (((Oscillate)target).animateY)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("startY"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("endY"));
        }

        if (((Oscillate)target).animateZ)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("startZ"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("endZ"));
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Speed", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("speed"));

        serializedObject.ApplyModifiedProperties();
    }
}
#endif