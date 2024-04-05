using UnityEditor;
using UnityEngine;

namespace PhEngine.Network.Editor
{
    [CustomEditor(typeof(APICallerConfig))]
    public class APICallConfigEditor : UnityEditor.Editor
    {
        APICallerConfig config;
        
        void OnEnable()
        {
            config = target as APICallerConfig;
            serializedObject.Update();
        }

        public override void OnInspectorGUI()
        {
            EditorStyles.label.fontStyle = FontStyle.Bold;
            EditorGUILayout.LabelField("Connection");
            EditorStyles.label.fontStyle = FontStyle.Normal;
            
            EditorGUI.BeginChangeCheck();
            var selectedIndex = EditorGUILayout.Popup("Environment", config.SelectedBackendIndex, config.GetEnvironmentOptions());
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(config, "Change Environment");
                config.SetBackendEnvironmentByIndex(selectedIndex);
            }
            base.OnInspectorGUI();
        }
    }
}