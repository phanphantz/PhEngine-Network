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

        [TextArea(0,100)]
        public string requestBodyTemplate;
        public RequestHeader[] headerTemplates;

        public WebRequestForm(string path, HTTPVerb verb, ParameterType parameterType = ParameterType.None, WebRequestPathType type = WebRequestPathType.FullURL, WebRequestSetting setting = null, string requestBodyTemplate = null, RequestHeader[] headers = null)
        {
            this.path = path;
            this.verb = verb;
            this.parameterType = parameterType;
            this.type = type;
            this.setting = setting ?? new WebRequestSetting();
            this.requestBodyTemplate = requestBodyTemplate;
            headerTemplates = headers?.Select(h => new RequestHeader(h)).ToArray();
        }

        public WebRequestForm(WebRequestForm form)
        {
            path = form.path;
            verb = form.verb;
            parameterType = form.parameterType;
            type = form.type;
            setting = new WebRequestSetting(form.setting);
            requestBodyTemplate = form.requestBodyTemplate;
            headerTemplates = form.headerTemplates?.Select(h => new RequestHeader(h)).ToArray();
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