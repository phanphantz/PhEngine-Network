using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PhEngine.Core;
using UnityEngine.Networking;

namespace PhEngine.Network
{
    public class APICaller : Singleton<APICaller>
    {
        [SerializeField] string accessToken;
        [SerializeField] string refreshToken;
        [SerializeField] List<RequestHeader> requestHeaderModifications = new List<RequestHeader>();
        
        [Header("Info")]
        [SerializeField] string latestServerTimeString;
        public DateTime LatestServerTime { get; private set; }
        public DateTime AccessTokenExpireTime { get; private set; }
        public DateTime RefreshTokenExpireTime { get; private set; }

        [Header("Configs")] 
        [SerializeField] APICallerConfig config;
        public APICallerConfig Config => config;

        public AccessTokenValidator AccessTokenValidator => accessTokenValidator;
        [SerializeField] AccessTokenValidator accessTokenValidator;

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

        public void SetBackendEnvironment(string environmentName)
        {
            config.SetBackendEnvironment(environmentName);
        }

        public void SetHeaderModification(string key, string value)
        {
            var existingHeader = GetHeaderModification(key);
            if (existingHeader != null)
            {
                existingHeader.value = value;
            }
            else
            {
                requestHeaderModifications.Add(new RequestHeader(key, value));
            }
        }

        public RequestHeader GetHeaderModification(string key)
        {
            return requestHeaderModifications.FirstOrDefault(h => h.key == key);
        }

        public void RemoveHeaderModification(string key)
        {
            var existingHeader = GetHeaderModification(key);
            if (existingHeader != null)
            {
                requestHeaderModifications.Remove(existingHeader);
            }
        }

        public void ClearHeaderModification()
        {
            requestHeaderModifications.Clear();
        }
        
        internal UnityWebRequest CreateWebRequest(APIOperation operation)
        {
            if (config.isForceUseTestMode)
                operation.SetDebugMode(config.testMode);

            operation.SetLogOption(config.logOption);
            operation.SetServerResultRule(config.serverResultRule);
            
            var backendSetting = config.GetCurrentEnvironment();
            var finalAccessToken = GetFinalAccessToken();
            return WebRequestFactory.Create(config, backendSetting, requestHeaderModifications.ToArray(), finalAccessToken, operation.ClientRequest);
            
            string GetFinalAccessToken()
            {
#if UNITY_EDITOR
                if (Application.isEditor && backendSetting != null && backendSetting.isUseEditorAccessToken)
                    return backendSetting.editorAccessToken;
#endif
                return accessToken;
            }
        }
        
        public void SetAccessToken(string value) => accessToken = value;
        public void SetRefreshToken(string value) => refreshToken = value;
        
        public void SetAccessTokenExpireTime(DateTime dateTime)
        {
            AccessTokenExpireTime = dateTime;
            var expireIn = AccessTokenExpireTime.ToUniversalTime() - DateTime.UtcNow;
            Debug.Log($"Set Access Token Expire Date to be {expireIn} from now ( {AccessTokenExpireTime} )");
        } 
        
        public void SetRefreshTokenExpireTime(DateTime dateTime)
        {
            RefreshTokenExpireTime = dateTime;
            var expireIn = RefreshTokenExpireTime.ToUniversalTime() - DateTime.UtcNow;
            Debug.Log($"Set Refresh Token Expire Date to be {expireIn} from now ( {RefreshTokenExpireTime} )");
        }
        
        public bool HasAccessToken => !string.IsNullOrEmpty(accessToken);
        public bool IsAccessTokenExpired(DateTime currentTime) => HasAccessToken && currentTime >= AccessTokenExpireTime;
        public bool IsRefreshTokenExpired(DateTime currentTime) => !string.IsNullOrEmpty(refreshToken) && currentTime >= RefreshTokenExpireTime;
    }
}