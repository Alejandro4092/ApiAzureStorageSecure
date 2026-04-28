namespace ApiAzureStorageSecure.Models
{
    // Representa un blob listado desde Azure Blob Storage
    public class BlobModel
    {
        public string Nombre { get; set; }
        public string Container { get; set; }
        public string Url { get; set; }
    }
}
