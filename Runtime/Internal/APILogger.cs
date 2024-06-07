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
        APILogOption LogOption { get; }
        public bool IsUsePrettyFormat => LogOption == APILogOption.Pretty;
        
        public APILogger(ClientRequest clientRequest, UnityWebRequest unityWebRequest, APILogOption logOption)
        {
            ClientRequest = clientRequest;
            UnityWebRequest = unityWebRequest;
            LogOption = logOption;
        }
        
        public void LogStartRequest()
        {
            var stringBuilder = GetEndpointLogTitle(ClientRequest, GetStartKeyword(LogOption));
            var content = ClientRequest.Content;
            if (content && ClientRequest.ParameterType == ParameterType.Body && (int)LogOption >= 1)
            {
                stringBuilder.Append("\n");
                stringBuilder.Append(content.Print(IsUsePrettyFormat));
                stringBuilder.Append("\n\n");
            }

            if ((int) LogOption >= 1)
            {
                stringBuilder.Append("URL: ");
                stringBuilder.Append(UnityWebRequest.url);
                
                if (ClientRequest.HeaderList.Count > 0)
                {
                    stringBuilder.Append("\n\nHeaders :\n");
                    foreach (var header in ClientRequest.HeaderList)
                        stringBuilder.Append($"{header.key} : {header.value}\n");
                }
            }

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
            => GetResultLog(result, GetConnectionFailKeyword(result.httpStatus, LogOption), UnityWebRequest.downloadHandler.text);
        
        string GetServerFailLog(ServerResult result)
            => GetResultLog(result, GetServerFailKeyword(LogOption), GetResultJsonString(result));
        
        string GetSuccessLog(ServerResult result)
            => GetResultLog(result, GetSuccessKeyword(LogOption), GetResultJsonString(result));

        string GetResultLog(ServerResult result, string logType, string body)
        {
            var stringBuilder = GetEndpointLogTitle(ClientRequest, logType, result.code);
            if (result.status != ServerResultStatus.ServerReturnSuccess && result.status != ServerResultStatus.ClientFail)
                stringBuilder.Append($"{UnityWebRequest.error}\n");
            
            if ((int)LogOption >= 1)
                stringBuilder.Append($"{body}\n\n");
            
            return stringBuilder.ToString();
        }
        
        StringBuilder GetEndpointLogTitle(ClientRequest request, string logType, int? code = null)
        {
            var stringBuilder = new StringBuilder();
            if (request.MockMode != MockMode.Off)
                stringBuilder.Append(GetMockKeyword(LogOption));

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
            if (result.isMocked && result.fullJson == null)
                return result.dataJson == null ? "null" : result.dataJson.Print(IsUsePrettyFormat);
            
            return result.fullJson == null ? "null" : result.fullJson.Print(IsUsePrettyFormat);
        }
        
        public static string GetServerFailKeyword(APILogOption option) => option == APILogOption.Pretty ? "<color=red><b>SERVER FAIL</b></color>" : "SERVER FAIL";
        public static string GetSuccessKeyword(APILogOption option) => option == APILogOption.Pretty ? "<color=green><b>SUCCESS</b></color>" : "SUCCESS";
        public static string GetMockKeyword(APILogOption option) => option == APILogOption.Pretty ? "<color=yellow><b>MOCK </b></color>" : "MOCK ";

        public static string GetConnectionFailKeyword(HttpStatusCode status, APILogOption option)
        {
            var errorName = status.ToString();
            if ((int) status == 0)
                errorName = "UnknownError";
            
            if ( option == APILogOption.Pretty)
                return $"<color=red><b>{errorName}</b></color>";
            
            return errorName;
        }
        public static string GetStartKeyword(APILogOption option) => option == APILogOption.Pretty ? "<b>START...</b>" : "START...";
    }
}