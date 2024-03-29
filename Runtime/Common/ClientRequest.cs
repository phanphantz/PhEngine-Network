﻿using System;
using PhEngine.Core.JSON;

namespace PhEngine.Network
{
    [Serializable]
    public class ClientRequest
    {
        public JSONObject Content { get; private set; }
        public string MockedResponse { get; private set; }
        public WebRequestForm Form { get; }
        
        public string Destination => Form.path;
        public HTTPVerb Verb => Form.verb;
        public ParameterType ParameterType => Form.parameterType;
        public bool IsShowLoading => Form.setting.isShowLoading;
        public bool IsShowConnectionFailError => Form.setting.isShowErrorOnConnectionFail;
        public bool IsShowServerFailError => Form.setting.isShowErrorOnServerFail;
        public bool IsShowErrorOnClientFail => Form.setting.isShowErrorOnClientFail;
        public FailureHandling FailureHandling => Form.setting.failureHandling;
        public TestMode TestMode => Form.setting.testMode;
        public WebRequestPathType Type => Form.type;

        internal ClientRequest(WebRequestForm form, JSONObject json = null)
        {
            Form = form;
            if (form.parameterType != ParameterType.None)
                Content = json;
        }

        internal void SetMockedResponse(string value)
        {
            MockedResponse = value;
        }

        internal void SetDebugMode(TestMode mode)
        {
            Form.setting.testMode = mode;
        }

        internal void SetContent(JSONObject json)
        {
            Content = json;
        }
    }
    
    public enum TestMode
    {
        Off, MockServerReturnSuccess, MockServerReturnFail, MockConnectionFail 
    }
}