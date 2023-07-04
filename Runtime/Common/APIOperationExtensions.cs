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
            var apiOp = api.CreateOperation();
            flow.Add(apiOp);
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
        
        public static APIOperation ExpectRawString(this APIOperation operation, Action<string> onSuccess, string mockedString = null)
        {
            operation.OnSuccess += (result)=>
            {
                if (result.isMocked)
                {
                    onSuccess?.Invoke(mockedString);
                    return;
                }
                
                onSuccess?.Invoke(result.dataJson.str);
            };
            return operation;
        }
        
        public static APIOperation ExpectStringField(this APIOperation operation, string fieldName, Action<string> onSuccess, string mockedString = null)
        {
            operation.OnSuccess += (result)=>
            {
                if (result.isMocked)
                {
                    onSuccess?.Invoke(mockedString);
                    return;
                }
                
                onSuccess?.Invoke(result.dataJson.SafeString(fieldName));
            };
            return operation;
        }
        
        public static APIOperation ExpectIntField(this APIOperation operation, string fieldName, Action<int> onSuccess, int mockedValue = 0)
        {
            operation.OnSuccess += (result)=>
            {
                if (result.isMocked)
                {
                    onSuccess?.Invoke(mockedValue);
                    return;
                }
                
                onSuccess?.Invoke(result.dataJson.SafeInt(fieldName));
            };
            return operation;
        }
        
        public static APIOperation ExpectFloatField(this APIOperation operation, string fieldName, Action<float> onSuccess, float mockedValue = 0)
        {
            operation.OnSuccess += (result)=>
            {
                if (result.isMocked)
                {
                    onSuccess?.Invoke(mockedValue);
                    return;
                }
                
                onSuccess?.Invoke(result.dataJson.SafeFloat(fieldName));
            };
            return operation;
        }
        
        public static APIOperation ExpectBoolField(this APIOperation operation, string fieldName, Action<bool> onSuccess, bool mockedValue = false)
        {
            operation.OnSuccess += (result)=>
            {
                if (result.isMocked)
                {
                    onSuccess?.Invoke(mockedValue);
                    return;
                }
                
                onSuccess?.Invoke(result.dataJson.SafeBool(fieldName));
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