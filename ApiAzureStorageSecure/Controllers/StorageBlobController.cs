using ApiAzureStorageSecure.Helpers;
using ApiAzureStorageSecure.Models;
using ApiAzureStorageSecure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiAzureStorageSecure.Controllers
{
    /// <summary>
    /// API REST para Azure Blob Storage, protegida con Bearer Token.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requiere token para todos los métodos
    public class StorageBlobController : ControllerBase
    {
        private ServiceStorageBlobs service;
        private HelperUsuarioToken helperToken;

        public StorageBlobController(ServiceStorageBlobs service, HelperUsuarioToken helperToken)
        {
            this.service = service;
            this.helperToken = helperToken;
        }

        // ─── Contenedores ───────────────────────────────────────────────

        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult<List<string>>> Containers()
        {
            List<string> containers = await this.service.GetContainersAsync();
            return Ok(containers);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult> CreateContainer([FromQuery] string containerName)
        {
            await this.service.CreateContainerAsync(containerName);
            return Ok(new { message = $"Contenedor '{containerName}' creado correctamente." });
        }

        [HttpDelete]
        [Route("[action]")]
        public async Task<ActionResult> DeleteContainer([FromQuery] string containerName)
        {
            await this.service.DeleteContainerAsync(containerName);
            return Ok(new { message = $"Contenedor '{containerName}' eliminado." });
        }

        // ─── Blobs ──────────────────────────────────────────────────────

        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult<List<BlobModel>>> Blobs([FromQuery] string? containerName)
        {
            // Si no viene parámetro, intentamos listar el contenedor por defecto
            string targetContainer = containerName ?? "comics-imagenes";

            try
            {
                List<BlobModel> blobs = await this.service.GetBlobsAsync(targetContainer);
                return Ok(blobs);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "No se pudo listar el contenedor", message = ex.Message });
            }
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> DownloadBlob(
            [FromQuery] string containerName,
            [FromQuery] string blobName)
        {
            Stream stream = await this.service.GetBlobStreamAsync(containerName, blobName);
            string contentType = GetContentType(blobName);
            return File(stream, contentType, blobName);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult> UploadBlob(
            [FromQuery] string containerName,
            IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No se ha enviado ningún fichero." });

            using (Stream stream = file.OpenReadStream())
            {
                await this.service.UploadBlobAsync(containerName, file.FileName, stream);
            }

            // CORRECCIÓN: Usamos UsuarioModel (el tuyo) en lugar de Empleado
            UsuarioModel usuario = this.helperToken.GetUsuario();

            return Ok(new
            {
                message = $"Blob '{file.FileName}' subido al contenedor '{containerName}'.",
                subidoPor = usuario.Username, // O la propiedad que tenga tu modelo
            });
        }

        [HttpDelete]
        [Route("[action]")]
        public async Task<ActionResult> DeleteBlob(
            [FromQuery] string containerName,
            [FromQuery] string blobName)
        {
            await this.service.DeleteBlobAsync(containerName, blobName);
            return Ok(new { message = $"Blob '{blobName}' eliminado del contenedor '{containerName}'." });
        }

        // ─── Utilidad privada ────────────────────────────────────────────

        private string GetContentType(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                ".json" => "application/json",
                _ => "application/octet-stream",
            };
        }
    }
}