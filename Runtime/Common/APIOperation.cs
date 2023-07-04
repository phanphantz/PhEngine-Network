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
        ClientRequest ClientRequest { get; set; }
        ServerResult Result { get; set; }

        APILogOption logOption;
        APILogger logger;
        ServerResultRule serverResultRule;

        bool IsShowingLog => logOption != APILogOption.None;

        public APIOperation(WebRequestForm form, object data)
        {
            var jsonString = JsonConvert.SerializeObject(data);
            Initialize(form, new JSONObject(jsonString));
        }

        public APIOperation(WebRequestForm form, JSONObject json = null)
        {
            Initialize(form, json);
        }

        void Initialize(WebRequestForm form, JSONObject json)
        {
            ClientRequest = new ClientRequest(form, json);
            OnStart += HandleOnSend;
            OnFinish += HandleOnReceiveResponse;
            OnSuccess += LogSuccess;
            OnFail += HandleOnFail;
        }

        protected override UnityWebRequest CreateWebRequest()
        {
            var builder = APICaller.Instance.GetBuilder();
            if (builder.Config.isForceUseNetworkDebugMode)
                SetDebugMode(builder.Config.networkDebugMode);

            logOption = builder.Config.logOption;
            serverResultRule = builder.Config.serverResultRule;
            return WebRequestFactory.Create(builder, ClientRequest);
        }

        public override void RunOn(MonoBehaviour target)
        {
            var caller = APICaller.Instance;
            if (caller == null)
            {
                Debug.LogError("APICaller is missing. APIOperation is aborted.");
                return;
            }
            
            if (caller.AccessTokenValidator)
                caller.AccessTokenValidator.Track(this, ParentFlow);
            
            base.RunOn(target);
        }

        void HandleOnSend()
        {
            if (ClientRequest.IsShowLoading)
                NetworkEvent.InvokeOnShowLoading(ClientRequest);
            
            NetworkEvent.InvokeOnAnyRequestSend(ClientRequest);
            if (!IsShowingLog)
                return;
            
            logger = new APILogger(ClientRequest, WebRequest, logOption);
            logger.LogStartRequest();
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
                logger.LogSuccess(result);

            NetworkEvent.InvokeOnAnyServerSuccess(result);
        }

        void HandleOnFail(ServerResult result)
        {
            if (result.status == ServerResultStatus.ConnectionFail)
                NotifyConnectionFail(result);
            else if (result.status == ServerResultStatus.ServerReturnFail)
                NotifyServerReturnFail(result);
            else if (result.status == ServerResultStatus.ClientFail)
                NotifyClientFail(result);
        }

        void NotifyConnectionFail(ServerResult result)
        {
            if (IsShowingLog)
                logger.LogConnectionFail(result);

            if (ClientRequest.IsShowConnectionFailError)
                NetworkEvent.InvokeOnShowConnectionFailError(result);
            
            NetworkEvent.InvokeOnAnyConnectionFail(result);
        }

        void NotifyServerReturnFail(ServerResult result)
        {
            if (IsShowingLog)
                logger.LogServerFail(result);
            
            if (ClientRequest.IsShowServerFailError)
                NetworkEvent.InvokeOnShowServerFailError(result);

            NetworkEvent.InvokeOnAnyServerFail(result);
        }
        
        void NotifyClientFail(ServerResult result)
        {
            if (IsShowingLog)
                Debug.LogError($"Client Fail : {result.message} (Code: {result.code})");
            
            if (ClientRequest.IsShowConnectionFailError)
                NetworkEvent.InvokeOnShowClientFailError(result);
            
            NetworkEvent.InvokeOnAnyClientFail(result);
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
            var elapsedTimeSeconds = (float)ElapsedRealTime.TotalSeconds;
            Result = new ServerResult(WebRequest, serverResultRule, ClientRequest, elapsedTimeSeconds);
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

        public void SetRequestBody(JSONObject json)
        {
            ClientRequest.SetContent(json);
        }

        public void SetRequestBody(object data)
        {
            var jsonString = JsonConvert.SerializeObject(data);
            SetRequestBody(new JSONObject(jsonString));
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

        public void AbortOn(Func<bool> condition, string error = "", FailureHandling failureHandling = FailureHandling.None, int errorCode = 0)
        {
            GuardCondition += () =>
            {
                var result = condition.Invoke();
                if (result)
                    Result = ServerResult.CreateClientFailResult(error, failureHandling, errorCode);

                return result;
            };
        }
        
        protected override ServerResult GetGuardConditionResult() => Result;
    }
}