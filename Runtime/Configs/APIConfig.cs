using System;
using PhEngine.Core.JSON;
using UnityEngine;

namespace PhEngine.Network
{
    [CreateAssetMenu(menuName = "PhEngine/Network/APIConfig", fileName = "APIConfig", order = 0)]
    public class APIConfig : ScriptableObject
    {
        [SerializeField] APIForm form;
        public APIForm ClonedForm => new APIForm(form);
        
        [SerializeField] APITester tester = new APITester();

        [ContextMenu(nameof(Try))]
        public void Try()
        {
            var factory = FindObjectOfType<APICaller>();
            if (factory == null)
                throw new NullReferenceException("API Caller is missing.");

            tester.TestOn(new APIOperation(this));
        }
        
        public APIOperation CreateCall() => new APIOperation(this);
        public APIOperation CreateCall(JSONObject json) => new APIOperation(this, json);
        public APIOperation CreateCall(object obj) => new APIOperation(this, obj);
        
#if UNITY_EDITOR
        public static APIConfig CreateInEditor(string path)
        {
            var config = CreateInstance<APIConfig>();
            config.name = path;
            UnityEditor.AssetDatabase.CreateAsset(config, path);
            UnityEditor.AssetDatabase.Refresh();
            UnityEditor.AssetDatabase.SaveAssets();
            return config;
        }
#endif

        public void AssignForm(APIForm value)
        {
            form = value;
        }
    }

    public enum MockResponseMode
    {
        Off, MockFullJson, MockDataJson
    }
}