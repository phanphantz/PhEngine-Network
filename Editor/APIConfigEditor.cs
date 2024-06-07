using UnityEditor;
using UnityEngine;

namespace PhEngine.Network.Editor
{
    [CustomEditor(typeof(APIConfig)), CanEditMultipleObjects]
    public class APIConfigEditor : UnityEditor.Editor
    {
        APIConfig config;
        
        void OnEnable()
        {
            config = target as APIConfig;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Try", GUILayout.Width(50), GUILayout.Height(25)))
                config.Try();
            EditorGUILayout.EndHorizontal();
        }
    }
}