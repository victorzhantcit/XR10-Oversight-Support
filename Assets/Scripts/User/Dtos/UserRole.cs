namespace User.Dtos
{
    public enum UserRole
    {
        Inspector,
        Worker,
        Default
    }

    public static class UserRoleConvert 
    {
        public static UserRole GetRoleEnum(string userRole)
        {
            if (string.IsNullOrWhiteSpace(userRole))
                return UserRole.Default;

            return userRole.ToLower() switch
            {
                "worker" => UserRole.Worker,
                "insp" => UserRole.Inspector,
                _ => UserRole.Default
            };
        }
    }
}
