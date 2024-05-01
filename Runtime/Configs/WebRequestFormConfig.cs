using System;
using PhEngine.Core.JSON;
using PhEngine.Core.Operation;
using UnityEngine;

namespace PhEngine.Network
{
    [CreateAssetMenu(menuName = "PhEngine/Network/WebRequestFormConfig", fileName = "WebRequestFormConfig", order = 0)]
    public class WebRequestFormConfig : ScriptableObject
    {
        [SerializeField] WebRequestForm form;
        public WebRequestForm Form => new WebRequestForm(form);
        
        [Header("Testing")]
        [TextArea(0,100)]
        public string requestBody;
        public RequestHeader[] headers = new RequestHeader[]{};
        [TextArea(0,100)]
        public string response;
        
        public bool isUseTestRequestBody = true;
        public bool isUseTestHeaders = true;
        public bool isUseMockedResponse;
        public bool isFullJsonResponse;

        [ContextMenu(nameof(Test))]
        public void Test()
        {
            var factory = FindObjectOfType<APICaller>();
            if (factory == null)
                throw new NullReferenceException("API Caller is missing.");

            var op = form.parameterType == ParameterType.Path ?
                new APIOperation(form, isUseTestRequestBody ? requestBody : null) :
                new APIOperation(form, isUseTestRequestBody ? new JSONObject(requestBody) : null);

            if (isUseTestHeaders)
            {
                foreach (var header in headers)
                    op.AddHeader(header);
            }

            if (isUseMockedResponse)
            {
                if (isFullJsonResponse)
                    op.SetMockedFullJson(new JSONObject(response));
                else
                    op.SetMockedResponseData(new JSONObject(response));
            }

            op.Run();
        }
        
        public APIOperation CreateCall() => new APIOperation(this);
        public APIOperation CreateCall(JSONObject json) => new APIOperation(this, json);
        public APIOperation CreateCall(object obj) => new APIOperation(this, obj);
    }
}