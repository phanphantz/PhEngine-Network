using System;

namespace PhEngine.Network
{
    [Serializable]
    public class ServerResultRule
    {
        public StatusCodeRange[] successStatusCodeRanges;
        public string messageFieldName = "message";
        public string dataFieldName = "data";
        public string statusCodeFieldName = "statusCode";
        public string currentDateTimeFieldName = "currentDateTime";
    }
    
}