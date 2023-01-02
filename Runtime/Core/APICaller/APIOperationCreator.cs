using System;
using System.Collections.Generic;
using PhEngine.JSON;
using UnityEngine;
using UnityEngine.Networking;

namespace PhEngine.Network
{
    public class APIOperationCreator
    {
        APICallConfig Config { get; }
        NetworkRuleConfig NetworkRuleConfig { get; }
        string AccessToken { get; }
        
        public APIOperationCreator(string accessToken, APICallConfig config, NetworkRuleConfig networkRuleConfig)
        {
            AccessToken = accessToken;
            Config = config;
            NetworkRuleConfig = networkRuleConfig;
        }

        public APIOperation Create(ClientRequest clientRequest)
        {
            if (!ValidateConfigs())
                return null;

            TryForceDebugModeOnClientRequestIfNeeded(clientRequest);
            var webRequest = CreateUnityWebRequest(clientRequest);
            var apiCall = new APIOperation(clientRequest, webRequest, NetworkRuleConfig.serverResultRule, Config.isShowingLog);
            return apiCall;
        }

        bool ValidateConfigs()
        {
            var isValid = Config && NetworkRuleConfig;
            if (isValid)
                return true;
            
            Debug.LogError("Cannot Create API Operation. APICallerConfig or NetworkRuleConfig is missing.");
            return false;
        }

        void TryForceDebugModeOnClientRequestIfNeeded(ClientRequest clientRequest)
        {
            if (Config.isForceUseNetworkDebugModeFromThisConfig)
                clientRequest.SetDebugMode(Config.networkDebugMode);
        }

        UnityWebRequest CreateUnityWebRequest(ClientRequest clientRequest)
        {
            return UnityWebRequestCreator.CreateUnityWebRequest(clientRequest, Config.url, Config.timeoutInSeconds, NetworkRuleConfig.clientRequestRule, AccessToken);
        }
    }
}