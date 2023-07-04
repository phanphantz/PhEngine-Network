using UnityEngine;
using System;
using System.Globalization;
using PhEngine.Core;
using UnityEngine.Networking;

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
        public APICallConfig Config => config;

        public AccessTokenValidator AccessTokenValidator => accessTokenValidator;
        [SerializeField] AccessTokenValidator accessTokenValidator;

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
        
        internal UnityWebRequest CreateWebRequest(APIOperation operation)
        {
            if (config.isForceUseNetworkDebugMode)
                operation.SetDebugMode(config.networkDebugMode);

            operation.SetLogOption(config.logOption);
            operation.SetServerResultRule(config.serverResultRule);
            
            var finalAccessToken = GetFinalAccessToken();
            return WebRequestFactory.Create(config, requestHeaderModifications, finalAccessToken, operation.ClientRequest);
            
            string GetFinalAccessToken()
            {
#if UNITY_EDITOR
                if (Application.isEditor && config.backend && config.backend.isUseEditorAccessToken)
                    return config.backend.editorAccessToken;
#endif
                return accessToken;
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