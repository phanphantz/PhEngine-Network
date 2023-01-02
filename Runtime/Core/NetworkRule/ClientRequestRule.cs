using System;

namespace PhEngine.Network
{
    [Serializable]
    public class ClientRequestRule
    {
        public string accessTokenFieldName = "accessToken";
        public string accessTokenPrefix = "bearer ";
        public RequestHeaderSetting[] additionalRequestHeaders;
    }
}