using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using LeadActress.Runtime.Dancing;
using UnityEditor;
using UnityEngine;

namespace LeadActress.Editor {
    public class ScriptableObjectCreatorWindow : EditorWindow {

        private void OnEnable() {
            var assembly = typeof(MltdModelAnimator).Assembly;
            var types = assembly.GetTypes();
            var sobjTypes = new List<Type>();

            foreach (var type in types) {
                if (type.IsSubclassOf(typeof(ScriptableObject))) {
                    sobjTypes.Add(type);
                }
            }

            _sobjTypes = sobjTypes.ToArray();
            _typeNames = sobjTypes.Select(t => t.Name).ToArray();
        }

        private void OnGUI() {
            if (_sobjTypes.Length == 0) {
                EditorGUILayout.LabelField("No ScriptableObject types.", GUIStyle.none);
            } else {
                EditorGUILayout.BeginHorizontal();
                _selectedTypeIndex = EditorGUILayout.Popup("Type", _selectedTypeIndex, _typeNames);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                _assetName = EditorGUILayout.TextField("Asset Name", _assetName);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                var clicked = GUILayout.Button("Create");
                EditorGUILayout.EndHorizontal();

                if (clicked) {
                    var type = _sobjTypes[_selectedTypeIndex];
                    if (CreateAsset(type, _assetName)) {
                        Close();
                    }
                }
            }
        }

        private static bool CreateAsset([NotNull] Type type, [NotNull] string assetName) {
            if (string.IsNullOrWhiteSpace(assetName)) {
                EditorUtility.DisplayDialog("Error", "Please enter asset name.", "Close");
                return false;
            }

            var activeObject = Selection.activeObject;
            var dir = AssetDatabase.GetAssetPath(activeObject);

            if (!Directory.Exists(dir)) {
                if (File.Exists(dir)) {
                    dir = (new FileInfo(dir)).DirectoryName;
                    dir = dir.Substring(Directory.GetCurrentDirectory().Length + 1);
                } else {
                    EditorUtility.DisplayDialog("Error", "Unknown selection.", "Close");
                    return false;
                }
            }

            var obj = CreateInstance(type);
            var assetPath = Path.Combine(dir, assetName);

            AssetDatabase.CreateAsset(obj, assetPath);

            var message = new StringBuilder();
            message.AppendLine("ScriptableObject created.");
            message.AppendLine($"Type: {type.Name}");
            message.Append($"Location: {assetPath}");

            EditorUtility.DisplayDialog("Info", message.ToString(), "OK");

            return true;
        }

        private Type[] _sobjTypes;

        private string[] _typeNames;

        private int _selectedTypeIndex;

        private string _assetName = string.Empty;

    }
}
