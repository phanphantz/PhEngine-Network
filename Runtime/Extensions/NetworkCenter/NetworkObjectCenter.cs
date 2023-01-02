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
            CreateGetAPI()
                .ExpectList<T>(Refresh)
                .RunOn(this);
        }
        
        public APIOperation CreateGetAPI()
        {
            return APICaller.Instance.Create(getRequestForm, CreateGetRequestBody());
        }
        
        public abstract JSONObject CreateGetRequestBody();
        
        public void Refresh(ServerResult result)
            => Refresh(result.dataJson);

        public void Refresh(JSONObject json)
            => Refresh(JSONConverter.ToList<T>(json));

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
}