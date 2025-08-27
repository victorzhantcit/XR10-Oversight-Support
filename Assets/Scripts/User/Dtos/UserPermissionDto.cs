using Newtonsoft.Json;

namespace User.Dtos
{
    public class UserPermissionDto
    {
        [JsonProperty("personal")]
        public PersonalDto Personal { get; set; }
    }

    public class PersonalDto
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
    }
}
