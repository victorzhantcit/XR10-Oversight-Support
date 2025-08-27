using Newtonsoft.Json;

namespace AutodeskPlatformService.Dtos
{
    public class APSAuthDto
    {
        [JsonProperty("token_type")]
        public string TokenType;

        [JsonProperty("expires_in")]
        public string ExpireIn;

        [JsonProperty("access_token")]
        public string AccessToken;
    }
}
