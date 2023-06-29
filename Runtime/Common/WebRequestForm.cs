using System;
using UnityEngine;

namespace PhEngine.Network
{
    [Serializable]
    public struct WebRequestForm
    {
        public string name;
        public string path;
        public HTTPVerb verb;
        public ParameterType parameterType;
        public WebRequestPathType type;
        public WebRequestSetting setting;

        [TextArea(0,100)]
        public string requestBodyTemplate;

        public WebRequestForm(string path, HTTPVerb verb, ParameterType parameterType = ParameterType.None, WebRequestPathType type = WebRequestPathType.FullURL, WebRequestSetting setting = null, string requestBodyTemplate = null)
        {
            name = string.Empty;
            this.path = path;
            this.verb = verb;
            this.parameterType = parameterType;
            this.type = type;
            this.setting = setting ?? new WebRequestSetting();
            this.requestBodyTemplate = requestBodyTemplate;
        }
    }
    
    public enum HTTPVerb
    {
        GET, HEAD, POST, PUT, CREATE, PATCH, DELETE
    }
    
    public enum ParameterType
    {
        None, Query, Body, Path
    }

    public enum WebRequestPathType
    {
        FullURL, Endpoint
    }

    [Serializable]
    public class WebRequestSetting
    {
        public bool isShowLoading = true;
        public bool isShowErrorOnConnectionFail = true;
        public bool isShowErrorOnServerFail = true;
        public NetworkDebugMode debugMode;
        public FailureHandling failureHandling;
    }
    
    public enum FailureHandling
    {
        None, SuggestReloadGame, SuggestRetry
    }
}