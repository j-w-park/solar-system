#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public sealed class ReadOnlyPropertyDrawer : PropertyDrawer
{

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        bool previousGUIState = GUI.enabled;
        GUI.enabled = false; // Disable editing
        EditorGUI.PropertyField(position, property, label);
        GUI.enabled = previousGUIState; // Restore previous GUI state
    }
}
#endif