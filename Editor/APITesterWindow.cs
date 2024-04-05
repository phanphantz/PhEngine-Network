using UnityEditor;
using UnityEngine;

namespace PhEngine.Network.Editor
{
    public class APITesterWindow : EditorWindow
    {
        static WebRequestFormConfig formConfig;
        static string requestBody;
        
        [MenuItem("PhEngine/Network/APITester")]
        static void Init()
        {
            var window = GetWindow(typeof(APITesterWindow), false, "API Tester");
            window.Show();
        }

        void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            formConfig = EditorGUILayout.ObjectField(new GUIContent("From Config"), formConfig, typeof(WebRequestFormConfig), false) as WebRequestFormConfig;
            if (EditorGUI.EndChangeCheck() && formConfig)
            {
                requestBody = formConfig.requestBody;
            }
            
            EditorGUILayout.LabelField("Request Body:");
            requestBody = EditorGUILayout.TextArea(requestBody, GUILayout.Height(200));

            EditorGUI.BeginDisabledGroup(formConfig == null);
            if (GUILayout.Button("Call"))
            {
                Call();
            }
            EditorGUI.EndDisabledGroup();
        }

        static void Call()
        {
            if (formConfig)
                formConfig.Test();
        }
    }
}