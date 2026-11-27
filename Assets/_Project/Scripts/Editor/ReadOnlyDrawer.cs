using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label)
    {
        return EditorGUI.GetPropertyHeight(_property, _label, true);
    }

    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUI.PropertyField(_position, _property, _label, true);
        }
    }
}
