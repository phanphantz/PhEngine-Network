using UnityEngine;
using System;
using System.Globalization;
using PhEngine.Core;
using PhEngine.Core.JSON;

namespace PhEngine.Network
{
    public class APICaller : Singleton<APICaller>
    {
        [SerializeField] string accessToken;
        [SerializeField] string refreshToken;
        [SerializeField] RequestHeaderSetting[] requestHeaderModifications;
        
        [Header("Info")]
        [SerializeField] string latestServerTimeString;
        public DateTime LatestServerTime { get; private set; }
        public DateTime AccessTokenExpireTime { get; private set; }
        public DateTime RefreshTokenExpireTime { get; private set; }

        [Header("Configs")] 
        [SerializeField] APICallConfig config;
        [SerializeField] NetworkRuleConfig networkRule;

        #region Initialization
        
        protected override void InitAfterAwake()
        {
            NetworkEvent.OnReceiveServerTime += SetLatestServerTime;
        }

        void OnDestroy()
        {
            NetworkEvent.OnReceiveServerTime -= SetLatestServerTime;
        }

        public void SetLatestServerTime(DateTime dateTime)
        {
            LatestServerTime = dateTime;
            latestServerTimeString = dateTime.ToString(CultureInfo.InvariantCulture);
        }
        
        #endregion

        public WebRequestBuilder GetBuilder()
        {
            if (config == null || networkRule == null)
            {
                Debug.LogError("Cannot Prepare API Operation. APICallerConfig or NetworkRuleConfig is missing.");
                return null;
            }
            
            var finalAccessToken = GetFinalAccessToken();
            return new WebRequestBuilder(config, networkRule, requestHeaderModifications, finalAccessToken);
            string GetFinalAccessToken()
            {
                return Application.isEditor && config.isUseEditorAccessToken ? config.editorAccessToken : accessToken;
            }
        }
        
        public void SetAccessToken(string value) => accessToken = value;
        public void SetRefreshToken(string value) => refreshToken = value;
        public void SetAccessTokenExpireTime(DateTime dateTime) => AccessTokenExpireTime = dateTime;
        public void SetRefreshTokenExpireTime(DateTime dateTime) => RefreshTokenExpireTime = dateTime;
        public bool IsAccessTokenExpired(DateTime currentTime) => currentTime >= AccessTokenExpireTime;
        public bool IsRefreshTokenExpired(DateTime currentTime) => currentTime >= RefreshTokenExpireTime;
    }
}