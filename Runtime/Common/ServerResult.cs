using System;
using System.Linq;
using PhEngine.JSON;
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
        public ServerResultStatus status;
        public bool isMocked;

        public ServerResult() {}
        
        public ServerResult(UnityWebRequest unityWebRequest, ServerResultRule resultRule, NetworkDebugMode debugMode = NetworkDebugMode.Off)
        {
            if (debugMode != NetworkDebugMode.Off)
            {
                isMocked = true;
                MockStatus(debugMode);
                return;
            }
            
            if (!IsHasResponse())
            {
                code = (int)unityWebRequest.responseCode;
                status = GetServerResultStatus();
                return;
            }

            fullJson = new JSONObject(unityWebRequest.downloadHandler.text);
            code = fullJson.SafeInt(resultRule.statusCodeFieldName);
            status = GetServerResultStatus();
            dateTime = fullJson.SafeString(resultRule.currentDateTimeFieldName);
            message = fullJson.SafeString(resultRule.messageFieldName);
            dataJson = GetDataJson(fullJson);
            
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
            
            JSONObject GetDataJson(JSONObject json)
            {
                if (string.IsNullOrEmpty(resultRule.dataFieldName))
                    return json;
            
                return json.GetField(resultRule.dataFieldName);
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