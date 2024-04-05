using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

namespace PhEngine.Network
{
    public static class WebRequestFactory
    {
        internal static UnityWebRequest Create(APICallConfig config, BackendSetting environment, RequestHeader[] headerModifications, string accessToken, ClientRequest clientRequest)
        {
            var fullURL = GetFullURL(clientRequest, environment);
            var webRequest = new UnityWebRequest(fullURL, clientRequest.Verb.ToString());
            AddContent(clientRequest, webRequest);
            AddRequestHeaders(webRequest, config.clientRequestRule, clientRequest, headerModifications, accessToken);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.timeout = config.timeoutInSeconds;
            return webRequest;
        }
        
        static string GetFullURL(ClientRequest clientRequest, BackendSetting backend)
        {
            var fullURL = clientRequest.Destination;
            if (clientRequest.Type == WebRequestPathType.Endpoint)
            {
                if (backend == null)
                    throw new ArgumentNullException(nameof(backend));

                fullURL = backend.baseUrl + clientRequest.Destination;
            }

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
        
        static void AddRequestHeaders(UnityWebRequest request, ClientRequestRule requestRule, ClientRequest clientRequest, RequestHeader[] headerModifications, string accessToken)
        {
            accessToken ??= "";
            var requestHeaderList = clientRequest.HeaderList;
            request.SetRequestHeader(requestRule.accessTokenFieldName, requestRule.accessTokenPrefix + accessToken);
            var finalHeaders = CreateFinalHeaderSettings();
            
            //Overwrite to client request
            clientRequest.SetHeaders(finalHeaders);
            
            //Assign to web request
            foreach (var requestHeader in finalHeaders)
                request.SetRequestHeader(requestHeader.key, requestHeader.value);

            RequestHeader[] CreateFinalHeaderSettings()
            {
                var headerList = new List<RequestHeader>(requestRule.defaultRequestHeaders);
                headerList.AddRange(requestHeaderList);
                foreach (var modification in headerModifications)
                {
                    var existingHeader = headerList.FirstOrDefault(mod => mod.key == modification.key);
                    if (existingHeader != null)
                        existingHeader.value = modification.value;


                    headerList.Add(new RequestHeader(modification));
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
        public RequestHeader[] defaultRequestHeaders = new RequestHeader[]{};
    }
}