using System;
using System.Collections.Generic;
using System.Globalization;
using PhEngine.Core.Operation;
using PhEngine.JSON;
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

        public APIOperation(ClientRequest clientRequest, UnityWebRequest webRequest,ServerResultRule serverResultRule, bool isShowingLog) : base(webRequest)
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

        protected override bool IsWebRequestHasNoError()
        {
            return Result.status == ServerResultStatus.ServerReturnSuccess;
        }

        protected override ServerResult CreateResultFromWebRequest(UnityWebRequest request)
        {
            Result = new ServerResult(WebRequest, ServerResultRule, ClientRequest.DebugMode);
            NetworkEvent.InvokeOnAnyResultReceived(Result);
            return Result;
        }
    }
    
    public static class APIOperationExtensions
    {
        public static APIOperation Expect<T>(this APIOperation operation, Action<T> onSuccess) where T : JSONConvertibleObject
        {
            operation.OnSuccess +=  (result)=> onSuccess?.Invoke(GetResultObject<T>(result));
            return operation;
        }

        public static APIOperation ExpectList<T>(this APIOperation operation, Action<List<T>> onSuccess) where T : JSONConvertibleObject
        {
            operation.OnSuccess +=  (result)=> onSuccess?.Invoke(GetResultObjectList<T>(result));
            return operation;
        }

        static T GetResultObject<T>(ServerResult result) where T : JSONConvertibleObject
            => JSONConverter.To<T>(result.dataJson);

        static List<T> GetResultObjectList<T>(ServerResult result) where T : JSONConvertibleObject
            => JSONConverter.ToList<T>(result.dataJson);
    }
}