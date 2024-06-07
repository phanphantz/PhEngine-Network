using System;
using PhEngine.Core.JSON;
using PhEngine.Core.Operation;
using UnityEngine;

namespace PhEngine.Network
{
    [Serializable]
    public class APITester
    {
        public MockMode mockMode;

        [TextArea(0,100)]
        public string requestBody;
        public RequestHeader[] headers = new RequestHeader[]{};

        public MockResponseMode mockResponseMode;
        
        [TextArea(0,100)]
        public string response;

        public void TestOn(APIOperation op)
        {
            if (op.ClientRequest.Form.parameterType == ParameterType.Path)
            {
                op.SetRequestBody(requestBody);
            }
            else
            {
                op.SetRequestBody(new JSONObject(requestBody));
            }

            op.SetMockMode(mockMode);
            
            foreach (var header in headers)
                op.AddHeader(header);

            if (mockResponseMode != MockResponseMode.Off)
            {
                switch (mockResponseMode)
                {
                    case MockResponseMode.MockFullJson:
                        op.SetMockedFullJson(new JSONObject(response));
                        break;
                    case MockResponseMode.MockDataJson:
                        op.SetMockedResponseData(new JSONObject(response));
                        break;
                }
            }

            op.Run();
        }
    }
}