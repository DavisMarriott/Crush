using UnityEditor;

// Shows the Progress-Gated unlock condition ONLY when Category == ProgressGated, so it appears the
// moment you pick that category (and stays hidden otherwise). editorForChildClasses → covers DanceCard.
[CustomEditor(typeof(DialogueCard), true)]
public class DialogueCardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // everything except the gate (drawn conditionally below)
        DrawPropertiesExcluding(serializedObject, "unlockCondition");

        var category = serializedObject.FindProperty("category");
        if (category != null && (CardCategory)category.enumValueIndex == CardCategory.ProgressGated)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("unlockCondition"), true);

        serializedObject.ApplyModifiedProperties();
    }
}
