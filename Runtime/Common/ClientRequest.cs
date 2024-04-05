using System;
using System.Collections.Generic;
using System.Linq;
using PhEngine.Core.JSON;

namespace PhEngine.Network
{
    [Serializable]
    public class ClientRequest
    {
        public JSONObject Content { get; private set; }
        public string MockedResponseData { get; private set; }
        public string MockedFullJson { get; private set; }
        public WebRequestForm Form { get; }
        public List<RequestHeader> HeaderList { get; private set; } = new List<RequestHeader>();
        
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

        internal void SetMockedResponseData(string value)
        {
            MockedResponseData = value;
            MockedFullJson = null;
        }
        
        internal void SetMockedFullJson(string value)
        {
            MockedFullJson = value;
            MockedResponseData = null;
        }

        internal void SetDebugMode(TestMode mode)
        {
            Form.setting.testMode = mode;
        }

        internal void SetContent(JSONObject json)
        {
            Content = json;
        }
        
        internal void AddHeader(RequestHeader header)
        {
            var existingHeader = HeaderList.FirstOrDefault(h => h.key == header.key);
            if (existingHeader != null)
            {
                existingHeader.value = header.value;
            }
            else
            {
                HeaderList.Add(new RequestHeader(header));
            }
        }

        internal void SetHeaders(RequestHeader[] headers)
        {
            HeaderList = new List<RequestHeader>(headers);
        }
    }
    
    public enum TestMode
    {
        Off, MockServerReturnSuccess, MockServerReturnFail, MockConnectionFail 
    }
}