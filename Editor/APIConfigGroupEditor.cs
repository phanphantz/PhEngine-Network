using PhEngine.Core.Editor;
using UnityEngine;
using UnityEditor;

namespace PhEngine.Network.Editor
{
    [CustomEditor(typeof(APIConfigGroup)), CanEditMultipleObjects]
    public class APIConfigGroupEditor : UnityEditor.Editor
    {
        APIConfigGroup group;
        HTTPVerb verb;
        ParameterType parameterType;
        string apiPath;

        int currentFocusIndex = -1;

        void OnEnable()
        {
            group = target as APIConfigGroup;
            apiPath = group.defaultPath;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var apiListProp = serializedObject.FindProperty("APIList");
            EditorGUIUtils.DrawTitle("APIs (" + apiListProp.arraySize + ")");
            EditorGUIUtils.DrawUILine();

            for (int i = 0; i < apiListProp.arraySize; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                var obj = apiListProp.GetArrayElementAtIndex(i).objectReferenceValue;
                if (i == currentFocusIndex)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(obj.name))
                        currentFocusIndex = -1;
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    EditorGUIUtils.DrawUILine();
                    EditorGUI.indentLevel++;
                    SerializedObject config = new SerializedObject(obj);
                    
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.PropertyField(config.FindProperty("form"));
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.PropertyField(config.FindProperty("tester"));

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Try", GUILayout.Width(50), GUILayout.Height(25)))
                        (obj as APIConfig)?.Try();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(obj.name))
                        currentFocusIndex = i;
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.Space();
            EditorGUIUtils.DrawTitle("Adding New API");
            EditorGUILayout.BeginHorizontal();
            verb = (HTTPVerb)EditorGUILayout.EnumPopup(verb, GUILayout.Width(70));
            EditorGUILayout.LabelField("Path", GUILayout.Width(32));
            apiPath = EditorGUILayout.TextField( apiPath);
            EditorGUILayout.LabelField("Type", GUILayout.Width(32));
            parameterType = (ParameterType)EditorGUILayout.EnumPopup(parameterType, GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add", GUILayout.Width(50), GUILayout.Height(25)))
                group.AppendNewAPI(verb + "_" + group.defaultName, apiPath, verb, parameterType, group.defaultPathType);
            EditorGUILayout.EndHorizontal();
        }
    }
}