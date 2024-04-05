using System;
using PhEngine.Core.JSON;
using PhEngine.Core.Operation;
using UnityEngine;
using UnityEngine.Serialization;

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
        public RequestHeader[] headers;
        [TextArea(0,100)]
        public string response;
        
        public bool isUseTestRequestBody;
        public bool isUseTestHeaders;
        public bool isUseMockedResponse;

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
                op.SetMockedResponse(new JSONObject(response));
            }

            op.Run();
        }
    }
}