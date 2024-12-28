using System;
using System.IO;
using DemoX.Framework.Core;
using UnityEditor;
using UnityEngine;

namespace DemoX.Editor
{
    [CustomPropertyDrawer(typeof(SceneField))]
    public class SceneFieldPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                EditorGUI.BeginProperty(position, label, property);

                // Draw the label
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

                // Draw the scene selection popup
                if (EditorBuildSettings.scenes.Length > 0)
                {
                    string[] sceneNames = new string[EditorBuildSettings.scenes.Length];
                    for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                    {
                        sceneNames[i] = Path.GetFileNameWithoutExtension(EditorBuildSettings.scenes[i].path);
                    }

                    int selectedSceneIndex = Mathf.Max(0, Array.IndexOf(sceneNames, property.stringValue));
                    selectedSceneIndex = EditorGUI.Popup(position, selectedSceneIndex, sceneNames);

                    if (selectedSceneIndex >= 0 && selectedSceneIndex < sceneNames.Length)
                    {
                        property.stringValue = sceneNames[selectedSceneIndex];
                    }
                }
                else
                {
                    EditorGUI.LabelField(position, "No scenes in build settings");
                }

                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.HelpBox(position, "Property type is not supported", MessageType.Error);
            }
        }
    }
}