using System.Security.Cryptography;
using System.Text;

namespace ApiAzureStorageSecure.Helpers
{
    public static class HelperCifrado
    {
        private static readonly byte[] _key;

        static HelperCifrado()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfiguration configuration = builder.Build();

            string keyStr =
                configuration.GetValue<string>("Cifrado:Key")
                ?? throw new InvalidOperationException("Falta 'Cifrado:Key' en appsettings.");

            _key = Encoding.UTF8.GetBytes(keyStr.PadRight(32).Substring(0, 32));
        }

        // Cifra texto plano → "ivBase64:cipherBase64"
        public static string EncryptString(string plainText)
        {
            using Aes aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();
            using ICryptoTransform enc = aes.CreateEncryptor();
            byte[] cipher = enc.TransformFinalBlock(
                Encoding.UTF8.GetBytes(plainText), 0,
                Encoding.UTF8.GetBytes(plainText).Length);
            return $"{Convert.ToBase64String(aes.IV)}:{Convert.ToBase64String(cipher)}";
        }

        // Descifra "ivBase64:cipherBase64" → texto plano
        public static string DecryptString(string encryptedText)
        {
            string[] parts = encryptedText.Split(':');
            using Aes aes = Aes.Create();
            aes.Key = _key;
            aes.IV = Convert.FromBase64String(parts[0]);
            using ICryptoTransform dec = aes.CreateDecryptor();
            byte[] plain = dec.TransformFinalBlock(
                Convert.FromBase64String(parts[1]), 0,
                Convert.FromBase64String(parts[1]).Length);
            return Encoding.UTF8.GetString(plain);
        }
    }
}