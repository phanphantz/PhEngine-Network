using System;
using PhEngine.Core.JSON;

namespace PhEngine.Network
{
    [Serializable]
    public class APIForm
    {
        public string path;
        public HTTPVerb verb;
        public ParameterType parameterType;
        public APIPathType type;
        public APISetting setting;
        public CustomSchema customResponseSchema;

        public APIForm(string path, HTTPVerb verb, ParameterType parameterType = ParameterType.None,
            APIPathType type = APIPathType.FullURL, APISetting setting = null,
            CustomSchema customCustomSchema = null)
        {
            this.path = path;
            this.verb = verb;
            this.parameterType = parameterType;
            this.type = type;
            this.setting = setting != null ? new APISetting(setting) : new APISetting();
            this.customResponseSchema = customCustomSchema != null ? new CustomSchema(customCustomSchema) : new CustomSchema();
        }

        public APIForm(APIForm form)
        {
            path = form.path;
            verb = form.verb;
            parameterType = form.parameterType;
            type = form.type;
            setting = new APISetting(form.setting);
            customResponseSchema = new CustomSchema(form.customResponseSchema);
        }

        public APIOperation CreateCall() => new APIOperation(this);
        public APIOperation CreateCall(JSONObject json) => new APIOperation(this, json);
        public APIOperation CreateCall(object obj) => new APIOperation(this, obj);
    }
    
    public enum HTTPVerb
    {
        GET, HEAD, POST, PUT, PATCH, DELETE
    }
    
    public enum ParameterType
    {
        None, Query, Body, Path
    }

    public enum APIPathType
    {
        Endpoint, FullURL
    }

    [Serializable]
    public class APISetting
    {
        public bool isShowLoading = true;
        public bool isShowErrorOnConnectionFail = true;
        public bool isShowErrorOnServerFail = true;
        public bool isShowErrorOnClientFail = true;
        public MockMode mockMode;
        public FailureHandling failureHandling;
        
        public APISetting()
        {
        }
        
        public APISetting(APISetting setting)
        {
            isShowLoading = setting.isShowLoading;
            isShowErrorOnConnectionFail = setting.isShowErrorOnConnectionFail;
            isShowErrorOnServerFail = setting.isShowErrorOnServerFail;
            isShowErrorOnClientFail = setting.isShowErrorOnClientFail;
            mockMode = setting.mockMode;
            failureHandling = setting.failureHandling;
        }
    }
    
    public enum FailureHandling
    {
        None, SuggestReloadGame, SuggestRetry
    }
}