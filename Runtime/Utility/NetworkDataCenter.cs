using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PhEngine.Core;
using PhEngine.Core.JSON;
using PhEngine.Core.Operation;
using UnityEngine;

namespace PhEngine.Network
{
    public abstract class NetworkDataCenter<T> : MonoBehaviour, IService<NetworkServices>
    {
        [SerializeField] WebRequestFormConfig getRequestConfig;
        [SerializeField] protected List<T> elementList;

        public T SingleOrDefault => elementList.FirstOrDefault();
        public IReadOnlyList<T> ElementList => elementList.AsReadOnly();
        public event Action<IReadOnlyList<T>> OnElementUpdate;
        
        public APIOperation GetListFromServer()
        {
            var apiOp = CreateGetListAPI();
            apiOp.Run();
            return apiOp;
        }
        
        public APIOperation CreateGetListAPI()
        {
            var apiOp = new APIOperation(getRequestConfig, CreateGetRequestBody());
            apiOp.ExpectList<T>(Refresh);
            return apiOp;
        }
        
        public APIOperation GetSingleFromServer()
        {
            var apiOp = CreateGetListAPI();
            apiOp.Run();
            return apiOp;
        }
        
        public APIOperation CreateGetSingleAPI()
        {
            var apiOp = new APIOperation(getRequestConfig, CreateGetRequestBody());
            apiOp.Expect<T>(Refresh);
            return apiOp;
        }
        
        public abstract JSONObject CreateGetRequestBody();
        
        public void Refresh(ServerResult result) => Refresh(result.dataJson);
        public void Refresh(JSONObject json) => Refresh(JsonConvert.DeserializeObject<List<T>>(json.ToString()));

        public void Refresh(T data)
        {
            elementList.Clear();
            elementList.Add(data);
            InvokeOnElementUpdate(ElementList);
        }
        public void Refresh(List<T> list)
        {
            elementList = list;
            InvokeOnElementUpdate(ElementList);
        }

        public void InvokeOnElementUpdate(IReadOnlyList<T> element)
        {
            OnElementUpdate?.Invoke(element);
        }
    }
}