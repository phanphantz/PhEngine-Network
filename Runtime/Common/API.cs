using PhEngine.JSON;
using UnityEngine;

namespace PhEngine.Network
{
    public abstract class API<T> where T : JSONConvertibleObject
    {
        [SerializeField] WebRequestForm form;
        
        public APIOperation Create()
        {
            var request1Op = APICaller.Instance.Create(form, CreateBody());
            request1Op.Expect<T>(OnReceiveResult, CreateMockedData());
            return request1Op;
        }

        protected abstract void OnReceiveResult(T result);
        protected abstract T CreateMockedData();
        protected abstract JSONObject CreateBody();
    }
}