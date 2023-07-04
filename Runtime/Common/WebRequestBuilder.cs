namespace PhEngine.Network
{
    public class WebRequestBuilder
    {
        public WebRequestBuilder(APICallConfig config, RequestHeaderSetting[] headerSettings, string accessToken, AccessTokenValidator accessTokenValidator)
        {
            Config = config;
            HeaderSettings = headerSettings;
            AccessToken = accessToken;
            AccessTokenValidator = accessTokenValidator;
        }

        public APICallConfig Config { get; set; }
        public RequestHeaderSetting[] HeaderSettings { get; set; }
        public string AccessToken { get; set; }
        public AccessTokenValidator AccessTokenValidator { get; }
    }
}