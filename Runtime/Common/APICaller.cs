using UnityEngine;
using System;
using System.Globalization;
using PhEngine.Core;
using PhEngine.JSON;

namespace PhEngine.Network
{
    public class APICaller : Singleton<APICaller>
    {
        [SerializeField] string accessToken;
        [SerializeField] RequestHeaderSetting[] requestHeaderModifications;
        
        [Header("Info")]
        [SerializeField] string latestServerTimeString;
        public DateTime LatestServerTime { get; private set; }

        [Header("Configs")] 
        [SerializeField] APICallConfig config;
        [SerializeField] NetworkRuleConfig networkRule;

        #region Initialization
        
        protected override void InitAfterAwake()
        {
            NetworkEvent.OnTimeChanged += SetLatestServerTime;
        }

        void OnDestroy()
        {
            NetworkEvent.OnTimeChanged -= SetLatestServerTime;
        }

        void SetLatestServerTime(DateTime dateTime)
        {
            LatestServerTime = dateTime;
            latestServerTimeString = dateTime.ToString(CultureInfo.InvariantCulture);
        }
        
        #endregion
        
        public void Call(WebRequestForm form, JSONObject json = null)
        {
            var call = Create(form, json);
            Call(call);
        }
        
        public void Call(APIOperation operation) => operation.RunOn(this);
        
        public APIOperation Create(WebRequestForm form, JSONObject json = null)
        {
            if (config == null || networkRule == null)
            {
                Debug.LogError("Cannot Create API Operation. APICallerConfig or NetworkRuleConfig is missing.");
                return null;
            }
            
            var clientRequest = new ClientRequest(form, json);
            if (config.isForceUseNetworkDebugMode)
                clientRequest.SetDebugMode(config.networkDebugMode);
            
            var finalAccessToken = GetFinalAccessToken();
            var webRequest = WebRequestFactory.Create(clientRequest, config.url, config.timeoutInSeconds, networkRule.clientRequestRule, requestHeaderModifications, finalAccessToken);
            return new APIOperation(clientRequest, webRequest, networkRule.serverResultRule, config.isShowingLog);
            
            string GetFinalAccessToken()
            {
                return Application.isEditor && config.isUseEditorAccessToken ? config.editorAccessToken : accessToken;
            }
        }
        
        public void SetAccessToken(string value) => accessToken = value;
    }
}