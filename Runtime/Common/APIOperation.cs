using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using PhEngine.Core.JSON;
using PhEngine.Core.Operation;
using UnityEngine;
using UnityEngine.Networking;

#if UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace PhEngine.Network
{
    public class APIOperation : NetworkOperation<ServerResult>
    {
        internal ClientRequest ClientRequest { get; private set; }
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
            return APICaller.Instance.CreateWebRequest(this);
        }

        public override void RunOn(MonoBehaviour target)
        {
            if (!RetrieveCaller(out var caller)) 
                return;

            if (caller.AccessTokenValidator)
                caller.AccessTokenValidator.BindValidateActions(this);
            
            base.RunOn(target);
        }

#if UNITASK
        protected override async UniTask PreProcessTask()
        {
            var caller = APICaller.Instance;
            if (caller.AccessTokenValidator)
                await caller.AccessTokenValidator.ValidateBeforeCallTask(this);
        }
        
        protected override async UniTask PostProcessTask()
        {
            var caller = APICaller.Instance;
            if (caller.AccessTokenValidator)
                await caller.AccessTokenValidator.ValidateAfterCallTask(this,Result);
        }
#endif

        static bool RetrieveCaller(out APICaller caller)
        {
            caller = APICaller.Instance;
            if (caller == null)
            {
                Debug.LogError("APICaller is missing. APIOperation is aborted.");
                return false;
            }

            if (caller.Config == null)
            {
                Debug.LogError("Cannot Prepare API Operation. APICallConfig is missing.");
                return false;
            }

            return true;
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
                NotifyClientFail(new ClientError(result));
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
        
        void NotifyClientFail(ClientError error)
        {
            if (IsShowingLog)
                Debug.LogError(error);
            
            if (ClientRequest.IsShowErrorOnClientFail)
                NetworkEvent.InvokeOnShowClientFailError(error);
            
            NetworkEvent.InvokeOnAnyClientFail(error);
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
            var result = new ServerResult(WebRequest, serverResultRule, ClientRequest, elapsedTimeSeconds);
            NetworkEvent.InvokeOnAnyResultReceived(result);
            return result;
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
                Debug.LogError("Result is null.");
                return false;
            }

            result = Result;
            return true;
        }
        
        public bool TryGetResult<T>(out T resultObj)
        {
            resultObj = default;
            if (!TryGetJson(out var json)) 
                return false;

            try
            {
                resultObj = JsonConvert.DeserializeObject<T>(json.ToString());
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
            return true;
        }

        bool TryGetJson(out JSONObject json)
        {
            json = null;
            if (!TryGetServerResult(out var result))
                return false;

            if (result.dataJson == null)
            {
                Debug.LogError("dataJson is null.");
                return false;
            }

            json =  result.dataJson;
            return true;
        }

        public bool TryGetResultList<T>(out List<T> resultObjList)
        {
            resultObjList = default;
            if (!TryGetJson(out var json)) 
                return false;
            
            try
            {
                var enumerator = JsonConvert.DeserializeObject<List<T>>(json.ToString());
                resultObjList = enumerator?.ToList();
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
                    SetResult(ServerResult.CreateClientFailResult(error, failureHandling, errorCode));

                return result;
            };
        }
        
        protected override ServerResult GetGuardConditionResult() => Result;

        internal void SetLogOption(APILogOption option)
        {
            logOption = option;
        }

        internal void SetServerResultRule(ServerResultRule rule)
        {
            serverResultRule = rule;
        }
        
#if UNITASK
        public async UniTask<JSONObject> JsonTask()
        {
            await Task();
            if (!TryGetJson(out var json))
                return new JSONObject();

            return json;
        }
        
        public async UniTask<T> Task<T>()
        {
            await Task();
            if (TryGetResult<T>(out var resultObj))
                return resultObj;

            return default;
        }
        
        public async UniTask<List<T>> ListTask<T>()
        {
            await Task();
            if (TryGetResultList<T>(out var resultObj))
                return resultObj;

            return default;
        }
        
        public async UniTask<string> RawStringTask()
        {
            await Task();
            if (!TryGetJson(out var json))
                return string.Empty;
            
            return json.ToString();
        }
        
        public async UniTask<string> StringFieldTask(string fieldName)
        {
            await Task();
            if (!TryGetJson(out var json))
                return string.Empty;
            
            return json.SafeString(fieldName);
        }
        
        public async UniTask<int> IntFieldTask(string fieldName)
        {
            await Task();
            if (!TryGetJson(out var json))
                return 0;
            
            return json.SafeInt(fieldName);
        }
        
        public async UniTask<float> FloatFieldTask(string fieldName)
        {
            await Task();
            if (!TryGetJson(out var json))
                return 0;
            
            return json.SafeFloat(fieldName);
        }
        
        public async UniTask<bool> BoolFieldTask(string fieldName)
        {
            await Task();
            if (!TryGetJson(out var json))
                return false;
            
            return json.SafeBool(fieldName);
        }
#endif
    }
}