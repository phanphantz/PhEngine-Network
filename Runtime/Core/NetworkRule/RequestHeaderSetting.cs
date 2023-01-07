using System;

namespace PhEngine.Network
{
    [Serializable]
    public class RequestHeaderSetting
    {
        public string key;
        public string value;

        public RequestHeaderSetting(RequestHeaderSetting setting)
        {
            CopyFrom(setting);
        }
        
        public void CopyFrom(RequestHeaderSetting newSetting)
        {
            key = newSetting.key;
            value = newSetting.value;
        }
    }
}