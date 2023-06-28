using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using PhEngine.Core.Operation;
using PhEngine.JSON;
using UnityEngine;
using UnityEngine.Networking;

namespace PhEngine.Network
{
    public class APIOperation : NetworkOperation<ServerResult>
    {
        ServerResultRule ServerResultRule { get; }
        ClientRequest ClientRequest { get; }
        ServerResult Result { get; set; }
        bool IsShowingLog { get; }
        ClientRequestLogger Logger { get; }
        
        public APIOperation(ClientRequest clientRequest, UnityWebRequest webRequest,ServerResultRule serverResultRule, bool isShowingLog = false) : base(webRequest)
        {
            ServerResultRule = serverResultRule;
            ClientRequest = clientRequest;
            IsShowingLog = isShowingLog;
            Logger = new ClientRequestLogger(ClientRequest, WebRequest);
            OnStart += HandleOnSend;
            OnFinish += HandleOnReceiveResponse;
            OnSuccess += LogSuccess;
            OnFail += HandleOnFail;
        }

        void HandleOnSend()
        {
            if (ClientRequest.IsShowLoading)
                NetworkEvent.InvokeOnShowLoading(ClientRequest);
            
            NetworkEvent.InvokeOnAnyRequestSend(ClientRequest);
            if (!IsShowingLog)
                return;
            
            Logger.LogStartRequest();
        }

        void HandleOnReceiveResponse()
        {
            if (ClientRequest.IsShowLoading)
                NetworkEvent.InvokeOnHideLoading(ClientRequest);
            
            var receivedTime = GetTimeFromServerResult();
            NetworkEvent.SetLatestServerTimeByString(receivedTime);
            
            string GetTimeFromServerResult()
            {
                return ClientRequest.DebugMode == NetworkDebugMode.Off? Result.dateTime : DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
            }
        }
        
        void LogSuccess(ServerResult result)
        {
            if (IsShowingLog)
                Logger.LogSuccess(result);

            NetworkEvent.InvokeOnAnyServerSuccess(result);
        }

        void HandleOnFail(ServerResult result)
        {
            if (result.status == ServerResultStatus.ConnectionFail)
                NotifyConnectionFail(result);
            else if (result.status == ServerResultStatus.ServerReturnFail)
                NotifyServerReturnFail(result);
        }
        
        void NotifyConnectionFail(ServerResult result)
        {
            if (IsShowingLog)
                Logger.LogConnectionFail(result);

            if (ClientRequest.IsShowConnectionFailError)
                NetworkEvent.InvokeOnShowConnectionFailError(result);
            
            NetworkEvent.InvokeOnAnyConnectionFail(result);
        }

        void NotifyServerReturnFail(ServerResult result)
        {
            if (IsShowingLog)
                Logger.LogServerFail(result);
            
            if (ClientRequest.IsShowServerFailError)
                NetworkEvent.InvokeOnShowServerFailError(result);

            NetworkEvent.InvokeOnAnyServerFail(result);
        }
        
        protected override float GetWebRequestProgress()
        {
            return ClientRequest.DebugMode != NetworkDebugMode.Off ? 1f : base.GetWebRequestProgress();
        }

        protected override bool IsNetworkOperationSuccess()
        {
            return Result.status == ServerResultStatus.ServerReturnSuccess;
        }

        protected override ServerResult CreateResultFromWebRequest(UnityWebRequest request)
        {
            Result = new ServerResult(WebRequest, ServerResultRule, ClientRequest);
            NetworkEvent.InvokeOnAnyResultReceived(Result);
            return Result;
        }

        public void SetMockedResponse(JSONObject value)
        {
            if (value == null)
                return;
            
            SetMockedResponse(value.ToString());
        }
        
        public void SetMockedResponse(string value)
        {
            ClientRequest.SetMockedResponse(value);
        }

        public void SetDebugMode(NetworkDebugMode mode)
        {
            ClientRequest.SetDebugMode(mode);
        }

        public void SetMockedResponse(object value)
        {
            try
            {
                var str = JsonConvert.SerializeObject(value);
                SetMockedResponse(str);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
    
    public static class APIOperationExtensions
    {
        public static void Add(this Flow flow, API api)
        {
            flow.Add(api.CreateOperation());
        }
        
        public static APIOperation Expect<T>(this APIOperation operation, Action<T> onSuccess, T mockedData = default)
        {
            operation.OnSuccess += (result)=>
            {
                var resultObj = result.isMocked? mockedData : GetResultObject<T>(result);
                onSuccess?.Invoke(resultObj);
            };
            return operation;
        }

        public static APIOperation ExpectList<T>(this APIOperation operation, Action<List<T>> onSuccess, List<T> mockedDataList = default)
        {
            operation.OnSuccess +=  (result)=>
            {
                var resultList = result.isMocked ? mockedDataList : GetResultObjectList<T>(result);
                onSuccess?.Invoke(resultList);
            };
            return operation;
        }

        static T GetResultObject<T>(ServerResult result)
        {
            T resultObj = default;
            try
            {
                resultObj = JsonConvert.DeserializeObject<T>(result.dataJson.ToString());
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return resultObj;
        }

        static List<T> GetResultObjectList<T>(ServerResult result)
        {
            List<T> resultObjList = default;
            try
            {
                resultObjList = JsonConvert.DeserializeObject<List<T>>(result.dataJson.ToString()).ToList();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return resultObjList;
        }
    }
}