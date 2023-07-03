using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace PhEngine.Network
{
    internal class APILogger
    {
        ClientRequest ClientRequest { get; }
        UnityWebRequest UnityWebRequest { get; }
        public static bool IsUsePrettyFormat => Application.isEditor;
        public APILogger(ClientRequest clientRequest, UnityWebRequest unityWebRequest)
        {
            ClientRequest = clientRequest;
            UnityWebRequest = unityWebRequest;
        }
        
        public void LogStartRequest()
        {
            var stringBuilder = GetEndpointLogTitle(ClientRequest, APILogKeyword.Start);
            if (ClientRequest.ParameterType == ParameterType.Body)
            {
                stringBuilder.Append("\n");
                stringBuilder.Append(ClientRequest.Content.Print(IsUsePrettyFormat));
                stringBuilder.Append("\n\n");
            }

            stringBuilder.Append("URL: ");
            stringBuilder.Append(UnityWebRequest.url);
            stringBuilder.Append("\n");
            
            Log(stringBuilder.ToString());
        }
        
        public void LogConnectionFail(ServerResult result)
        {
            var log = GetConnectionFailLog(result);
            LogError(log);
        }

        public void LogServerFail(ServerResult result)
        {
            var log = GetServerFailLog(result);
            LogError(log);
        }
        
        public void LogSuccess(ServerResult result)
        {
            var log = GetSuccessLog(result);
            Log(log);
        }
        
        string GetConnectionFailLog(ServerResult result)
            => GetResultLog(result, APILogKeyword.ConnectionFail(result.HttpStatus), UnityWebRequest.downloadHandler.text);

        string GetServerFailLog(ServerResult result)
            => GetResultLog(result, APILogKeyword.ServerFail, GetResultJsonString(result));
        
        string GetSuccessLog(ServerResult result)
            => GetResultLog(result, APILogKeyword.Success, GetResultJsonString(result));

        string GetResultLog(ServerResult result, string logType, string body)
        {
            var stringBuilder = GetEndpointLogTitle(ClientRequest, logType, result.code);
            if (result.status != ServerResultStatus.ServerReturnSuccess)
                stringBuilder.Append($"{UnityWebRequest.error}\n");
            
            stringBuilder.Append($"{body}\n\n");
            return stringBuilder.ToString();
        }
        
        static StringBuilder GetEndpointLogTitle(ClientRequest request, string logType, int? code = null)
        {
            var stringBuilder = new StringBuilder();
            if (request.DebugMode != NetworkDebugMode.Off)
                stringBuilder.Append(APILogKeyword.Mock);

            if (IsUsePrettyFormat)
                stringBuilder.Append("<b>[");
           
            stringBuilder.Append(request.Verb.ToString());
            
            if (IsUsePrettyFormat)
                stringBuilder.Append("]</b>");
           
            stringBuilder.Append(" ");
            stringBuilder.Append(request.Destination);
            stringBuilder.Append(" => ");
            stringBuilder.Append(logType);
            if (code.HasValue)
            {
                stringBuilder.Append(" ");
                
                if (IsUsePrettyFormat)
                    stringBuilder.Append(" <b>");
                
                stringBuilder.Append(code.Value);
                
                if (IsUsePrettyFormat)
                    stringBuilder.Append("</b>");
            }
            stringBuilder.Append("\n");
            return stringBuilder;
        }
        
        void Log(string message)
        {
            Debug.Log(message);
        }
        
        void LogError(string message)
        {
            Debug.LogError(message);
        }

        string GetResultJsonString(ServerResult result)
        {
            if (result.isMocked)
                return result.dataJson == null ? "null" : result.dataJson.Print(IsUsePrettyFormat);
            
            return result.fullJson == null ? "null" : result.fullJson.Print(IsUsePrettyFormat);
        }
    }
    
    public static class APILogKeyword
    {
        public static string ServerFail => APILogger.IsUsePrettyFormat ? "<color=red><b>SERVER FAIL</b></color>" : "SERVER FAIL";
        public static string Success => APILogger.IsUsePrettyFormat ? "<color=green><b>SUCCESS</b></color>" : "SUCCESS";
        public static string Mock => APILogger.IsUsePrettyFormat ? "<color=yellow><b>MOCK </b></color>" : "MOCK ";

        public static string ConnectionFail(HttpStatusCode status)
        {
            if (APILogger.IsUsePrettyFormat)
                    return $"<color=red><b>{status.ToString().ToUpper()}</b></color>";
            
            return status.ToString().ToUpper();
        }
        
        public static string Start => APILogger.IsUsePrettyFormat ? "<b>START...</b>" : "START...";
    }
}