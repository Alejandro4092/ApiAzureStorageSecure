namespace ApiAzureStorageSecure.Models
{
    // Este objeto se serializa a JSON, se CIFRA con AES
    // y se mete en el claim "UserData" del JWT.
    // No mapea a ninguna tabla.
    public class UsuarioModel
    {
        public int IdUsuario { get; set; }
        public string Username { get; set; }
    }
}