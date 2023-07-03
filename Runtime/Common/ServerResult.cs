using System;
using System.Net;
using System.Linq;
using PhEngine.Core.JSON;
using UnityEngine;
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
        public HttpStatusCode HttpStatus;
        public bool isMocked;
        public FailureHandling failureHandling;

        public ServerResult() {}
        
        public ServerResult(UnityWebRequest unityWebRequest, ServerResultRule resultRule, ClientRequest clientRequest, float elapsedTimeSeconds)
        {
            this.elapsedTimeSeconds = elapsedTimeSeconds;
            failureHandling = clientRequest.FailureHandling;
            if (clientRequest.DebugMode != NetworkDebugMode.Off)
            {
                isMocked = true;
                MockStatus(clientRequest.DebugMode);
                dataJson = new JSONObject(clientRequest.MockedResponse);
                return;
            }
            
            code = (int)unityWebRequest.responseCode;
            HttpStatus = (HttpStatusCode)code;
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
            
            if (string.IsNullOrEmpty(resultRule.dataFieldName))
                dataJson = fullJson;
            else
                dataJson = fullJson.GetField(resultRule.dataFieldName);
            
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
                {
                    Debug.LogWarning("There is no provided success code ranges to determine ServerResultStatus. So all result will be marked as Success.");
                    return ServerResultStatus.ServerReturnSuccess;
                }

                if (StatusCodeRange.IsBetweenRange(code, successCodeRanges))
                    return ServerResultStatus.ServerReturnSuccess;
            
                return ServerResultStatus.ServerReturnFail;
            }
        }

        void MockStatus(NetworkDebugMode debugMode)
        {
            status = debugMode switch
            {
                NetworkDebugMode.MockConnectionFail => ServerResultStatus.ConnectionFail,
                NetworkDebugMode.MockServerReturnFail => ServerResultStatus.ServerReturnFail,
                NetworkDebugMode.MockServerReturnSuccess => ServerResultStatus.ServerReturnSuccess,
                NetworkDebugMode.Off => ServerResultStatus.ServerReturnSuccess,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
    
    public enum ServerResultStatus
    {
        ServerReturnSuccess, ServerReturnFail, ConnectionFail 
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