using System;
using System.Collections.Generic;
using PhEngine.JSON;
using UnityEngine;

namespace PhEngine.Network
{
    public abstract class NetworkObjectCenter<T> : MonoBehaviour where T : JSONConvertibleObject
    {
        [SerializeField] protected List<T> elementList;
        [SerializeField] WebRequestForm getRequestForm;

        public IReadOnlyCollection<T> ElementList => elementList.AsReadOnly();
        public event Action<IReadOnlyCollection<T>> OnElementUpdate;

        public void GetFromServer()
        {
            var api = CreateGetAPI();
            api.ExpectList<T>(Refresh);
            api.RunOn(this);
        }
        
        public APIOperation CreateGetAPI()
        {
            return APICaller.Instance.Create(getRequestForm, CreateGetRequestBody());
        }
        
        public abstract JSONObject CreateGetRequestBody();
        
        public void Refresh(ServerResult result) => Refresh(result.dataJson);
        public void Refresh(JSONObject json) => Refresh(JSONConverter.ToList<T>(json));
        public void Refresh(List<T> list)
        {
            elementList = list;
            InvokeOnElementUpdate(ElementList);
        }

        public void InvokeOnElementUpdate(IReadOnlyCollection<T> element)
        {
            OnElementUpdate?.Invoke(element);
        }
    }
    
    public abstract class NetworkConfigCenter<T> : NetworkObjectCenter<T> where T : NetworkConfig
    {
        public override JSONObject CreateGetRequestBody() => null;
    }
    
    public abstract class NetworkConfig : JSONConvertibleObject
    {
        public abstract string ConfigId { get; }
    }
    
    public abstract class NetworkDataCenter<T> : NetworkObjectCenter<T> where T : NetworkData
    {
    }
    
    public abstract class NetworkData : JSONConvertibleObject
    {
        public abstract string ConfigId { get; }
        public abstract string UserId { get; }
    }
}