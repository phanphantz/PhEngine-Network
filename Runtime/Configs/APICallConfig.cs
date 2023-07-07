using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PhEngine.Network
{
    [CreateAssetMenu(menuName = "PhEngine/Network/APICallConfig" , fileName = "APICallConfig")]
    public class APICallConfig : ScriptableObject
    {
        public List<BackendSetting> backendSettingList = new List<BackendSetting>();
        public int timeoutInSeconds = 10;

        public int SelectedBackendIndex => selectedBackendIndex;
        [HideInInspector] [SerializeField] int selectedBackendIndex;

        public BackendSetting GetCurrentEnvironment()
        {
            if (selectedBackendIndex < 0 || selectedBackendIndex >= backendSettingList.Count)
                return null;

            return backendSettingList[selectedBackendIndex];
        }

        [Header("Debugging")] public bool isForceUseTestMode;
        public TestMode testMode;
        public APILogOption logOption = APILogOption.Pretty;

        [Header("Format Settings")] public ClientRequestRule clientRequestRule;
        public ServerResultRule serverResultRule;

        public void SetBackendEnvironment(string environmentName)
        {
            var matchedSetting = backendSettingList.FirstOrDefault(env => env.name == environmentName);
            if (matchedSetting == null)
                throw new Exception($"There is no assigned BackendSetting with name: {environmentName}");

            var index = backendSettingList.IndexOf(matchedSetting);
            SetBackendEnvironmentByIndex(index);
        }

        public void SetBackendEnvironmentByIndex(int index)
        {
            selectedBackendIndex = Mathf.Clamp(index, 0, backendSettingList.Count - 1);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public string[] GetEnvironmentOptions()
        {
            return backendSettingList
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