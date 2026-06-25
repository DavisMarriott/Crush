using UnityEditor;
using UnityEngine;

// Type dropdown, then only the field that type uses: a tag for BranchTag, a loop number for LoopReached.
[CustomPropertyDrawer(typeof(DraftUnlockCondition))]
public class DraftUnlockConditionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var typeProp = property.FindPropertyRelative("type");
        float h = EditorGUIUtility.singleLineHeight;
        float gap = EditorGUIUtility.standardVerticalSpacing;

        var line = new Rect(position.x, position.y, position.width, h);
        EditorGUI.PropertyField(line, typeProp, label);

        line.y += h + gap;
        EditorGUI.indentLevel++;
        if ((DraftUnlockType)typeProp.enumValueIndex == DraftUnlockType.BranchTag)
            EditorGUI.PropertyField(line, property.FindPropertyRelative("tag"), new GUIContent("Branch Tag"));
        else
            EditorGUI.PropertyField(line, property.FindPropertyRelative("loop"), new GUIContent("Loop Reached"));
        EditorGUI.indentLevel--;

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
}
