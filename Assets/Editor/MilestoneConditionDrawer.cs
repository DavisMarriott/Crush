using UnityEngine;
using UnityEditor;

// Draws MilestoneCondition so only the field that matters for the chosen Type shows:
//   DialogueTagFired -> Tag
//   everything else  -> the number field (relabeled to fit the type)
// Keeps the inspector from implying every milestone needs a Tag / Threshold.
[CustomPropertyDrawer(typeof(MilestoneCondition))]
public class MilestoneConditionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
        EditorGUI.BeginProperty(pos, label, prop);

        var typeProp = prop.FindPropertyRelative("type");
        var thresholdProp = prop.FindPropertyRelative("threshold");
        var tagProp = prop.FindPropertyRelative("tag");

        float line = EditorGUIUtility.singleLineHeight;
        float sp = EditorGUIUtility.standardVerticalSpacing;
        var row = new Rect(pos.x, pos.y, pos.width, line);

        EditorGUI.PropertyField(row, typeProp);
        row.y += line + sp;

        var type = (MilestoneConditionType)typeProp.enumValueIndex;
        if (type == MilestoneConditionType.DialogueTagFired)
        {
            EditorGUI.PropertyField(row, tagProp);
        }
        else
        {
            string lbl = type switch
            {
                MilestoneConditionType.LoopReached => "Loop #",
                MilestoneConditionType.PeakConfidenceReached => "Peak Confidence",
                MilestoneConditionType.PeakCharmReached => "Peak Charm",
                MilestoneConditionType.CardsPlayedInLoopAtLeast => "Min Cards Played",
                _ => "Threshold",
            };
            EditorGUI.PropertyField(row, thresholdProp, new GUIContent(lbl));
        }

        EditorGUI.EndProperty();
    }

    // Always two rows: Type + the one relevant field.
    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
    }
}
