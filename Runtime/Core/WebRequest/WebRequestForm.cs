using System;

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

        public WebRequestForm(string path, HTTPVerb verb, ParameterType parameterType, WebRequestPathType type = WebRequestPathType.FullURL, WebRequestSetting setting = null)
        {
            name = string.Empty;
            this.path = path;
            this.verb = verb;
            this.parameterType = parameterType;
            this.type = type;
            this.setting = setting ?? new WebRequestSetting();
        }
        
        public WebRequestForm(string name, string path, HTTPVerb verb, ParameterType parameterType, WebRequestPathType type = WebRequestPathType.FullURL, WebRequestSetting setting = null)
        {
            this.name = name;
            this.path = path;
            this.verb = verb;
            this.parameterType = parameterType;
            this.type = type;
            this.setting = setting ?? new WebRequestSetting();
        }
    }
}