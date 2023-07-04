using UnityEngine;

namespace PhEngine.Network
{
    [CreateAssetMenu(menuName = "PhEngine/Network/BackendConfig", fileName = "BackendConfig", order = 0)]
    public class BackendConfig : ScriptableObject
    {
        public string baseUrl;
#if UNITY_EDITOR
        [Header("Editor Only")]
        public bool isUseEditorAccessToken = true;
        public string editorAccessToken;
#endif
    }
}