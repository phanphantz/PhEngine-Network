using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PhEngine.Network
{
    [CreateAssetMenu(menuName = "PhEngine/Network/APIBible", fileName = "APIBible", order = 0)]
    public class APIBible : ScriptableObject
    {
        public IReadOnlyCollection<WebRequestFormConfig> Endpoints => endpointList.ToArray();
        [SerializeField] List<WebRequestFormConfig> endpointList = new List<WebRequestFormConfig>();
       
        public IReadOnlyCollection<WebRequestFormConfig> Urls => urlList.ToArray();
        [SerializeField] List<WebRequestFormConfig> urlList = new List<WebRequestFormConfig>();

        public void SetConfigs(params WebRequestFormConfig[] configs)
        {
            endpointList.Clear();
            urlList.Clear();
            foreach (var config in configs)
            {
                if (config.Form.type == WebRequestPathType.Endpoint)
                    endpointList.Add(config);
                else if (config.Form.type == WebRequestPathType.FullURL)
                    urlList.Add(config);
            }
            
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
        
        public WebRequestFormConfig GetEndpointByName(string configName)
        {
            return endpointList.FirstOrDefault(t => t.name == configName);
        }
        
        public WebRequestFormConfig GetURLByName(string configName)
        {
            return urlList.FirstOrDefault(t => t.name == configName);
        }
    }
}