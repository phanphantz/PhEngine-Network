using System;
using PhEngine.JSON;

namespace PhEngine.Network
{
    [Serializable]
    public class ClientRequest
    {
        public JSONObject Content { get; }
        public WebRequestForm Form { get; }
        
        public string Destination => Form.path;
        public HTTPVerb Verb => Form.verb;
        public ParameterType ParameterType => Form.parameterType;
        public bool IsShowLoading => Form.setting.isShowLoading;
        public bool IsShowConnectionFailError => Form.setting.isShowErrorOnConnectionFail;
        public bool IsShowServerFailError => Form.setting.isShowErrorOnServerFail;
        public NetworkDebugMode DebugMode => Form.setting.debugMode;
        public WebRequestPathType Type => Form.type;

        internal ClientRequest(WebRequestForm form, JSONObject json = null)
        {
            Form = form;
            Content = json;
        }

        internal void SetDebugMode(NetworkDebugMode mode)
        {
            Form.setting.debugMode = mode;
        }
    }
}