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
        public bool isForceUseNetworkDebugModeFromThisConfig;
        public NetworkDebugMode networkDebugMode;
        
        [Header("Editor Only")]
        public bool isShowingLog = true;
        public bool isUseEditorAccessToken = true;
        public string editorAccessToken;
    }
}