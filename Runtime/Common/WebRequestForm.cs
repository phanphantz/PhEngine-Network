using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace PhEngine.Network
{
    [Serializable]
    public struct WebRequestForm
    {
        public string path;
        public HTTPVerb verb;
        public ParameterType parameterType;
        public WebRequestPathType type;
        public WebRequestSetting setting;

        public WebRequestForm(string path, HTTPVerb verb, ParameterType parameterType = ParameterType.None, WebRequestPathType type = WebRequestPathType.FullURL, WebRequestSetting setting = null)
        {
            this.path = path;
            this.verb = verb;
            this.parameterType = parameterType;
            this.type = type;
            this.setting = setting ?? new WebRequestSetting();
        }

        public WebRequestForm(WebRequestForm form)
        {
            path = form.path;
            verb = form.verb;
            parameterType = form.parameterType;
            type = form.type;
            setting = new WebRequestSetting(form.setting);
        }
    }
    
    public enum HTTPVerb
    {
        GET, HEAD, POST, PUT, PATCH, DELETE
    }
    
    public enum ParameterType
    {
        None, Query, Body, Path
    }

    public enum WebRequestPathType
    {
        Endpoint, FullURL
    }

    [Serializable]
    public class WebRequestSetting
    {
        public bool isShowLoading = true;
        public bool isShowErrorOnConnectionFail = true;
        public bool isShowErrorOnServerFail = true;
        public bool isShowErrorOnClientFail = true;
        public TestMode testMode;
        public FailureHandling failureHandling;
        
        public WebRequestSetting()
        {
        }
        
        public WebRequestSetting(WebRequestSetting setting)
        {
            isShowLoading = setting.isShowLoading;
            isShowErrorOnConnectionFail = setting.isShowErrorOnConnectionFail;
            isShowErrorOnServerFail = setting.isShowErrorOnServerFail;
            isShowErrorOnClientFail = setting.isShowErrorOnClientFail;
            testMode = setting.testMode;
            failureHandling = setting.failureHandling;
        }
    }
    
    public enum FailureHandling
    {
        None, SuggestReloadGame, SuggestRetry
    }
}