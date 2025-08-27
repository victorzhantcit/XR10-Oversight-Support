using System;

namespace User.Dtos
{
    [Serializable]
    public class UserData
    {
        public string Id = string.Empty;
        public string Password = string.Empty;
        public string Pin = string.Empty;
        public UserRole Role;

        public void Setup(string id, string password, string pin, UserRole role)
        {
            Id = id;
            Password = password;
            Pin = pin;
            Role = role;
        }
    }
}
