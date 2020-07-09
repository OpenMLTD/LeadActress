using UnityEditor;

namespace LeadActress.Editor {
    public static class CustomEditorMenus {

        [MenuItem("Assets/Create/ScriptableObject")]
        public static void OpenScriptableObjectCreatorWindow() {
            var window = EditorWindow.CreateWindow<ScriptableObjectCreatorWindow>("Scriptable Object Creator");
            window.Show();
        }

    }
}
