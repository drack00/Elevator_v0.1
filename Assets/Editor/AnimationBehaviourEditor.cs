using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(AnimationBehaviour))]
public class AnimationBehaviourEditor : Editor {
    public static readonly string[] excluding = new string[]
    {
        "blocking",
        "updateOn"
    };

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        AnimationBehaviour ab = target as AnimationBehaviour;

        EditorGUI.BeginChangeCheck();

        DrawPropertiesExcluding(serializedObject, excluding);

        ab.blocking = (AnimationBehaviour.Blocking)EditorGUILayout.EnumMaskField("Blocking", (System.Enum)ab.blocking);
        ab.updateOn = (AnimationBehaviour.UpdateOn)EditorGUILayout.EnumPopup("Update On", (System.Enum)ab.updateOn);

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(ab);
        }
    }
}
