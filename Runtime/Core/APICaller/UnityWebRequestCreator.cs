using System;
using System.Collections.Generic;
using System.Text;
using PhEngine.JSON;
using UnityEngine.Networking;

namespace PhEngine.Network
{
    public static class UnityWebRequestCreator
    {
        public static UnityWebRequest CreateUnityWebRequest(ClientRequest clientRequest, string urlPrefix,int timeout, ClientRequestRule requestRule, string accessToken)
        {
            var fullURL = GetFullURL(clientRequest, urlPrefix);
            return CreateUnityWebRequest(clientRequest, timeout, requestRule, accessToken, fullURL);
        }
        
        static string GetFullURL(ClientRequest clientRequest, string urlPrefix = "")
        {
            var fullURL = clientRequest.Destination;
            if (clientRequest.Type == WebRequestPathType.Endpoint)
                fullURL = urlPrefix + clientRequest.Destination;

            return fullURL;
        }

        static UnityWebRequest CreateUnityWebRequest(ClientRequest clientRequest, int timeout, ClientRequestRule requestRule,
            string accessToken, string fullURL)
        {
            var result = new UnityWebRequest(fullURL, clientRequest.Verb.ToString());
            InitializeWebRequest(clientRequest, timeout, requestRule, accessToken, result);
            return result;
        }

        static void InitializeWebRequest(ClientRequest clientRequest, int timeout, ClientRequestRule requestRule,
            string accessToken, UnityWebRequest result)
        {
            TryInjectJsonIntoUnityWebRequest(clientRequest, result);
            InjectAccessTokenAndAdditionalRequestHeaders(result, requestRule, accessToken);
            result.downloadHandler = new DownloadHandlerBuffer();
            result.timeout = timeout;
        }

        static void TryInjectJsonIntoUnityWebRequest(ClientRequest clientRequest, UnityWebRequest unityWebRequest)
        {
            var json = clientRequest.Content;
            if (json == null)
                return;

            InjectJsonIntoUnityWebRequest(clientRequest, unityWebRequest, json);
        }

        static void InjectJsonIntoUnityWebRequest(ClientRequest clientRequest, UnityWebRequest unityWebRequest, JSONObject json)
        {
            var rawData = CreateUnityWebRequestRawData(clientRequest, json, unityWebRequest);
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
            AssignEachFieldAsQueries(json, request);
        }

        static void AssignEachFieldAsQueries(JSONObject json, UnityWebRequest request)
        {
            foreach (KeyValuePair<string, string> entry in json.ToDictionary())
            {
                if (entry.Value == null)
                    continue;

                AssignFieldAsQuery(request, entry);
            }
            request.url = request.url.Remove(request.url.Length - 1);
        }

        static void AssignFieldAsQuery(UnityWebRequest request, KeyValuePair<string, string> entry)
        {
            var value = entry.Value.Replace("#", "%23");
            request.url += $"{entry.Key}={value}&";
        }

        static byte[] CreateBodyTypeRequestRawData(JSONObject json, UnityWebRequest request)
        {
            var rawData = Encoding.UTF8.GetBytes(json.ToString());
            request.SetRequestHeader("Content-Type", "application/json");
            return rawData;
        }
        
        static void InjectAccessTokenAndAdditionalRequestHeaders(UnityWebRequest request, ClientRequestRule requestRule, string accessToken)
        {
            InjectAccessTokenHeader(request, requestRule, accessToken);
            InjectAdditionalRequestHeaders(request, requestRule.additionalRequestHeaders);
        }

        static void InjectAccessTokenHeader(UnityWebRequest request, ClientRequestRule requestRule, string accessToken)
        {
            var accessTokenToInject = accessToken;
            request.SetRequestHeader(requestRule.accessTokenFieldName, requestRule.accessTokenPrefix + GetNullSafeString(accessTokenToInject));
        }

        static string GetNullSafeString(string accessTokenToInject)
        {
            return string.IsNullOrEmpty(accessTokenToInject) ? string.Empty : accessTokenToInject;
        }

        static void InjectAdditionalRequestHeaders(UnityWebRequest request, RequestHeaderSetting[] additionalRequestHeaders)
        {
            foreach (var requestHeader in additionalRequestHeaders)
                request.SetRequestHeader(requestHeader.key, requestHeader.value);
        }
    }
}