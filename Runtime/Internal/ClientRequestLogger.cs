using System.Text;
using PhEngine.JSON;
using UnityEngine;
using UnityEngine.Networking;

namespace PhEngine.Network
{
    internal class ClientRequestLogger
    {
        ClientRequest ClientRequest { get; }
        UnityWebRequest UnityWebRequest { get; }

        public ClientRequestLogger(ClientRequest clientRequest, UnityWebRequest unityWebRequest)
        {
            ClientRequest = clientRequest;
            UnityWebRequest = unityWebRequest;
        }
        
        public void LogStartRequest()
        {
            var stringBuilder = GetEndpointLogTitle(ClientRequest, ClientRequestLogType.START);
            if (ClientRequest.ParameterType == ParameterType.Body)
            {
                stringBuilder.Append("Request Body: \n");
                stringBuilder.Append(ClientRequest.Content.Print(true));
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
            => GetResultLog(result, ClientRequestLogType.CONNECTION_FAIL, UnityWebRequest.downloadHandler.text);

        string GetServerFailLog(ServerResult result)
            => GetResultLog(result, ClientRequestLogType.SERVER_FAIL, GetResultJsonString(result));
        
        string GetSuccessLog(ServerResult result)
            => GetResultLog(result, ClientRequestLogType.SUCCESS, GetResultJsonString(result));

        string GetResultLog(ServerResult result, string logType, string body)
        {
            var stringBuilder = GetEndpointLogTitle(ClientRequest, logType);
            stringBuilder.Append($"Code: {result.code}\n");
            if (result.status != ServerResultStatus.ServerReturnSuccess)
                stringBuilder.Append($"Error: {UnityWebRequest.error}\n");
            
            stringBuilder.Append($"Body: \n{body}\n\n");
            return stringBuilder.ToString();
        }
        
        static StringBuilder GetEndpointLogTitle(ClientRequest request, string logType)
        {
            var stringBuilder = new StringBuilder();
            if (request.DebugMode != NetworkDebugMode.Off)
                stringBuilder.Append(ClientRequestLogType.MOCK);

            stringBuilder.Append("<b>[");
            stringBuilder.Append(request.Verb.ToString());
            stringBuilder.Append("]</b> ");
            stringBuilder.Append(request.Destination);
            stringBuilder.Append(" => ");
            stringBuilder.Append(logType);
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
                return result.dataJson == null ? "null" : result.dataJson.Print(true);
            
            return result.fullJson == null ? "null" : result.fullJson.Print(true);
        }
    }
    
    public static class ClientRequestLogType
    {
        public const string SERVER_FAIL = "<color=red><b>SERVER FAIL</b></color>";
        public const string SUCCESS = "<color=green><b>SUCCESS!</b></color>";
        public const string MOCK = "<color=yellow><b>MOCK</b></color> ";
        public const string CONNECTION_FAIL = "<color=red><b>CONNECTION FAIL</b></color>";
        public const string START = "<b>START...</b>";
    }
}