using UnityEditor;
using UnityEngine;

namespace PhEngine.Network.Editor
{
    [CustomEditor(typeof(APICallConfig))]
    public class APICallConfigEditor : UnityEditor.Editor
    {
        APICallConfig config;
        
        void OnEnable()
        {
            config = target as APICallConfig;
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