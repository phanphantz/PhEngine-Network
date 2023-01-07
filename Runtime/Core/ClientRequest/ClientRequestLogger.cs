using System.Text;
using PhEngine.JSON;
using UnityEngine;
using UnityEngine.Networking;

namespace PhEngine.Network
{
    public class ClientRequestLogger
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
            var startWebRequestLog = GetStartRequestLog();
            Log(startWebRequestLog);
        }
        
        string GetStartRequestLog()
        {
            var stringBuilder = GetEndpointLogTitle(ClientRequest, ClientRequestLogType.START);
            TryAppendRequestBody(stringBuilder);
            AppendURL(stringBuilder);
            return stringBuilder.ToString();
        }

        void TryAppendRequestBody(StringBuilder stringBuilder)
        {
            stringBuilder.Append($"Parameter Type: {ClientRequest.ParameterType}\n");
            var json = ClientRequest.Content;
            if (json != null && !json.IsNull)
                AppendRequestBody(stringBuilder, json);
        }

        static void AppendRequestBody(StringBuilder stringBuilder, JSONObject json)
        {
            stringBuilder.Append("Request Body: \n");
            stringBuilder.Append(json.Print(true));
            stringBuilder.Append("\n\n");
        }

        void AppendURL(StringBuilder stringBuilder)
        {
            stringBuilder.Append("URL: ");
            stringBuilder.Append(UnityWebRequest.url);
            stringBuilder.Append("\n");
        }

        public void LogConnectionFail(ServerResult result)
        {
            var log = GetConnectionFailLog(result);
            LogError(log);
        }

        string GetConnectionFailLog(ServerResult result)
            => GetResultLog(result, ClientRequestLogType.CONNECTION_FAIL, UnityWebRequest.downloadHandler.text);

        string GetResultLog(ServerResult result, string logType, string body)
        {
            var stringBuilder = GetEndpointLogTitle(ClientRequest, logType);
            AppendDetailAndError(result, stringBuilder, body);
            return stringBuilder.ToString();
        }

        void AppendDetailAndError(ServerResult result, StringBuilder stringBuilder, string response)
        {
            stringBuilder.Append($"Code: {result.code}\n");
            TryAppendError(result, stringBuilder);
            stringBuilder.Append($"Body: \n{response}\n\n");
        }

        void TryAppendError(ServerResult result, StringBuilder stringBuilder)
        {
            if (result.status != ServerResultStatus.ServerReturnSuccess)
                stringBuilder.Append($"Error: {UnityWebRequest.error}\n");
        }

        public void LogServerFail(ServerResult result)
        {
            var log = GetServerFailLog(result);
            LogError(log);
        }
        
        string GetServerFailLog(ServerResult result)
            => GetResultLog(result, ClientRequestLogType.SERVER_FAIL, GetResultJsonString(result));

        public void LogSuccess(ServerResult result)
        {
            var log = GetSuccessLog(result);
            Log(log);
        }

        string GetSuccessLog(ServerResult result)
            => GetResultLog(result, ClientRequestLogType.SUCCESS, GetResultJsonString(result));

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
        
        string GetResultJsonString(ServerResult result) =>  result.fullJson == null ? "null" : result.fullJson.Print(true);
    }
}