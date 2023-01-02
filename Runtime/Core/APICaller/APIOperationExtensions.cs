using System;
using System.Collections.Generic;
using PhEngine.JSON;

namespace PhEngine.Network
{
    public static class APIOperationExtensions
    {
        public static APIOperation Expect<T>(this APIOperation operation, Action<T> onSuccess) where T : JSONConvertibleObject
        {
            operation.OnSuccess +=  (result)=> onSuccess.Invoke(GetResultObject<T>(result));
            return operation;
        }

        public static APIOperation ExpectList<T>(this APIOperation operation, Action<List<T>> onSuccess) where T : JSONConvertibleObject
        {
            operation.OnSuccess +=  (result)=> onSuccess.Invoke(GetResultObjectList<T>(result));
            return operation;
        }

        static T GetResultObject<T>(ServerResult result) where T : JSONConvertibleObject
            => JSONConverter.To<T>(result.dataJson);

        static List<T> GetResultObjectList<T>(ServerResult result) where T : JSONConvertibleObject
            => JSONConverter.ToList<T>(result.dataJson);
    }
}