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

        #region Constructors
        
        public APIOperation(WebRequestFormConfig config, object data)
        {
            var jsonString = JsonConvert.SerializeObject(data);
            Initialize(config.Form, new JSONObject(jsonString));
        }

        public APIOperation(WebRequestForm form, object data)
        {
            var jsonString = JsonConvert.SerializeObject(data);
            Initialize(form, new JSONObject(jsonString));
        }
        
        public APIOperation(WebRequestFormConfig config, JSONObject json = null)
        {
            Initialize(config.Form, json);
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
        
        #endregion
        
        public override void RunOn(MonoBehaviour target)
        {
            if (!RetrieveCaller(out var caller)) 
                return;

            if (caller.AccessTokenValidator)
                caller.AccessTokenValidator.BindValidateActions(this);
            
            base.RunOn(target);
        }

        protected override UnityWebRequest CreateWebRequest()
        {
            return APICaller.Instance.CreateWebRequest(this);
        }

#if UNITASK
        protected override async UniTask PreProcessTask()
        {
            await base.PreProcessTask();
            var caller = APICaller.Instance;
            if (caller.AccessTokenValidator)
                await caller.AccessTokenValidator.ValidateBeforeCallTask(this);
        }
        
        protected override async UniTask PostProcessTask()
        {
            var caller = APICaller.Instance;
            var isAccessTokenRefreshed = false;
            if (caller.AccessTokenValidator)
                isAccessTokenRefreshed = await caller.AccessTokenValidator.TryValidateAfterCallTask(this,Result);

            //If Access Token is handled, no need to terminate pending tasks
            if (isAccessTokenRefreshed)
                return;
            
            await base.PostProcessTask();
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
                return ClientRequest.TestMode == TestMode.Off? Result.dateTime : DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
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
        
        protected override bool IsShouldFinish => base.IsShouldFinish || ClientRequest.TestMode != TestMode.Off;
        protected override float GetWebRequestProgress()
        {
            return ClientRequest.TestMode != TestMode.Off ? 1f : base.GetWebRequestProgress();
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

        #region Public Set Methods

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

        public void SetDebugMode(TestMode mode)
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

        #endregion

        #region Public Result Getters
        
        public bool TryGetServerResult(out ServerResult result)
        {
            result = null;
            if (Result == null)
                return false;

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
                return false;

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
        
        #endregion
        
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
        
        public void AddHeader(RequestHeader header)
        {
            ClientRequest.AddHeader(header);
        }
        
#if UNITASK
        public async UniTask<JSONObject> JsonTask(JSONObject mockData = null)
        {
            await Task();
            if (IsMockedResult())
                return mockData;
            
            if (!TryGetJson(out var json))
                throw new OperationCanceledException();

            return json;
        }

        bool IsMockedResult()
        {
            return Result != null && Result.isMocked;
        }

        public async UniTask<T> Task<T>(T mockData = default)
        {
            await Task();
            if (IsMockedResult())
                return mockData;
            
            if (TryGetResult<T>(out var resultObj))
                return resultObj;

            throw new OperationCanceledException();
        }
        
        public async UniTask<List<T>> ListTask<T>(List<T> mockDataList = null)
        {
            await Task();
            if (IsMockedResult())
                return mockDataList;
            
            if (TryGetResultList<T>(out var resultObj))
                return resultObj;

            throw new OperationCanceledException();
        }
        
        public async UniTask<string> RawStringTask(string mockData = null)
        {
            await Task();
            if (IsMockedResult())
                return mockData;
            
            if (!TryGetJson(out var json))
                throw new OperationCanceledException();
            
            return json.ToString();
        }
        
        public async UniTask<string> StringFieldTask(string fieldName, string mockData = null)
        {
            await Task();
            if (IsMockedResult())
                return mockData;
            
            if (!TryGetJson(out var json))
                throw new OperationCanceledException();
            
            return json.SafeString(fieldName);
        }
        
        public async UniTask<int> IntFieldTask(string fieldName, int mockData = 0)
        {
            await Task();
            if (IsMockedResult())
                return mockData;
            
            if (!TryGetJson(out var json))
                throw new OperationCanceledException();
            
            return json.SafeInt(fieldName);
        }
        
        public async UniTask<float> FloatFieldTask(string fieldName, float mockData = 0)
        {
            await Task();
            if (IsMockedResult())
                return mockData;
            
            if (!TryGetJson(out var json))
                throw new OperationCanceledException();
            
            return json.SafeFloat(fieldName);
        }
        
        public async UniTask<bool> BoolFieldTask(string fieldName, bool mockData = false)
        {
            await Task();
            if (IsMockedResult())
                return mockData;
            
            if (!TryGetJson(out var json))
                throw new OperationCanceledException();
            
            return json.SafeBool(fieldName);
        }
#endif
    }
}