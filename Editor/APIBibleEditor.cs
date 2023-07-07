using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PhEngine.Network.Editor
{
    [CustomEditor(typeof(APIBible))]
    public class APIBibleEditor : UnityEditor.Editor
    {
        APIBible config;
        
        void OnEnable()
        {
            config = target as APIBible;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Refresh"))
            {
                var webRequestConfigs = FindAllScriptableObjects<WebRequestFormConfig>();
                var sortedConfigs = webRequestConfigs.OrderBy(c => c.name).ToArray();
                Undo.RegisterCompleteObjectUndo(config, "Refresh API Bible");
                config.SetConfigs(sortedConfigs);
            }
        }
        
        public static T[] FindAllScriptableObjects<T>() where T : ScriptableObject
        {
            var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            var results = new T[guids.Length];
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                results[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }
            
            return results;
        }
    }
}