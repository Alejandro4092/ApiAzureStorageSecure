using ApiAzureStorageSecure.Helpers;
using ApiAzureStorageSecure.Models;
using ApiAzureStorageSecure.Repositories;
using ApiAzureStorageSecure.Helpers;
using ApiAzureStorageSecure.Models;
using ApiAzureStorageSecure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ApiAzureStorageSecure.Helpers;


namespace ApiAzureStorageSecure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RepositoryComics _repo;
        private readonly HelperActionOAuthService _helper;

        public AuthController(RepositoryComics repo, HelperActionOAuthService helper)
        {
            _repo = repo;
            _helper = helper;
        }

        /// <summary>
        /// POST /api/auth/login
        /// Body: { "username": "admin", "password": "1234" }
        /// Devuelve un JWT Bearer Token con el UsuarioModel cifrado en el claim "UserData".
        /// </summary>
        [HttpPost("[action]")]
        public async Task<ActionResult> Login(LoginModel model)
        {
            Usuario usuario = await _repo.LoginAsync(model.Username, model.Password);

            if (usuario == null)
                return Unauthorized(new { message = "Credenciales incorrectas." });

            SigningCredentials credentials = new SigningCredentials(
                _helper.GetKeyToken(), SecurityAlgorithms.HmacSha256);

            // Construimos el modelo que irá cifrado en el token
            UsuarioModel usuarioModel = new UsuarioModel
            {
                IdUsuario = usuario.IdUsuario,
                Username = usuario.Username,
            };

            // Serializamos y CIFRAMOS el modelo
            string jsonCifrado = HelperCifrado.EncryptString(
                JsonConvert.SerializeObject(usuarioModel));

            // Claims del token
            Claim[] claims = new[]
            {
                new Claim("UserData", jsonCifrado),
                new Claim(ClaimTypes.Name, usuario.Username),
            };

            SigningCredentials credentials = new SigningCredentials(
                _helper.GetKeyToken(), SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _helper.Issuer,
                audience: _helper.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            return Ok(new { response = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }
}