using UnityEngine;

namespace PhEngine.Network
{
    [CreateAssetMenu(menuName = "PhEngine/Network/APICallConfig" , fileName = "APICallConfig")]
    public class APICallConfig : ScriptableObject
    {
        [Header("Connection")]
        public string url;
        public int timeoutInSeconds = 10;
       
        [Header("Debugging")]
        public bool isForceUseNetworkDebugMode;
        public NetworkDebugMode networkDebugMode;
        public APILogOption logOption = APILogOption.Pretty;
        
#if UNITY_EDITOR
        [Header("Editor Only")]
        public bool isUseEditorAccessToken = true;
        public string editorAccessToken;
#endif
        
        [Header("Format Settings")]
        public ClientRequestRule clientRequestRule;
        public ServerResultRule serverResultRule;
    }

    public enum APILogOption
    {
        Pretty = 2, Verbose = 1, Minimal = 0, None = -1
    }
}