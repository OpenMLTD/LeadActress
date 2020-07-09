#define USE_CUSTOM_TRANSFORM_INSPECTOR
// #undef USE_CUSTOM_TRANSFORM_INSPECTOR

#if USE_CUSTOM_TRANSFORM_INSPECTOR
using UnityEditor;
using UnityEngine;

namespace LeadActress.Editor {
    // http://answers.unity.com/answers/39371/view.html
    [CustomEditor(typeof(Transform))]
    public class WorldPositionEditor : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            var t = (Transform)target;

            EditorGUILayout.BeginFoldoutHeaderGroup(false, "Local");
            EditorGUILayout.BeginHorizontal();
            t.localPosition = EditorGUILayout.Vector3Field("Position", t.localPosition);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            t.localEulerAngles = EditorGUILayout.Vector3Field("Rotation", t.localEulerAngles);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            t.localScale = EditorGUILayout.Vector3Field("Scale", t.localScale);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndFoldoutHeaderGroup();

            _showWorldPositions = EditorGUILayout.BeginFoldoutHeaderGroup(_showWorldPositions, "World (read-only)");

            if (_showWorldPositions) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Vector3Field("Position", t.position);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Vector3Field("Rotation", t.eulerAngles);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private bool _showWorldPositions = true;

    }
}
#endif
