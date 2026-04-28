using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ApiAzureStorageSecure.Helpers;
using ApiAzureStorageSecure.Models;
using ApiAzureStorageSecure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

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

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult> Login(LoginModel model)
        {
            Usuario usuario = await _repo.LoginAsync(
                model.Username,
                model.Password
            );

            if (usuario == null)
            {
                return Unauthorized(new { message = "Credenciales incorrectas." });
            }

            // 👇 MISMO ORDEN QUE EL SEGUNDO CONTROLLER
            SigningCredentials credentials = new SigningCredentials(
                _helper.GetKeyToken(),
                SecurityAlgorithms.HmacSha256
            );

            UsuarioModel usuarioModel = new UsuarioModel
            {
                IdUsuario = usuario.IdUsuario,
                Username = usuario.Username,
            };

            // Serializamos y ciframos
            string jsonUsuario = JsonConvert.SerializeObject(usuarioModel);
            string jsonCifrado = HelperCifrado.EncryptString(jsonUsuario);

            Claim[] informacion = new[]
            {
                new Claim("UserData", jsonCifrado),
                new Claim(ClaimTypes.Name, usuario.Username),
            };

            JwtSecurityToken token = new JwtSecurityToken(
                claims: informacion,
                issuer: _helper.Issuer,
                audience: _helper.Audience,
                signingCredentials: credentials,
                expires: DateTime.Now.AddHours(2),
                notBefore: DateTime.UtcNow
            );

            return Ok(new { response = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }
}