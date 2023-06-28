using System;
using PhEngine.Core.Operation;
using PhEngine.JSON;

namespace PhEngine.Network
{
    [Serializable]
    public abstract class API<T> : API
    {
        public override APIOperation Create()
        {
            var apiOp = base.Create();
            apiOp.Expect(OnReceiveData, CreateMockedData());
            return apiOp;
        }

        public abstract void OnReceiveData(T result);

        protected virtual T CreateMockedData() => default;
    }

    [Serializable]
    public abstract class API
    {
        public abstract WebRequestForm Form { get; }
        public void Call()
        {
            var api = Create();
            api.Run();
        }

        public virtual APIOperation Create()
        {
            var apiOp = APICaller.Instance.Create(Form, CreateBody());
            apiOp.SetMockedResponse(CreateBody());
            apiOp.OnSuccess += (result) => OnReceiveJson(result.dataJson);
            apiOp.OnFail += OnFail;
            return apiOp;
        }

        public abstract void OnFail(ServerResult result);
        public virtual void OnReceiveJson(JSONObject result) {}

        protected virtual JSONObject CreateMockJson() => null;
        public virtual JSONObject CreateBody() => null;
    }
}