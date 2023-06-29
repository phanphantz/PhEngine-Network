using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

namespace PhEngine.Network
{
    internal static class WebRequestFactory
    {
        internal static UnityWebRequest Create(ClientRequest clientRequest, string urlPrefix,int timeout, ClientRequestRule requestRule, RequestHeaderSetting[] headerModifications, string accessToken)
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
            var json = clientRequest.Content;
            if (json == null)
                return;
            
            switch (clientRequest.ParameterType)
            {
                case ParameterType.Query:
                    AssignQueryURL();
                    break;
                
                case ParameterType.Body:
                    AssignBodyRawData();
                    break;
                
                case ParameterType.Path:
                    unityWebRequest.url += json.str;
                    return;
                
                case ParameterType.None:
                    return;
                
                default:
                    throw new NotImplementedException();
            }
            
            void AssignQueryURL()
            {
                unityWebRequest.url += "?";
                foreach (KeyValuePair<string, string> entry in json.ToDictionary())
                {
                    if (entry.Value == null)
                        continue;

                    var value = entry.Value.Replace("#", "%23");
                    unityWebRequest.url += $"{entry.Key}={value}&";
                }
                unityWebRequest.url = unityWebRequest.url.Remove(unityWebRequest.url.Length - 1);
            }
            
            void AssignBodyRawData()
            {
                var rawData = Encoding.UTF8.GetBytes(json.ToString());
                unityWebRequest.SetRequestHeader("Content-Type", "application/json");
                unityWebRequest.uploadHandler = new UploadHandlerRaw(rawData);
            }
        }
        
        static void AddRequestHeaders(UnityWebRequest request, ClientRequestRule requestRule, RequestHeaderSetting[] headerSettingModifications, string accessToken)
        {
            accessToken ??= "";
            request.SetRequestHeader(requestRule.accessTokenFieldName, requestRule.accessTokenPrefix + accessToken);

            if (requestRule.additionalRequestHeaders == null)
                return;
            
            var headers = CreateFinalHeaderSettings();
            foreach (var requestHeader in headers)
                request.SetRequestHeader(requestHeader.key, requestHeader.value);
            
            RequestHeaderSetting[] CreateFinalHeaderSettings()
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
            key = setting.key;
            value = setting.value;
        }
    }
}