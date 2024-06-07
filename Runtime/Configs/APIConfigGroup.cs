using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

using UnityEngine;

namespace PhEngine.Network
{
    [CreateAssetMenu(menuName = "PhEngine/Network/APIConfigGroup", fileName = "APIConfigGroup", order = 0)]
    public class APIConfigGroup : ScriptableObject
    {
        public string defaultName = "Sample";
        public string defaultPath = "sample";
        public APIPathType defaultPathType;

        public APIConfig[] APIs => APIList.ToArray();
        [SerializeField] List<APIConfig> APIList = new List<APIConfig>();

        public APIConfig GetByName(string configName)
        {
            return APIList.FirstOrDefault(t => t.name == configName);
        }

#if UNITY_EDITOR
        public void AppendNewAPI(string configName, string path, HTTPVerb verb, ParameterType parameterType, APIPathType pathType)
        {
            var location = Path.GetDirectoryName(AssetDatabase.GetAssetPath(this));
            var finalPath = Path.Combine(location, configName + ".asset");
            if (File.Exists(finalPath))
                throw new Exception("API with the same name is already exists");

            var newConfig = APIConfig.CreateInEditor(finalPath);
            var form = new APIForm(path, verb, parameterType, pathType);
            newConfig.AssignForm(form);
            EditorUtility.SetDirty(newConfig);
            
            Undo.RecordObject(this, "Modify APIs");
            APIList.Add(newConfig);
            EditorUtility.SetDirty(this);
        }
#endif
    }
}