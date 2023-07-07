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
            
            var codeFromJson = fullJson.SafeInt(resultRule.statusCodeFieldName);
            if (codeFromJson != 0)
                code = codeFromJson;
            
            status = GetServerResultStatus();
            dateTime = fullJson.SafeString(resultRule.currentDateTimeFieldName);
            message = fullJson.SafeString(resultRule.messageFieldName);
            
            dataJson = fullJson;
            if (!string.IsNullOrEmpty(resultRule.dataFieldName))
            {
                var tryGetDataJsonFromField = fullJson.GetField(resultRule.dataFieldName);
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
        public string messageFieldName = "message";
        public string dataFieldName = "data";
        public string statusCodeFieldName = "statusCode";
        public string currentDateTimeFieldName = "currentDateTime";
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