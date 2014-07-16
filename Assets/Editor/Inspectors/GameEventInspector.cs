using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(GameEvent))]
internal class GameEventInspector : Editor {

    private Dictionary<string, ReorderableList> lists = new Dictionary<string, ReorderableList>();

    // The names of the list properties.
    private string[] listPropertyNames = new string[] {
        "workerEffects",
        "companyEffects"
    };

    GameEvent ge;

    void OnEnable() {
        ge = target as GameEvent;

        foreach (string key in listPropertyNames)
        {
            lists[key] = ShowList(serializedObject.FindProperty(key), key);
        }
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.LabelField("Description");
        ge.description = EditorGUILayout.TextArea(ge.description, GUILayout.Height(50));
        EditorGUILayout.Space();

        ge.probability.baseValue = EditorGUILayout.FloatField("Probability", ge.probability.baseValue);
        ge.probability.baseValue = Mathf.Clamp(ge.probability.baseValue, 0, 1);
        EditorGUILayout.Space();

        // Render the lists.
        foreach (string key in listPropertyNames)
        {
            lists[key].DoLayoutList();
            EditorGUILayout.Space();
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("unlocks"), true);
        EditorGUILayout.Space();

        // Product Effects
        // Have to handle this one specially cause nested lists are tricky...if not impossible.
        EditorGUILayout.LabelField("Product Effects");
        EditorGUI.indentLevel += 1;

        if (ge.productEffects == null) {
            ge.productEffects = new List<ProductEffect>();
        }

        for (int i=0; i < ge.productEffects.Count; i++) {
            ProductEffect e = ge.productEffects[i];

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Delete")) {
                ge.productEffects.Remove(e);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("productEffects").GetArrayElementAtIndex(i), true);
        }
        if (GUILayout.Button("Add New Product Effect")) {
            ge.productEffects.Add(new ProductEffect());
            EditorUtility.SetDirty(target);
        }
        EditorGUI.indentLevel -= 1;

        if (GUI.changed) {
            EditorUtility.SetDirty(target);
            serializedObject.ApplyModifiedProperties();
        }
    }

    private ReorderableList ShowList(SerializedProperty property, string labelText) {
        ReorderableList list = new UnityEditorInternal.ReorderableList(serializedObject, property, true, true, true, true);
        list.drawHeaderCallback += rect => GUI.Label(rect, labelText);
        list.drawElementCallback += (rect, index, active, focused) =>
        {
            rect.height = 16;
            rect.y += 2;
            EditorGUI.PropertyField(rect, 
                list.serializedProperty.GetArrayElementAtIndex(index), 
                GUIContent.none);
        };
        return list;
    }

}
