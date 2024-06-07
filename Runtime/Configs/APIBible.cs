using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PhEngine.Network
{
    [CreateAssetMenu(menuName = "PhEngine/Network/APIBible", fileName = "APIBible", order = 0)]
    public class APIBible : ScriptableObject
    {
        public IReadOnlyCollection<APIConfig> Endpoints => endpointList.ToArray();
        [SerializeField] List<APIConfig> endpointList = new List<APIConfig>();
       
        public IReadOnlyCollection<APIConfig> Urls => urlList.ToArray();
        [SerializeField] List<APIConfig> urlList = new List<APIConfig>();

        public void SetConfigs(params APIConfig[] configs)
        {
            endpointList.Clear();
            urlList.Clear();
            foreach (var config in configs)
            {
                if (config.ClonedForm.type == APIPathType.Endpoint)
                    endpointList.Add(config);
                else if (config.ClonedForm.type == APIPathType.FullURL)
                    urlList.Add(config);
            }
            
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
        
        public APIConfig GetEndpointByName(string configName)
        {
            return endpointList.FirstOrDefault(t => t.name == configName);
        }
        
        public APIConfig GetURLByName(string configName)
        {
            return urlList.FirstOrDefault(t => t.name == configName);
        }
    }
}