using UnityEngine;

namespace PhEngine.Network
{
    [CreateAssetMenu(menuName = "PhEngine/Network/APICallConfig" , fileName = "APICallConfig")]
    public class APICallConfig : ScriptableObject
    {
        [Header("Connection")] 
        public BackendSetting[] backendSettings;
        public int timeoutInSeconds = 10;
        [HideInInspector][SerializeField] int selectedBackendIndex;

        public BackendSetting GetBackendSetting()
        {
            if (selectedBackendIndex < 0 || selectedBackendIndex >= backendSettings.Length)
                    return null;

            return backendSettings[selectedBackendIndex];
        }
        
        [Header("Debugging")]
        public bool isForceUseTestMode;
        public TestMode testMode;
        public APILogOption logOption = APILogOption.Pretty;
        
        [Header("Format Settings")]
        public ClientRequestRule clientRequestRule;
        public ServerResultRule serverResultRule;
    }

    public enum APILogOption
    {
        Pretty = 2, Verbose = 1, Minimal = 0, None = -1
    }
}