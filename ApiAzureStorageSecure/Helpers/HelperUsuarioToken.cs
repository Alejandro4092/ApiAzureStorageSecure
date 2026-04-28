using ApiAzureStorageSecure.Helpers;
using ApiAzureStorageSecure.Models;
using ApiAzureStorageSecure.Models;
using Newtonsoft.Json;
using System.Security.Claims;

namespace ApiAzureStorageSecure.Helpers
{
    public class HelperUsuarioToken
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public HelperUsuarioToken(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public UsuarioModel GetUsuario()
        {
            Claim claim = _contextAccessor.HttpContext!.User
                .FindFirst(z => z.Type == "UserData");

            string json = HelperCifrado.DecryptString(claim.Value);
            return JsonConvert.DeserializeObject<UsuarioModel>(json);
        }
    }
}