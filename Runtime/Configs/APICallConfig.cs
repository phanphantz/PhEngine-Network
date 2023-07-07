using System;
using System.Linq;
using UnityEngine;

namespace PhEngine.Network
{
    [CreateAssetMenu(menuName = "PhEngine/Network/APICallConfig" , fileName = "APICallConfig")]
    public class APICallConfig : ScriptableObject
    {
        public BackendSetting[] backendSettings;
        public int timeoutInSeconds = 10;

        public int SelectedBackendIndex => selectedBackendIndex;
        [HideInInspector][SerializeField] int selectedBackendIndex;

        public BackendSetting GetCurrentEnvironment()
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

        public void SetBackendEnvironment(string environmentName)
        {
            var matchedSetting = backendSettings.FirstOrDefault(env => env.name == environmentName);
            if (matchedSetting == null)
                throw new Exception($"There is no assigned BackendSetting with name: {environmentName}");

            var index = Array.IndexOf(backendSettings, matchedSetting);
            SetBackendEnvironmentByIndex(index);
        }

        public void SetBackendEnvironmentByIndex(int index)
        {
            selectedBackendIndex = Mathf.Clamp(index, 0, backendSettings.Length -1);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public string[] GetEnvironmentOptions()
        {
            if (backendSettings == null)
                return new string[] {};

            return backendSettings
                .Select(setting => setting.name)
                .Where(value => !string.IsNullOrEmpty(value))
                .ToArray();
        }
    }

    public enum APILogOption
    {
        Pretty = 2, Verbose = 1, Minimal = 0, None = -1
    }
}