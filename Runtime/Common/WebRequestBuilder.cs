namespace PhEngine.Network
{
    public class WebRequestBuilder
    {
        public WebRequestBuilder(APICallConfig config, NetworkRuleConfig networkRuleConfig, RequestHeaderSetting[] headerSettings, string accessToken)
        {
            Config = config;
            NetworkRuleConfig = networkRuleConfig;
            HeaderSettings = headerSettings;
            AccessToken = accessToken;
        }

        public APICallConfig Config { get; set; }
        public NetworkRuleConfig NetworkRuleConfig { get; set; }
        public RequestHeaderSetting[] HeaderSettings { get; set; }
        public string AccessToken { get; set; }
    }
}