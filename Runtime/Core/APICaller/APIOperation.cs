using System;
using System.Globalization;
using PhEngine.Core.Operation;
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
            BindActions();
        }

        void BindActions()
        {
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
            TryLogRequest();
        }
        
        void TryLogRequest()
        {
            if (!IsShowingLog)
                return;
            
            Logger.LogStartRequest();
        }

        void HandleOnReceiveResponse()
        {
            if (ClientRequest.IsShowLoading)
                NetworkEvent.InvokeOnHideLoading(ClientRequest);
            
            var receivedTime = GetTimeFromServerResult(ClientRequest, Result);
            NetworkEvent.SetLatestServerTimeByString(receivedTime);
        }

        static string GetTimeFromServerResult(ClientRequest request, ServerResult serverResult)
        {
            return request.DebugMode == NetworkDebugMode.Off? serverResult.dateTime : DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
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
            var resultCreator = new ServerResultCreator(ClientRequest, WebRequest, ServerResultRule);
            Result = resultCreator.Create();
            NetworkEvent.InvokeOnAnyResultReceived(Result);
            return Result;
        }
    }
}