using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PhEngine.Core.Operation;
using PhEngine.JSON;
using UnityEngine;

namespace PhEngine.Network
{
    [Serializable]
    public abstract class API
    {
        public event Action<ServerResult> onFail;
        
        public WebRequestForm Form => form;
        [SerializeField] protected WebRequestForm form;
        
        public void Call(NetworkDebugMode debugMode = NetworkDebugMode.Off)
        {
            var api = Create();
            api.SetDebugMode(debugMode);
            api.Run();
        }

        public virtual APIOperation Create()
        {
            var apiOp = Create(Form, CreateBody());
            apiOp.SetMockedResponse(CreateBody());
            apiOp.OnFail += (result) =>
            {
                OnFail(result);
                onFail?.Invoke(result);
            };
            return apiOp;
        }

        public abstract void OnFail(ServerResult result);

        protected virtual JSONObject CreateMockJson() => null;
        public virtual JSONObject CreateBody() => null;
        
        public static APIOperation Call(WebRequestForm form, JSONObject json = null)
        {
            var call = Create(form, json);
            Call(call);
            return call;
        }

        public static void Call(APIOperation operation) => operation.Run();

        public static APIOperation Create(API api)
        {
            return Create(api.Form, api.CreateBody());
        }

        public static APIOperation Create(WebRequestForm form, object requestData)
        {
            if (APICaller.Instance == null)
            {
                Debug.LogError("APICaller instance is missing");
                return null;
            }
            
            var jsonString = JsonConvert.SerializeObject(requestData);
            return APICaller.Instance.Create(form, new JSONObject(jsonString));
        }

        public static APIOperation Create(WebRequestForm form, JSONObject json = null)
        {
            if (APICaller.Instance == null)
            {
                Debug.LogError("APICaller instance is missing");
                return null;
            }
            
            return APICaller.Instance.Create(form, json);
        }
    }
    
    [Serializable]
    public abstract class RespondListAPI<T> : API
    {
        public event Action<List<T>> onReceiveDataList;
        public override APIOperation Create()
        {
            var apiOp = base.Create();
            apiOp.ExpectList
            (
                dataList =>
                {
                    OnReceiveDataList(dataList);
                    onReceiveDataList?.Invoke(dataList);
                }, 
                CreateMockedListData()
            );
            return apiOp;
        }

        public abstract void OnReceiveDataList(List<T> result);
        public virtual List<T> CreateMockedListData() => default;
    }
    
    [Serializable]
    public abstract class RespondSingleAPI<T> : API
    {
        public event Action<T> onReceiveData;
        public override APIOperation Create()
        {
            var apiOp = base.Create();
            apiOp.Expect
            ((data) =>
                {
                    OnReceiveData(data);
                    onReceiveData?.Invoke(data);
                }, 
                CreateMockedData()
            );
            return apiOp;
        }

        public abstract void OnReceiveData(T result);
        public virtual T CreateMockedData() => default;
    }

    [Serializable]
    public abstract class RespondJsonAPI : API
    {
        public event Action<JSONObject> onSuccess;
        public override APIOperation Create()
        {
            var apiOp = base.Create();
            apiOp.OnSuccess += (result) =>
            {
                OnReceiveJson(result.dataJson);
                onSuccess?.Invoke(result.dataJson);
            };
            return apiOp;
        }

        public abstract void OnReceiveJson(JSONObject result);
    }
}