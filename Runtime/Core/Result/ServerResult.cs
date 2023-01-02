using System;
using PhEngine.JSON;

namespace PhEngine.Network
{
    [Serializable]
    public class ServerResult
    {
        public int code;
        public JSONObject fullJson;
        public JSONObject dataJson;
        public string message;
        public string dateTime;
        public ServerResultStatus status;

        public ServerResult() {}

        public ServerResult(int code, ServerResultStatus status)
        {
            this.code = code;
            this.status = status;
        }

        public ServerResult(ServerResultStatus status)
        {
            this.status = status;
        }
        
        public ServerResult(int code, JSONObject fullJson, JSONObject dataJson, string message, string dateTime, ServerResultStatus status)
        {
            this.code = code;
            this.fullJson = fullJson;
            this.dataJson = dataJson;
            this.message = message;
            this.dateTime = dateTime;
            this.status = status;
        }
    }
}