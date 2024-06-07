using System;
using System.Collections.Generic;
using PhEngine.Core.JSON;
using PhEngine.Core.Operation;
using UnityEngine;

namespace PhEngine.Network
{
    [Serializable]
    public abstract class API
    {
        public APIForm Form => new APIForm(form);
        [SerializeField] protected APIForm form;
        
        public APIOperation CreateOperation(MockMode mockMode)
        {
            var api = CreateOperation();
            api.SetMockMode(mockMode);
            return api;
        }

        public virtual APIOperation CreateOperation()
        {
            var apiOp = new APIOperation(Form, CreateBody());
            apiOp.SetMockedResponseData(CreateBody());
            apiOp.OnFail += OnFail;
            return apiOp;
        }

        public abstract void OnFail(ServerResult result);

        protected virtual JSONObject CreateMockJson() => null;
        public virtual JSONObject CreateBody() => null;
        
        public static APIOperation Call(APIForm form, JSONObject json = null)
        {
            var call = new APIOperation(form, json);
            Call(call);
            return call;
        }
        
        public static APIOperation Call(APIForm form, object requestData)
        {
            var call = new APIOperation(form, requestData);
            Call(call);
            return call;
        }

        public static void Call(APIOperation operation) => operation.Run();
    }
    
    [Serializable]
    public abstract class RespondListAPI<T> : API
    {
        public override APIOperation CreateOperation()
        {
            var apiOp = base.CreateOperation();
            apiOp.ExpectList
            (
                OnReceiveDataList, 
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
        public override APIOperation CreateOperation()
        {
            var apiOp = base.CreateOperation();
            apiOp.Expect(OnReceiveData, CreateMockedData());
            return apiOp;
        }

        public abstract void OnReceiveData(T result);
        public virtual T CreateMockedData() => default;
    }

    [Serializable]
    public abstract class RespondJsonAPI : API
    {
        public override APIOperation CreateOperation()
        {
            var apiOp = base.CreateOperation();
            apiOp.OnSuccess += (result) => { OnReceiveJson(result.dataJson); };
            return apiOp;
        }

        public abstract void OnReceiveJson(JSONObject result);
    }
}