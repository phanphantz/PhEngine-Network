using System;

namespace PhEngine.Network
{
    [Serializable]
    public class ClientError
    {
        public string message;
        public int code;
        public FailureHandling failureHandling;

        public ClientError(ServerResult result)
        {
            message = result.message;
            code = result.code;
            failureHandling = result.failureHandling;
        }

        public ClientError(string message, int code, FailureHandling failureHandling)
        {
            this.message = message;
            this.code = code;
            this.failureHandling = failureHandling;
        }

        public override string ToString()
        {
            return $"Client Fail : {message} (Code: {code})";
        }
    }
}