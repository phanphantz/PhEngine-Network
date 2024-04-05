using System;
using System.Net;
using System.Linq;
using PhEngine.Core.JSON;
using UnityEngine.Networking;

namespace PhEngine.Network
{
    [Serializable]
    public class ServerResult
    {
        public int code;
        public JSONObject fullJson;
        public JSONObject dataJson;
        public string message;
        public string dateTime;
        public float elapsedTimeSeconds;
        public ServerResultStatus status;
        public HttpStatusCode httpStatus;
        public bool isMocked;
        public FailureHandling failureHandling;

        public ServerResult() {}

        public ServerResult(ServerResultStatus status, string message, int code, FailureHandling failureHandling)
        {
            this.message = message;
            this.code = code;
            this.status = status;
            this.failureHandling = failureHandling;
        }
        
        public ServerResult(UnityWebRequest unityWebRequest, ServerResultRule resultRule, ClientRequest clientRequest, float elapsedTimeSeconds)
        {
            this.elapsedTimeSeconds = elapsedTimeSeconds;
            failureHandling = clientRequest.FailureHandling;
            if (clientRequest.TestMode != TestMode.Off)
            {
                isMocked = true;
                MockStatus(clientRequest.TestMode);
                dataJson = new JSONObject(clientRequest.MockedResponse);
                return;
            }
            
            code = (int)unityWebRequest.responseCode;
            httpStatus = (HttpStatusCode)code;
            if (!IsHasResponse())
            {
                status = GetServerResultStatus();
                return;
            }

            fullJson = new JSONObject(unityWebRequest.downloadHandler.text);
            
            var codeFromJson = resultRule.GetStatusCodeField(fullJson);
            if (codeFromJson != 0)
                code = codeFromJson;
            
            status = GetServerResultStatus();
            dateTime = resultRule.GetCurrentDateTimeField(fullJson);
            message = resultRule.GetMessageField(fullJson);
            
            dataJson = fullJson;
            if (!string.IsNullOrEmpty(resultRule.dataFieldSchema))
            {
                var tryGetDataJsonFromField = resultRule.GetTargetFieldFromSchema(fullJson, resultRule.dataFieldSchema, out _);
                if (tryGetDataJsonFromField != null)
                    dataJson = tryGetDataJsonFromField;
            }
            
            bool IsHasResponse()
            {
                if (unityWebRequest.downloadHandler == null)
                    return false;

                return !string.IsNullOrEmpty(unityWebRequest.downloadHandler.text);
            }

            ServerResultStatus GetServerResultStatus()
            {
                if (unityWebRequest.error != null)
                    return ServerResultStatus.ConnectionFail;
                
                var successCodeRanges = resultRule.successStatusCodeRanges;
                if (successCodeRanges == null || successCodeRanges.Length == 0)
                    return ServerResultStatus.ServerReturnSuccess;

                if (StatusCodeRange.IsBetweenRange(code, successCodeRanges))
                    return ServerResultStatus.ServerReturnSuccess;
            
                return ServerResultStatus.ServerReturnFail;
            }
        }

        void MockStatus(TestMode testMode)
        {
            status = testMode switch
            {
                TestMode.MockConnectionFail => ServerResultStatus.ConnectionFail,
                TestMode.MockServerReturnFail => ServerResultStatus.ServerReturnFail,
                TestMode.MockServerReturnSuccess => ServerResultStatus.ServerReturnSuccess,
                TestMode.Off => ServerResultStatus.ServerReturnSuccess,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static ServerResult CreateClientFailResult(string message, FailureHandling failureHandling,int code = 0)
        {
            return new ServerResult(ServerResultStatus.ClientFail, message, code, failureHandling);
        }
    }
    
    public enum ServerResultStatus
    {
        ServerReturnSuccess, ServerReturnFail, ConnectionFail, ClientFail
    }
    
    [Serializable]
    public class ServerResultRule
    {
        public StatusCodeRange[] successStatusCodeRanges;
        public char schemaNavigatorChar = '/';
        public string messageFieldSchema = "message";
        public string dataFieldSchema = "data";
        public string statusCodeFieldSchema = "statusCode";
        public string currentDateTimeFieldSchema = "currentDateTime";

        public string GetMessageField(JSONObject json)
        {
            return SafeStringFromSchema(json, messageFieldSchema);
        }
        
        public string GetDataField(JSONObject json)
        {
            return SafeStringFromSchema(json, dataFieldSchema);
        }
        
        public int GetStatusCodeField(JSONObject json)
        {
            return SafeIntFromSchema(json, statusCodeFieldSchema);
        }
        
        public string GetCurrentDateTimeField(JSONObject json)
        {
            return SafeStringFromSchema(json, currentDateTimeFieldSchema);
        }

        string SafeStringFromSchema(JSONObject json, string schema)
        {
            var currentField = GetTargetFieldFromSchema(json, schema, out var targetField);
            return currentField.SafeString(targetField);
        }
        
        int SafeIntFromSchema(JSONObject json, string schema)
        {
            var currentField = GetTargetFieldFromSchema(json, schema, out var targetField);
            return currentField.SafeInt(targetField);
        }

        public JSONObject GetTargetFieldFromSchema(JSONObject json, string schema, out string targetField)
        {
            var schemas = schema.Split(schemaNavigatorChar);
            JSONObject currentField = json;
            for (var i = 0; i < schemas.Length - 1; i++)
                currentField = currentField.GetField(schemas[i]);

            targetField = schemas.LastOrDefault();
            return currentField;
        }
    }
    
    [Serializable]
    public class StatusCodeRange
    {
        public int start;
        public int end;

        public bool IsBetweenRange(int value)
        {
            return start <= value && value <= end;
        }
        
        public static bool IsBetweenRange(int value , params StatusCodeRange[] ranges)
        {
            return ranges.Any(range => range.IsBetweenRange(value));
        }
    }
}