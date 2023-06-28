using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PhEngine.JSON;
using UnityEngine.Networking;

namespace PhEngine.Network
{
    public static class UnityWebRequestCreator
    {
        public static UnityWebRequest Create(ClientRequest clientRequest, string urlPrefix,int timeout, ClientRequestRule requestRule, RequestHeaderSetting[] headerModifications, string accessToken)
        {
            var fullURL = GetFullURL(clientRequest, urlPrefix);
            var webRequest = new UnityWebRequest(fullURL, clientRequest.Verb.ToString());
            AddContent(clientRequest, webRequest);
            AddRequestHeaders(webRequest, requestRule, headerModifications, accessToken);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.timeout = timeout;
            return webRequest;
        }
        
        static string GetFullURL(ClientRequest clientRequest, string urlPrefix = "")
        {
            var fullURL = clientRequest.Destination;
            if (clientRequest.Type == WebRequestPathType.Endpoint)
                fullURL = urlPrefix + clientRequest.Destination;

            return fullURL;
        }

        static void AddContent(ClientRequest clientRequest, UnityWebRequest unityWebRequest)
        {
            if (clientRequest.Content == null)
                return;
            
            var rawData = CreateUnityWebRequestRawData(clientRequest, clientRequest.Content, unityWebRequest);
            if (rawData != null)
                unityWebRequest.uploadHandler = new UploadHandlerRaw(rawData);
        }

        static byte[] CreateUnityWebRequestRawData
        (
            ClientRequest clientRequest,
            JSONObject json,
            UnityWebRequest request
        )
        {
            byte[] rawData = null;
            switch (clientRequest.ParameterType)
            {
                case ParameterType.Query:
                    AssignQueryTypeURL(json, request);
                    break;
                case ParameterType.Body:
                    rawData = CreateBodyTypeRequestRawData(json, request);
                    break;
                case ParameterType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return rawData;
        }

        static void AssignQueryTypeURL(JSONObject json, UnityWebRequest request)
        {
            if (json == null)
                return;

            request.url += "?";
            foreach (KeyValuePair<string, string> entry in json.ToDictionary())
            {
                if (entry.Value == null)
                    continue;

                var value = entry.Value.Replace("#", "%23");
                request.url += $"{entry.Key}={value}&";
            }
            request.url = request.url.Remove(request.url.Length - 1);
        }

        static byte[] CreateBodyTypeRequestRawData(JSONObject json, UnityWebRequest request)
        {
            var rawData = Encoding.UTF8.GetBytes(json.ToString());
            request.SetRequestHeader("Content-Type", "application/json");
            return rawData;
        }
        
        static void AddRequestHeaders(UnityWebRequest request, ClientRequestRule requestRule, RequestHeaderSetting[] headerSettingModifications, string accessToken)
        {
            accessToken ??= "";
            request.SetRequestHeader(requestRule.accessTokenFieldName, requestRule.accessTokenPrefix + accessToken);
            
            var headers = CreateFinalHeaderSettings(requestRule, headerSettingModifications);
            foreach (var requestHeader in headers)
                request.SetRequestHeader(requestHeader.key, requestHeader.value);
        }

        static RequestHeaderSetting[] CreateFinalHeaderSettings(ClientRequestRule requestRule,
            RequestHeaderSetting[] headerSettingModifications)
        {
            var headerList = new List<RequestHeaderSetting>();
            foreach (var setting in requestRule.additionalRequestHeaders)
            {
                var header = new RequestHeaderSetting(setting);
                var modification = headerSettingModifications.FirstOrDefault(mod => mod.key == header.key);
                if (modification != null)
                    header.value = modification.value;
                
                headerList.Add(header);
            }

            return headerList.ToArray();
        }
    }
    
    [Serializable]
    public class ClientRequestRule
    {
        public string accessTokenFieldName = "accessToken";
        public string accessTokenPrefix = "bearer ";
        public RequestHeaderSetting[] additionalRequestHeaders;
    }
    
    [Serializable]
    public class RequestHeaderSetting
    {
        public string key;
        public string value;

        public RequestHeaderSetting(RequestHeaderSetting setting)
        {
            CopyFrom(setting);
        }
        
        public void CopyFrom(RequestHeaderSetting newSetting)
        {
            key = newSetting.key;
            value = newSetting.value;
        }
    }
}