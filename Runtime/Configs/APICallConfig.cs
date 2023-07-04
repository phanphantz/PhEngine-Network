using UnityEngine;

namespace PhEngine.Network
{
    [CreateAssetMenu(menuName = "PhEngine/Network/APICallConfig" , fileName = "APICallConfig")]
    public class APICallConfig : ScriptableObject
    {
        [Header("Connection")] 
        public BackendConfig backend;
        public int timeoutInSeconds = 10;
       
        [Header("Debugging")]
        public bool isForceUseNetworkDebugMode;
        public NetworkDebugMode networkDebugMode;
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