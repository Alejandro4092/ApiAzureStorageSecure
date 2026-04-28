using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ApiAzureStorageSecure.Models;

namespace ApiAzureStorageSecure.Services
{
    /// <summary>
    /// Servicio de Azure Blob Storage optimizado para trabajar con Contenedores y Blobs.
    /// </summary>
    public class ServiceStorageBlobs
    {
        private readonly BlobServiceClient _client;
        private readonly string _containerName;

        public ServiceStorageBlobs(BlobServiceClient client, IConfiguration configuration)
        {
            _client = client;
            // Busca en AzureKeys:ContainerName (como en tu appsettings) o usa el default
            _containerName = configuration.GetValue<string>("AzureKeys:ContainerName")
                             ?? "comics-imagenes";
        }

        // ─── MÉTODOS DE CONTENEDORES (Para solucionar tus errores) ────────────────

        public async Task<List<string>> GetContainersAsync()
        {
            List<string> containers = new();
            await foreach (BlobContainerItem container in _client.GetBlobContainersAsync())
            {
                containers.Add(container.Name);
            }
            return containers;
        }

        public async Task CreateContainerAsync(string containerName)
        {
            // Azure requiere que los nombres sean en minúsculas
            await _client.CreateBlobContainerAsync(containerName.ToLower(), PublicAccessType.Blob);
        }

        public async Task DeleteContainerAsync(string containerName)
        {
            await _client.DeleteBlobContainerAsync(containerName);
        }

        // ─── MÉTODOS DE BLOBS ─────────────────────────────────────────────────────

        /// <summary>
        /// Devuelve la URL pública de un blob.
        /// </summary>
        public string GetBlobUrl(string blobName)
        {
            BlobContainerClient container = _client.GetBlobContainerClient(_containerName);
            BlobClient blob = container.GetBlobClient(blobName);
            return blob.Uri.AbsoluteUri;
        }

        /// <summary>
        /// Lista los objetos BlobModel de un contenedor específico.
        /// </summary>
        public async Task<List<BlobModel>> GetBlobsAsync(string containerName)
        {
            BlobContainerClient containerClient = _client.GetBlobContainerClient(containerName);
            List<BlobModel> models = new();
            await foreach (BlobItem item in containerClient.GetBlobsAsync())
            {
                BlobClient blobClient = containerClient.GetBlobClient(item.Name);
                models.Add(new BlobModel
                {
                    Nombre = item.Name,
                    Container = containerName,
                    Url = blobClient.Uri.AbsoluteUri
                });
            }
            return models;
        }

        public async Task UploadBlobAsync(string containerName, string blobName, Stream stream)
        {
            BlobContainerClient containerClient = _client.GetBlobContainerClient(containerName);
            await containerClient.UploadBlobAsync(blobName, stream);
        }

        public async Task DeleteBlobAsync(string containerName, string blobName)
        {
            BlobContainerClient containerClient = _client.GetBlobContainerClient(containerName);
            await containerClient.DeleteBlobAsync(blobName);
        }

        public async Task<Stream> GetBlobStreamAsync(string containerName, string blobName)
        {
            BlobContainerClient containerClient = _client.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            var response = await blobClient.DownloadAsync();
            return response.Value.Content;
        }
    }
}