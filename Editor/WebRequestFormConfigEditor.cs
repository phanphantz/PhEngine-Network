using UnityEditor;
using UnityEngine;

namespace PhEngine.Network.Editor
{
    [CustomEditor(typeof(WebRequestFormConfig)), CanEditMultipleObjects]
    public class WebRequestFormConfigEditor : UnityEditor.Editor
    {
        WebRequestFormConfig config;
        
        void OnEnable()
        {
            config = target as WebRequestFormConfig;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Test", GUILayout.Width(50), GUILayout.Height(25)))
                config.Test();
            EditorGUILayout.EndHorizontal();
        }
    }
}