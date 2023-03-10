using System;
using PhEngine.JSON;
using UnityEngine;
using UnityEngine.Networking;

namespace PhEngine.Network
{
    public class ServerResultCreator
    {
        ClientRequest ClientRequest { get; }
        UnityWebRequest UnityWebRequest { get; }
        ServerResultRule ResultRule { get; }
        
        public ServerResultCreator(ClientRequest clientRequest, UnityWebRequest unityWebRequest, ServerResultRule resultRule)
        {
            ClientRequest = clientRequest;
            UnityWebRequest = unityWebRequest;
            ResultRule = resultRule;
        }

        public ServerResult Create()
        {
            return !IsRequestingMock(ClientRequest)
                ? CreateFromUnityWebRequest(UnityWebRequest)
                : CreateMockedResult(ClientRequest);
        }

        ServerResult CreateMockedResult(ClientRequest clientRequest)
        {
            var mockedStatus = clientRequest.DebugMode switch
            {
                NetworkDebugMode.MockConnectionFail => ServerResultStatus.ConnectionFail,
                NetworkDebugMode.MockServerReturnFail => ServerResultStatus.ServerReturnFail,
                NetworkDebugMode.MockServerReturnSuccess => ServerResultStatus.ServerReturnSuccess,
                NetworkDebugMode.Off => ServerResultStatus.ServerReturnSuccess,
                _ => throw new ArgumentOutOfRangeException()
            };

            return new ServerResult(mockedStatus);
        }

        static bool IsRequestingMock(ClientRequest clientRequest)
        {
            return clientRequest.DebugMode != NetworkDebugMode.Off;
        }

        ServerResult CreateFromUnityWebRequest(UnityWebRequest request)
        {
            var code = (int)request.responseCode;
            var resultStatus = GetServerResultStatus(code);
            if (!IsWebRequestResponseValid(request))
                return new ServerResult(code, resultStatus);

            var fullJson = new JSONObject(request.downloadHandler.text);
            var currentDateTime = GetCurrentDateTime(fullJson);
            var message = GetMessage(fullJson);
            var dataJson = GetDataJson(fullJson);
            code = GetStatusCode(fullJson);
            resultStatus = GetServerResultStatus(code);
            return new ServerResult(code, fullJson, dataJson, message, currentDateTime, resultStatus);
        }

        bool IsWebRequestResponseValid(UnityWebRequest request)
        {
            if (request == null)
                return false;

            if (request.downloadHandler == null)
                return false;

            return !string.IsNullOrEmpty(request.downloadHandler.text);
        }
        
        int GetStatusCode(JSONObject json)
        {
            return json.SafeInt(ResultRule.statusCodeFieldName);
        }

        string GetCurrentDateTime(JSONObject json)
        {
            return json.SafeString(ResultRule.currentDateTimeFieldName);
        }

        string GetMessage(JSONObject json)
        {
            return json.SafeString(ResultRule.messageFieldName);
        }

        JSONObject GetDataJson(JSONObject json)
        {
            if (string.IsNullOrEmpty(ResultRule.dataFieldName))
                return json;
            
            return json.GetField(ResultRule.dataFieldName);
        }

        ServerResultStatus GetServerResultStatus(int code)
        {
            return IsConnectionFailed() ? 
                ServerResultStatus.ConnectionFail : 
                GetServerResultStatusFromServerResponse(code);
        }

        bool IsConnectionFailed()
        {
            return IsSimulatingConnectionFailed() || UnityWebRequest.error != null;
        }

        bool IsSimulatingConnectionFailed()
        {
            return ClientRequest.DebugMode == NetworkDebugMode.MockConnectionFail;
        }

        ServerResultStatus GetServerResultStatusFromServerResponse(int code)
        {
            return IsServerReturnedFail(code) ? 
                ServerResultStatus.ServerReturnFail : 
                ServerResultStatus.ServerReturnSuccess;
        }

        bool IsServerReturnedFail(int code)
        {
            return IsSimulatingReturnedFail() ||
                   !IsServerSuccessCode(code);
        }

        bool IsSimulatingReturnedFail()
        {
            return ClientRequest.DebugMode == NetworkDebugMode.MockServerReturnFail;
        }
        
        bool IsServerSuccessCode(int code)
        {
            var successCodeRanges = ResultRule.successStatusCodeRanges;
            if (successCodeRanges == null || successCodeRanges.Length == 0)
            {
                Debug.LogWarning("There is no provided success code ranges to determine ServerResultStatus. So all result will be marked as Success.");
                return true;
            }
            
            return StatusCodeRange.IsBetweenRange(code, successCodeRanges);
        }
    }
}