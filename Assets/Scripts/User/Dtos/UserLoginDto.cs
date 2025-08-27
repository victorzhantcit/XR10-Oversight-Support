using Newtonsoft.Json;

namespace User.Dtos
{
    public class UserLoginIBMSPlatformDto
    {
        [JsonProperty("account")]
        public string Id = string.Empty;
        [JsonProperty("pw")]
        public string Password = string.Empty;

        [JsonConstructor]
        public UserLoginIBMSPlatformDto() { }

        public UserLoginIBMSPlatformDto(string id, string password)
        {
            this.Id = id;
            this.Password = password;
        }

        public string Print() => $"Id : {Id}, Password: {Password}";
    }

    public class UserLoginIBMSAppDto
    {
        [JsonProperty("id")]
        public string Id = string.Empty;
        [JsonProperty("password")]
        public string Password = string.Empty;
        [JsonProperty("name")]
        public string Name = string.Empty;
        [JsonProperty("department")]
        public string Department = string.Empty;
        [JsonProperty("tel")]
        public string Tel = string.Empty;
        [JsonProperty("email")]
        public string Email = string.Empty;

        public void Init(string id, string password)
        {
            this.Id = id;
            this.Password = password;
        }
    }
}
