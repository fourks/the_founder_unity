using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(Technology))]
internal class TechnologyInspector : Editor {

    Technology d;

    void OnEnable() {
        d = target as Technology;
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorStyles.textField.wordWrap = true;
        EditorGUILayout.LabelField("Description");
        d.description = EditorGUILayout.TextArea(d.description, GUILayout.Height(50));
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Icon");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"), GUIContent.none);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Research Needed");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cost"), GUIContent.none);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Required Vertical");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("requiredVertical"), GUIContent.none);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Required Technologies");
        for (int i=0; i < d.requiredTechnologies.Count; i++) {
            EditorGUILayout.BeginHorizontal();
            d.requiredTechnologies[i] = (Technology)EditorGUILayout.ObjectField(d.requiredTechnologies[i], typeof(Technology), false);
            if (GUILayout.Button("Delete")) {
                d.requiredTechnologies.Remove(d.requiredTechnologies[i]);
            }
            EditorGUILayout.EndHorizontal();
        }
        if (GUILayout.Button("Add Required Technology")) {
            d.requiredTechnologies.Add(null);
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Effects");
        if (d.effects == null)
            d.effects = new EffectSet();
        EffectSetRenderer.RenderEffectSet(d, d.effects);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("effects").FindPropertyRelative("unlocks"), GUIContent.none);
        EditorGUILayout.Space();

        if (GUI.changed) {
            EditorUtility.SetDirty(target);
            serializedObject.ApplyModifiedProperties();
        }
    }

}
