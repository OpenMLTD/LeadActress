using UnityEditor;
using UnityEngine;

namespace LeadActress.Editor {
    [CustomPropertyDrawer(typeof(LabelOverrideAttribute))]
    public class ThisPropertyDrawer : PropertyDrawer {

        // http://answers.unity.com/answers/1383657/view.html
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            try {
                var labelOverride = attribute as LabelOverrideAttribute;
                Debug.Assert(labelOverride != null);

                if (!IsItBloodyArrayTho(property)) {
                    label.text = labelOverride.Label;
                } else {
                    Debug.LogWarningFormat(nameof(LabelOverrideAttribute) + " (\"{0}\", as \"{1}\") doesn't support arrays ", label.text, labelOverride.Label);
                }

                EditorGUI.PropertyField(position, property, label);
            } catch (System.Exception ex) {
                Debug.LogException(ex);
            }
        }

        private static bool IsItBloodyArrayTho(SerializedProperty property) {
            var path = property.propertyPath;
            var idot = path.IndexOf('.');

            if (idot == -1) {
                return false;
            }

            var propName = path.Substring(0, idot);
            var p = property.serializedObject.FindProperty(propName);

            return p.isArray;
            //CREDITS: https://answers.unity.com/questions/603882/serializedproperty-isnt-being-detected-as-an-array.html
        }

    }
}
