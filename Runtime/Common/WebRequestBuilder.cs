namespace PhEngine.Network
{
    public class WebRequestBuilder
    {
        public WebRequestBuilder(APICallConfig config, RequestHeaderSetting[] headerSettings, string accessToken)
        {
            Config = config;
            HeaderSettings = headerSettings;
            AccessToken = accessToken;
        }

        public APICallConfig Config { get; set; }
        public RequestHeaderSetting[] HeaderSettings { get; set; }
        public string AccessToken { get; set; }
    }
}