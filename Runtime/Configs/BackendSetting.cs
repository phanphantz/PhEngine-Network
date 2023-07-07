using System;
using UnityEngine;

namespace PhEngine.Network
{
    [Serializable]
    public class BackendSetting
    {
        public string name;
        public string baseUrl;
        
#if UNITY_EDITOR
        [Header("Editor Only")]
        public bool isUseEditorAccessToken = true;
        public string editorAccessToken;
#endif
    }
}