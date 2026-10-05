using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ConfigSelector))]
public class ConfigSelectorEditor : Editor
{
    private SerializedProperty selectedConfigProp;

    private void OnEnable()
    {
        selectedConfigProp = serializedObject.FindProperty("selectedConfig");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");

        List<ScriptableObject> foundConfigs = new List<ScriptableObject>();
        List<string> options = new List<string>();

        // Берем сборку, в которой объявлен ConfigSelector
        var myAssembly = typeof(ConfigSelector).Assembly;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // Игнорируем системные пакеты Unity
            if (path.StartsWith("Packages/")) continue;

            ScriptableObject config = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (config == null) continue;

            Type type = config.GetType();

            if (type.Assembly != myAssembly) continue;

            if (type.Name.EndsWith("Installer")) continue;

            if (type.Name == "Readme") continue;

            foundConfigs.Add(config);
            options.Add($"{config.name} ({type.Name})");
        }

        // Находим текущий выбранный элемент
        ScriptableObject currentConfig = selectedConfigProp.objectReferenceValue as ScriptableObject;
        int currentIndex = foundConfigs.IndexOf(currentConfig);
        if (currentIndex < 0) currentIndex = 0;

        // Выводим конфиг
        int newIndex = EditorGUILayout.Popup("Выбранный конфиг", currentIndex, options.ToArray());

        if (newIndex != currentIndex || selectedConfigProp.objectReferenceValue == null)
        {
            selectedConfigProp.objectReferenceValue = foundConfigs[newIndex];
        }

        // Встроенный предпросмотр содержимого конфига
        if (selectedConfigProp.objectReferenceValue != null)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Содержимое выбранного конфига:", EditorStyles.boldLabel);

            CreateEditor(selectedConfigProp.objectReferenceValue).OnInspectorGUI();
        }

        serializedObject.ApplyModifiedProperties();
    }
}