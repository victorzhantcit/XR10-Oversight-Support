using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Extensions;
using User.Dtos;

namespace User.Core
{
    public static class AuthService
    {
        private static string _iBMSPlatformServer = string.Empty;

        public static string LoginAPI_iBMSPlatform => $"{_iBMSPlatformServer}/api/Auth/Login";

        private static string PermAPI_iBMSPlatform => $"{_iBMSPlatformServer}/api/Auth/Login/Permission";

        public static void Initialize(string iBMSPlatformServer)
        {
            _iBMSPlatformServer = iBMSPlatformServer;
            APIHelper.RegisterLoginAPI(LoginAPI_iBMSPlatform);
        }

        public static async Task<bool> LoginUserOnIBMSPlatform(UserLoginIBMSPlatformDto userData)
        {
            var responseToken = await APIHelper.SendFormRequestAsync<string>(
                url: LoginAPI_iBMSPlatform,
                method: HttpMethod.POST,
                data: new List<KeyValue>
                {
                    new KeyValue("account", userData.Id),
                    new KeyValue("pw", userData.Password)
                },
                returnPureString: true
            );

            if (responseToken.IsSuccess)
                APIHelper.RegisterLoginAPI(LoginAPI_iBMSPlatform, responseToken.Data);

            return responseToken.IsSuccess;
        }

        public static async Task<UserPermissionDto> GetUserPermissionOnOnIBMSPlatform()
            => await APIHelper.SendServerFormRequestAsync<UserPermissionDto>(PermAPI_iBMSPlatform, HttpMethod.GET);
    }
}
