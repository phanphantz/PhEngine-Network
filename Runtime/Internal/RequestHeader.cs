using System;

namespace PhEngine.Network
{
    [Serializable]
    public class RequestHeader
    {
        public string key;
        public string value;

        public RequestHeader(RequestHeader setting)
        {
            key = setting.key;
            value = setting.value;
        }

        public RequestHeader(string key, string value)
        {
            this.key = key;
            this.value = value;
        }
    }
}