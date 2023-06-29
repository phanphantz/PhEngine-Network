using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using PhEngine.Core.JSON;
using PhEngine.Core.Operation;
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

        public bool TryGetServerResult(out ServerResult result)
        {
            result = null;
            if (Result == null)
            {
                Debug.LogError("TryGetServerResult<T> failed. Result is null.");
                return false;
            }

            result = Result;
            return true;
        }
        
        public bool TryGetResult<T>(out T resultObj)
        {
            resultObj = default;
            if (Result == null)
            {
                Debug.LogError("TryGetResult<T> failed. Server Result is null");
                return false;
            }

            if (Result.dataJson == null)
            {
                Debug.LogError("TryGetResult<T> failed. dataJson is null.");
                return false;
            }
            
            try
            {
                resultObj = JsonConvert.DeserializeObject<T>(Result.dataJson.ToString());
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
            return true;
        }

        public bool TryGetResultList<T>(out List<T> resultObjList)
        {
            resultObjList = default;
            if (Result == null)
            {
                Debug.LogError("TryGetResultList<T> failed. Server Result is null");
                return false;
            }

            if (Result.dataJson == null)
            {
                Debug.LogError("TryGetResultList<T> failed. dataJson is null");
                return false;
            }
            
            try
            {
                resultObjList = JsonConvert.DeserializeObject<List<T>>(Result.dataJson.ToString()).ToList();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }

            return true;
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
                if (result.isMocked)
                {
                    onSuccess?.Invoke(mockedData);
                    return;
                }

                if (operation.TryGetResult<T>(out var resultObj))
                    onSuccess?.Invoke(resultObj);
            };
            return operation;
        }

        public static APIOperation ExpectJson(this APIOperation operation, Action<JSONObject> onSuccess, JSONObject mockedJson = null)
        {
            operation.OnSuccess += (result)=>
            {
                if (result.isMocked)
                {
                    onSuccess?.Invoke(mockedJson);
                    return;
                }

                if (operation.TryGetServerResult(out var resultObj))
                    onSuccess?.Invoke(resultObj.dataJson);
            };
            return operation;
        }

        public static APIOperation ExpectList<T>(this APIOperation operation, Action<List<T>> onSuccess, List<T> mockedDataList = default)
        {
            operation.OnSuccess +=  (result)=>
            {
                if (result.isMocked)
                {
                    onSuccess?.Invoke(mockedDataList);
                    return;
                }

                if (operation.TryGetResultList<T>(out var resultObjList))
                    onSuccess?.Invoke(resultObjList);
            };
            return operation;
        }

       
    }
}