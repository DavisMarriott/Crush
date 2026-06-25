using UnityEditor;
using UnityEngine;

// Shows the type dropdown, then ONLY the field that type uses: a number for PlayThreshold,
// a tag dropdown for BranchTag. (Unity auto-spaces the enum names → "Play Threshold" / "Branch Tag".)
[CustomPropertyDrawer(typeof(UpgradeCondition))]
public class UpgradeConditionDrawer : PropertyDrawer
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
        if ((UpgradeConditionType)typeProp.enumValueIndex == UpgradeConditionType.PlayThreshold)
            EditorGUI.PropertyField(line, property.FindPropertyRelative("playThreshold"), new GUIContent("Play Threshold"));
        else
            EditorGUI.PropertyField(line, property.FindPropertyRelative("tag"), new GUIContent("Branch Tag"));
        EditorGUI.indentLevel--;

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
    }
}
