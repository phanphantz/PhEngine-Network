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

        public void SetAccessToken(string value) => accessToken = value;
        
        string FinalAccessToken => IsShouldUseEditorAccessToken() ? config.editorAccessToken : accessToken;

        bool IsShouldUseEditorAccessToken()
        {
            return Application.isEditor && config.isUseEditorAccessToken;
        }
        
        public void CallByRequest(WebRequestForm form, JSONObject json = null)
        {
            var call = Create(form, json);
            Call(call);
        }
        
        public APIOperation Create(WebRequestForm form, JSONObject json = null)
        {
            //TODO: over-engineer?
            var creator = new APIOperationCreator(FinalAccessToken, config, networkRule, requestHeaderModifications);
            var request = new ClientRequest(form, json);
            var call = creator.Create(request);
            return call;
        }

        public void Call(APIOperation operation)
        {
            operation.RunOn(this);
        }

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
    }
}