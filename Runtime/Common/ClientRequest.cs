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
        public APIForm Form { get; }
        public List<RequestHeader> HeaderList { get; private set; } = new List<RequestHeader>();
        public CustomSchema DataFieldCustomSchema => Form.customResponseSchema;
        
        public string Destination => Form.path;
        public HTTPVerb Verb => Form.verb;
        public ParameterType ParameterType => Form.parameterType;
        public bool IsShowLoading => Form.setting.isShowLoading;
        public bool IsShowConnectionFailError => Form.setting.isShowErrorOnConnectionFail;
        public bool IsShowServerFailError => Form.setting.isShowErrorOnServerFail;
        public bool IsShowErrorOnClientFail => Form.setting.isShowErrorOnClientFail;
        public FailureHandling FailureHandling => Form.setting.failureHandling;
        public MockMode MockMode => Form.setting.mockMode;
        public APIPathType Type => Form.type;

        internal ClientRequest(APIForm form, JSONObject json = null)
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

        internal void SetMockMode(MockMode mode)
        {
            Form.setting.mockMode = mode;
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

        internal void OverrideDataFieldSchema(string schema)
        {
            Form.customResponseSchema = new CustomSchema(schema, JsonSchemaModification.Override);
        }
        
        internal void AppendDataFieldSchema(string schema)
        {
            Form.customResponseSchema = new CustomSchema(schema, JsonSchemaModification.Append);
        }

        public void	AddRequestField(string fieldName, int value)
        {
            Content ??= new JSONObject();
            Content.AddField(fieldName, value);
        }
        
        public void	AddRequestField(string fieldName, string value)
        {
            Content ??= new JSONObject();
            Content.AddField(fieldName, value);
        }
        
        public void	AddRequestField(string fieldName, long value)
        {
            Content ??= new JSONObject();
            Content.AddField(fieldName, value);
        }
        
        public void	AddRequestField(string fieldName, bool value)
        {
            Content ??= new JSONObject();
            Content.AddField(fieldName, value);
        }
        
        public void	AddRequestField(string fieldName, float value)
        {
            Content ??= new JSONObject();
            Content.AddField(fieldName, value);
        }
        
        public void	AddRequestField(string fieldName, JSONObject value)
        {
            Content ??= new JSONObject();
            Content.AddField(fieldName, value);
        }
    }
    
    public enum MockMode
    {
        Off, MockServerReturnSuccess, MockServerReturnFail, MockConnectionFail 
    }

    [Serializable]
    public class CustomSchema
    {
        public string value;
        public JsonSchemaModification mode;

        public CustomSchema()
        {
        }
        
        public CustomSchema(string value, JsonSchemaModification mode)
        {
            this.value = value;
            this.mode = mode;
        }

        public CustomSchema(CustomSchema customSchema)
        {
            value = customSchema.value;
            mode = customSchema.mode;
        }

        public string GetFinalSchema(string input, string separator)
        {
            switch (mode)
            {
                case JsonSchemaModification.None:
                    return input;
                case JsonSchemaModification.Append:
                    return input + separator + value;
                case JsonSchemaModification.Override:
                    return value;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public enum JsonSchemaModification
    {
        None, Override, Append
    }
}