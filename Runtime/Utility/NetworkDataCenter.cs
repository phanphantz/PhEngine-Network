using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PhEngine.Core;
using PhEngine.Core.JSON;
using UnityEngine;

namespace PhEngine.Network
{
    public abstract class NetworkDataCenter<T> : MonoBehaviour, IService<NetworkServices>
    {
        public APIConfig GetRequestConfig => getRequestConfig;
        [SerializeField] APIConfig getRequestConfig;
        [SerializeField] protected List<T> elementList = new List<T>();

        public T SingleOrDefault => elementList.FirstOrDefault();
        public IReadOnlyList<T> ElementList => elementList?.AsReadOnly();
        public event Action<IReadOnlyList<T>> OnListUpdate;

        public virtual APIOperation CreateGetAPI()
        {
            return new APIOperation(getRequestConfig, CreateGetRequestBody());
        }
        
        public virtual APIOperation CreateGetListFromServer()
        {
            var apiOp = CreateGetAPI();
            apiOp.ExpectList<T>(AssignList);
            return apiOp;
        }

        public virtual APIOperation CreateGetSingleFromServer()
        {
            var apiOp = CreateGetAPI();
            apiOp.Expect<T>(AssignList);
            return apiOp;
        }

        protected virtual JSONObject CreateGetRequestBody() => new JSONObject();
        
        public void AssignList(ServerResult result) => AssignList(result.dataJson);
        public void AssignList(JSONObject json) => AssignList(JsonConvert.DeserializeObject<List<T>>(json.ToString()));

        public void AssignList(T data)
        {
            elementList.Clear();
            elementList.Add(data);
            InvokeOnListUpdate(ElementList);
        }
        
        public void AssignList(List<T> list)
        {
            elementList = list;
            InvokeOnListUpdate(ElementList);
        }

        public void InvokeOnListUpdate(IReadOnlyList<T> element)
        {
            OnListUpdate?.Invoke(element);
        }
    }
}