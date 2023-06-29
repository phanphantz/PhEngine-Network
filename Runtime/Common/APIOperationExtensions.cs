using System;
using System.Collections.Generic;
using PhEngine.Core.JSON;
using PhEngine.Core.Operation;

namespace PhEngine.Network
{
    public static class APIOperationExtensions
    {
        public static void Add(this Flow flow, API api)
        {
            flow.Add(api.CreateOperation());
        }
        
        public static APIOperation Expect<T>(this APIOperation operation, Action<T> onSuccess, T mockedData = default)
        {
            operation.OnSuccess += (result)=>
            {
                if (result.isMocked)
                {
                    onSuccess?.Invoke(mockedData);
                    return;
                }

                if (operation.TryGetResult<T>(out var resultObj))
                    onSuccess?.Invoke(resultObj);
            };
            return operation;
        }

        public static APIOperation ExpectJson(this APIOperation operation, Action<JSONObject> onSuccess, JSONObject mockedJson = null)
        {
            operation.OnSuccess += (result)=>
            {
                if (result.isMocked)
                {
                    onSuccess?.Invoke(mockedJson);
                    return;
                }

                if (operation.TryGetServerResult(out var resultObj))
                    onSuccess?.Invoke(resultObj.dataJson);
            };
            return operation;
        }

        public static APIOperation ExpectList<T>(this APIOperation operation, Action<List<T>> onSuccess, List<T> mockedDataList = default)
        {
            operation.OnSuccess +=  (result)=>
            {
                if (result.isMocked)
                {
                    onSuccess?.Invoke(mockedDataList);
                    return;
                }

                if (operation.TryGetResultList<T>(out var resultObjList))
                    onSuccess?.Invoke(resultObjList);
            };
            return operation;
        }
    }
}